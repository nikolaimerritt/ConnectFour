using System;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using BoardGameLearner;
using System.Collections.Generic;

namespace ConnectFour
{
	public class ConnectFour
	{
		private readonly PlayerID _firstPlayer;
		private readonly PlayerID _secondPlayer;
		private PlayerID _currentPlayer;
		private readonly Cell[,] _board;

		public ConnectFour(PlayerID firstPlayer, PlayerID secondPlayer, int rows = 6, int columns = 7)
		{
			_firstPlayer = firstPlayer;
			_secondPlayer = secondPlayer;
			_currentPlayer = _firstPlayer;
			_board = BlankBoard(rows, columns);
		}

		public void MakeMove(PlayerID player, int col)
        {
			if (player != _currentPlayer)
				throw new ArgumentException($"It is not the turn of {player}");

			if (!_board[0, col].IsEmpty)
				throw new ArgumentException($"Could not place a piece along column {col} as the column is full");

			for (int r = 0; r < _board.GetLength(0); r++)
            {
				if (_board[r, col].IsEmpty && (r == _board.GetLength(0) - 1 || !_board[r+1, col].IsEmpty))
                {
					_board[r, col].PlayerId = player;
					_currentPlayer = (_currentPlayer == _firstPlayer) ? _secondPlayer : _firstPlayer;
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

        public override string ToString()
        {
			string header = new ('#', count: _board.GetLength(1) + 2);
			StringBuilder builder = new(header);
			builder.AppendLine();

			for (int r = 0; r < _board.GetLength(0); r++)
            {
				builder.Append('#');
				for (int c = 0; c < _board.GetLength(1); c++)
                {
					char cell = ' ';
					if (_board[r, c].PlayerId == _firstPlayer)
						cell = 'O';
					else if (_board[r, c].PlayerId == _secondPlayer)
						cell = 'X';
					builder.Append(cell);
                }
				builder.Append('#');
				builder.AppendLine();
            }
			builder.AppendLine(header);
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

		private Cell At(Coord coord)
			=> At(_board, coord);

		private static IEnumerable<Cell> Line(Cell[,] board, Coord start, Coord step)
			=> Enumerable.Range(0, 4)
			.Select(i => At(board, start + i * step));
    }
}
