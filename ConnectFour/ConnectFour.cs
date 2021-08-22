using System;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using BoardGameLearner;
using System.Collections.Generic;
using DeepLearning;

namespace ConnectFour
{
	public class ConnectFour : IBoardGame<int>
	{
		private readonly PlayerID _firstPlayer;
		private readonly PlayerID _secondPlayer;
		private PlayerID _currentPlayer;
		private Cell[,] _board;
		private readonly Stack<(PlayerID player, Coord coord)> _moveHistory = new(capacity: 20);

		public ConnectFour(PlayerID firstPlayer, PlayerID secondPlayer, int rows = 6, int columns = 7)
		{
			_firstPlayer = firstPlayer;
			_secondPlayer = secondPlayer;
			_currentPlayer = _firstPlayer;
			_board = BlankBoard(rows, columns);
		}

		public void MakeMove(PlayerID player, int col)
        {
			/*
			if (player != _currentPlayer && !overrideTurnCheck)
				throw new ArgumentException($"{player} is not the current player"); */

			if (!ValidMoves(player).Contains(col))
				throw new ArgumentException($"{player} is the current player, but move {col} is invalid for {player}");

			for (int r = 0; r < _board.GetLength(0); r++)
            {
				if (_board[r, col].IsEmpty && (r == _board.GetLength(0) - 1 || !_board[r+1, col].IsEmpty))
                {
					_board[r, col].PlayerId = player;
					_moveHistory.Push((player, new Coord(col, r)));
					_currentPlayer = OtherPlayer(_currentPlayer);
					return;
				}
            }
        }

		public PlayerID Winner()
			=> 
			//left -> right
			SearchForWin(_board, 
				start: (0, 0), 
				end: (_board.GetLength(1) - 3, _board.GetLength(0)),
				step: (1, 0)) 
			?? 
			// up -> down
			SearchForWin(_board, 
				start: (0, 0), 
				end: (_board.GetLength(1), _board.GetLength(0) - 3), 
				step: (0, 1)) 
			??	
			// top left -> bottom right
			SearchForWin(_board, 
				start: (0, 0), 
				end: (_board.GetLength(1) - 3, _board.GetLength(0) - 3), 
				step: (1, 1)) 
			?? 
			// bottom left -> top right
			SearchForWin(_board, 
				start: (0, 4), 
				end: (_board.GetLength(1) - 3, _board.GetLength(0)),
				step: (1, -1))
			??
			null;

		public void Reset()
		{
			_currentPlayer = _firstPlayer;
			_board = BlankBoard(_board.GetLength(0), _board.GetLength(1));
		}

		public List<int> ValidMoves(PlayerID player)
		{
			return Enumerable
				.Range(0, _board.GetLength(1))
				.Where(col => _board[0, col].IsEmpty)
				.ToList();
		}

		public double Value(PlayerID player)
        {
			PlayerID opponent = OtherPlayer(player);
			return ValueForPlayer(_board, player, opponent) - ValueForPlayer(_board, opponent, player);
        }

		public void UndoLastMove()
		{
			if (!_moveHistory.Any())
				throw new Exception($"No move has been made to undo");

			(PlayerID prevPlayer, Coord prevMove) = _moveHistory.Pop();
			_board[prevMove.Y, prevMove.X].PlayerId = null;
			_currentPlayer = _moveHistory.Peek().player;
		}

		public bool IsGameOver()
			=> IsDraw() || Winner() != null;

		public bool HasWon(PlayerID player)
			=> Winner() == player;

		public bool HasLost(PlayerID player)
			=> Winner() != null && Winner() != player;

		public bool IsDraw()
		{
			for (int r = 0; r < _board.GetLength(0); r++)
            {
				if (_board[r, 0].IsEmpty)
					return false;
            }
			return true;
		}

		public Vector<double> ToInputLayer(PlayerID player)
		{
			List<double> elements = new(capacity: 3 * _board.Length);
			foreach (Cell cell in _board)
				elements.AddRange(cell.OneHotEncode(_firstPlayer, _secondPlayer));
			return Vector<double>.Build.DenseOfEnumerable(elements);
		}

		private static PlayerID SearchForWin(Cell[,] board, (int, int) start, (int, int) end, (int, int) step)
			=> SearchForWin(board, new Coord(start), new Coord(end), new Coord(step));

