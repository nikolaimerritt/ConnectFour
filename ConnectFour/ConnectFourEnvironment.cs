using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DeepLearning;
using BoardGameLearner;
using MathNet.Numerics.LinearAlgebra;

namespace ConnectFour
{
    public class ConnectFourEnvironment : ConnectFour, IEnvironment<int>
    {
        public int LayerSize => _board.Length;

        private readonly PlayerID _player;
        private readonly PlayerID _opponent;
        private static readonly Random _rng = new();

        public ConnectFourEnvironment(PlayerID player, PlayerID opponent, bool playsFirst, int rows = 6, int columns = 7)
            : base(firstPlayer: playsFirst ? player : opponent, 
                  secondPlayer: playsFirst ? opponent : player, 
                  rows, 
                  columns) 
        {
            _player = player;
            _opponent = opponent;
        }

        public bool HasLost()
            => HasLost(_player);

        public bool HasTimedOut()
            => false;

        public bool HasWon()
            => HasWon(_player);

        public bool IsTerminal()
            => IsGameOver();

        public void MakeMove(int move)
        {
            MakeMove(player: _player, col: move);
            if (!IsTerminal())
            {
                MakeMove(player: _opponent, col: SmartRandom());
            }
        }

        public int Hint(out bool gaveHint)
        {
            var validMoves = AllPossibleMoves.Where(IsValidMove);
            // if can win
            foreach (int move in validMoves)
            {
                MakeMove(player: _player, col: move);
                if (HasWon(_player))
                {
                    gaveHint = true;
                    UndoLastMove();
                    return move;
                }
                else UndoLastMove();
            }
            // if can block opponent from winning
            foreach (int move in validMoves)
            {
                MakeMove(player: _opponent, col: move);
                if (HasWon(_opponent))
                {
                    gaveHint = true;
                    UndoLastMove();
                    return move;
                }
                else UndoLastMove();
            }
            gaveHint = false;
            return -1;
        }

        public double Reward()
        {
            if (HasWon())
                return 1;
            if (HasLost())
                return -1;
            return Math.Clamp(Value(_player) / 30, min: -0.1, max: 0.1);
        }

        public Vector<double> ToLayer()
            => ToInputLayer(_player);

        public string MoveToString(int move) 
            => $"{move + 1}";

        private int OneStepAhead() // deterministic
        {
            List<int> validMoves = AllPossibleMoves.Where(IsValidMove).ToList();
            // opponent blocks players win
            int smartMove = WinsOrBlocksWin(out bool existsSmartMove);
            if (existsSmartMove)
                return smartMove;

            int bestMove = validMoves.Contains(3) ? 3 : validMoves.First(); // will prefer to go in centre
            double highestValue = double.NegativeInfinity;
            foreach (int move in validMoves)
            {
                MakeMove(_opponent, move);
                double value = Value(_opponent);
                if (value > highestValue)
                {
                    highestValue = value;
                    bestMove = move;
                }
                UndoLastMove();
            }
            return bestMove;
        }

        private int SmartRandom()
        {
            var validMoves = AllPossibleMoves.Where(IsValidMove).ToList();
            if (_moveHistory.Count < 15 || _rng.NextDouble() < 0.4)
            {
                int smartMove = WinsOrBlocksWin(out bool existsSmartMove);
                if (existsSmartMove)
                    return smartMove;
            }
            return validMoves[_rng.Next(validMoves.Count)];
        }

        private int WinsOrBlocksWin(out bool existsSmartMove)
        {
            var validMoves = AllPossibleMoves.Where(IsValidMove).ToList();
            foreach (int move in validMoves)
            {
                MakeMove(player: _opponent, col: move);
                if (HasWon(_opponent))
                {
                    UndoLastMove();
                    existsSmartMove = true;
                    return move;
                }
                else UndoLastMove();
            }

            foreach (int move in validMoves)
            {
                MakeMove(player: _player, col: move);
                if (HasWon(_player))
                {
                    UndoLastMove();
                    existsSmartMove = true;
                    return move;
                }
                else UndoLastMove();
            }
            existsSmartMove = false;
            return -1;
        }

        private int RandomMove()
        {
            List<int> validMoves = AllPossibleMoves.Where(IsValidMove).ToList();
            return validMoves[_rng.Next(validMoves.Count)];
        }

        private int UserMove()
            => int.Parse(Console.ReadLine());

        public override string ToString()
            => base.ToString();
    }
}
