using System;
using BoardGameLearner;

namespace ConnectFour
{
    class Program
    {
        static int UserMove()
            => int.Parse(Console.ReadLine());

        static void UsersPlay()
        {
            PlayerID first = new("Neek");
            PlayerID second = new("Alexei");

            ConnectFour game = new(first, second);
            PlayerID currentPlayer = first;
            
            Console.WriteLine(game);
            while (game.Winner() == null)
            {
                game.MakeMove(currentPlayer, UserMove());
                Console.Clear();
                Console.WriteLine(game);
                currentPlayer = (currentPlayer == first) ? second : first;
            }
            Console.WriteLine(game.Winner());
        }

        static void Main(string[] args)
        {
            UsersPlay();
        }
    }
}