		private static PlayerID SearchForWin(Cell[,] board, Coord start, Coord end, Coord step)
        {
			for (int r = start.Y; r < end.Y; r++)
            {
				for (int c = start.X; c < end.X; c++)
                {
					if (!board[r, c].IsEmpty)
                    {
						PlayerID player = board[r, c].PlayerId;
						if (Line(board, start: new(c, r), step).All(cell => cell.PlayerId == player))
							return player;
                    }
                }
            }
			return null;
        }

		private PlayerID OtherPlayer(PlayerID player)
			=> player == _firstPlayer ? _secondPlayer : _firstPlayer;

		private static double ValueForPlayer(Cell[,] board, PlayerID player, PlayerID opponent)
			=> SearchForValue(board, player, opponent,
				start: (0, 0),
				end: (board.GetLength(1) - 3, board.GetLength(0)),
				step: (1, 0))
			+
			// up -> down
			SearchForValue(board, player, opponent,
				start: (0, 0),
				end: (board.GetLength(1), board.GetLength(0) - 3),
				step: (0, 1))
			+
			// top left -> bottom right
			SearchForValue(board, player, opponent,
				start: (0, 0),
				end: (board.GetLength(1) - 3, board.GetLength(0) - 3),
				step: (1, 1))
			+
			// bottom left -> top right
			SearchForValue(board, player, opponent,
				start: (0, 4),
				end: (board.GetLength(1) - 3, board.GetLength(0)),
				step: (1, -1));

		private static double SearchForValue(Cell[,] board, PlayerID player, PlayerID opponent, (int, int) start, (int, int) end, (int, int) step)
			=> SearchForValue(board, player, opponent, new Coord(start), new Coord(end), new Coord(step));

		private static double SearchForValue(Cell[,] board, PlayerID player, PlayerID opponent, Coord start, Coord end, Coord step)
        {
			double value = 0;
			double fourInARowValue = 10e4;
			double threeInARowValue = 10e2;
			double twoInARowValue = 1;

			for (int r = start.Y; r < end.Y; r++)
            {
				for (int c = start.X; c < end.X; c++)
                {
					var line = Line(board, start: new(c, r), step);
					if (line.Any(cell => cell.PlayerId == player) && !line.Any(cell => cell.PlayerId == opponent))
                    {
						bool lineIsSupported = true;
						for (int s = 0; s < 4; s++)
                        {
							Coord coordUnderneath = new Coord(c, r - 1) + s * step;
							if (coordUnderneath.Y >= 0 && At(board, coordUnderneath).IsEmpty)
                            {
								lineIsSupported = false;
								break;
							}
                        }
						if (lineIsSupported)
                        {
							int playersCells = line.Where(cell => cell.PlayerId == player).Count();
							value += playersCells switch
							{
								4 => fourInARowValue,
								3 => threeInARowValue,
								2 => twoInARowValue,
								_ => 0
							};
						}
					}
                }
            }
			return value;
        }

        public override string ToString()
        {
			string header = "|" + string.Join('|', Enumerable.Range(1, _board.GetLength(1))) + "|\n";
			string line = new('-', count: 2 * _board.GetLength(1) + 1);
			StringBuilder builder = new(header);
			builder.AppendLine(line);

			for (int r = 0; r < _board.GetLength(0); r++)
            {
				builder.Append('|');
				for (int c = 0; c < _board.GetLength(1); c++)
                {
					char cell = ' ';
					if (_board[r, c].PlayerId == _firstPlayer)
						cell = 'O';
					else if (_board[r, c].PlayerId == _secondPlayer)
						cell = 'X';
					builder.Append(cell);
					builder.Append('|');
                }
				builder.AppendLine();
				builder.AppendLine(line);
            }
			return builder.ToString();
        }

		private static Cell[,] BlankBoard(int rows, int columns)
        {
			Cell[,] board = new Cell[rows, columns];
			for (int r = 0; r < rows; r++)
            {
				for (int c = 0; c < columns; c++)
                {
					board[r, c] = new Cell();
                }
            }
			return board;
        }

		private static Cell At(Cell[,] board, Coord coord)
			=> board[coord.Y, coord.X];

		private static IEnumerable<Cell> Line(Cell[,] board, Coord start, Coord step)
			=> Enumerable.Range(0, 4)
			.Select(i => At(board, start + i * step));

        
    }
}
