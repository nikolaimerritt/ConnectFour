using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetLearning;
using NeuralNetLearning.Maths;
using BoardGameLearner;
using DeepLearning;

namespace ConnectFour
{
    class Program
    {
        static readonly string directoryPath = "C:/Users/tiamat/Code/C#/ConnectFour/augmented-qlearner-initialised-vs-random-5m";

        static List<Tuple<Vector<double>, Vector<double>>> TrainingPairs(int count)
        {
            List<Tuple<Vector<double>, Vector<double>>> trainingPairs = new(capacity: count);
            for (int i = 0; i < count; i++)
            {
                Vector<double> input = 50 * VectorFunctions.StdUniform(dim: 1);
                Vector<double> desiredOutput = input.PointwisePower(2);
                trainingPairs.Add(new Tuple<Vector<double>, Vector<double>>(input, desiredOutput));
            }
            return trainingPairs;
        }

        static void LeanXSquared()
        {
            var layers = new List<NeuralLayerConfig>() 
            {
                new InputLayer(1),
                new HiddenLayer(8, new ReluActivation()),
                new HiddenLayer(8, new ReluActivation()),
                new OutputLayer(1, new IdentityActivation())
            };
            var descender = new AdamGradientDescender();
            var cost = new MSECost();
            NeuralNet net = NeuralNetFactory.RandomCustomisedForRelu(layers, descender, cost);

            Console.WriteLine("learning...");
            net.GradientDescent(TrainingPairs(10000), numEpochs: 15000);
            Console.WriteLine("finished learning!");
            Console.ReadKey();

            foreach ((var input, var desiredOutput) in TrainingPairs(10))
            {
                Console.WriteLine($"{input[0]:0.000} \t --> \t {net.Output(input)[0]:0.000} \t (actual {desiredOutput[0]:0.000})");
            }

        }

        static int UserMove()
            => int.Parse(Console.ReadLine()) - 1;

        static void UsersPlay()
        {
            PlayerID neek = new("Neek");
            PlayerID alexei = new("alexei");

            BoardGameLearner<ConnectFour, int> learner = new(alexei, neek);

            ConnectFour game = new(neek, alexei);
            PlayerID currentPlayer = neek;
            
            Console.WriteLine(game);
            while (!game.IsGameOver())
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

        static void TrainQLearner(int games)
        {
            PlayerID ai = new("AI");
            PlayerID rando = new("Rando");
            ConnectFourEnvironment environment = new(ai, rando, playsFirst: true);
            QLearner<ConnectFourEnvironment, int> qLearner = QLearner<ConnectFourEnvironment, int>.ReadFromDirectory(environment, "C:/Users/tiamat/Code/C#/ConnectFour/augmented-qlearner-vs-random-500k");
            qLearner.Learn(games, directoryPath);
            Console.WriteLine($"Finished learning! Writing to directory {directoryPath}");
            qLearner.WriteToDirectory(directoryPath);
        }

        static void TestQLearner()
        {
            PlayerID ai = new("AI");
            PlayerID rando = new("Rando");
            ConnectFourEnvironment game = new(ai, rando, playsFirst: true);
            QLearner<ConnectFourEnvironment, int> qLearner = QLearner<ConnectFourEnvironment, int>.ReadFromDirectory(game, directoryPath);

            PlayerID currentPlayer = ai;
            while (!game.IsGameOver())
            {
                if (currentPlayer.Name == "AI")
                {
                    Console.Clear();
                    qLearner.ShowQValues();
                    game.MakeMove(currentPlayer, qLearner.MakeMove(game));
                    Console.WriteLine(game);
                }
                else
                {
                    game.MakeMove(currentPlayer, UserMove());
                    Console.Clear();
                    Console.WriteLine(game);
                }

                currentPlayer = (currentPlayer == rando) ? ai : rando;
            }
            Console.WriteLine(game.Winner());
        }

        static void Main(string[] args)
        {
            TestQLearner();
        }
    }
}
