using System;
using BoardGameLearner;

namespace ConnectFour
{
    class Program
    {
        static int UserMove()
            => int.Parse(Console.ReadLine()) - 1;



        static void UsersPlay()
        {
            PlayerID neek = new("Neek");
            PlayerID alexei = new("AI");

            BoardGameLearner<ConnectFour, int> learner = new(alexei, neek);

            ConnectFour game = new(neek, alexei);
            PlayerID currentPlayer = neek;
            
            Console.WriteLine(game);
            while (game.Winner() == null)
            {
                if (currentPlayer.Name == "AI")
                    game.MakeMove(currentPlayer, learner.MakeMove(game));
                else
                    game.MakeMove(currentPlayer, UserMove());
               
                Console.Clear();
                Console.WriteLine(game);
                Console.WriteLine($"{neek}: {game.Value(neek)} \t {alexei}: {game.Value(alexei)}");
                
                currentPlayer = (currentPlayer == neek) ? alexei : neek;
            }
            Console.WriteLine(game.Winner());
        }

        static void Main(string[] args)
        {
            UsersPlay();
        }
    }
}
