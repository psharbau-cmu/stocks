using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class TurtleZebraGiraffe
    {
        static void Main(string[] args)
        {
            var nothing = new [] {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
            var turtle = new [] {1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
            var zebra = new [] {1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f};
            var giraffe = new [] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1};

            var tests = new List<Tuple<float[], float[]>>
            {
                Tuple.Create(nothing, nothing),
                Tuple.Create(turtle, turtle),
                Tuple.Create(zebra, zebra),
                Tuple.Create(giraffe, giraffe)
            };

            var description = SimpleDescriptionBuilder.GetDescription(10, new[] {3, 3,3, 10});
            var net = Net.FromDescription(description);

            var trainer = new Trainer(tests, net);

            trainer.Train(.01f, .9f, .0001f, 1000000, true);

            Console.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));

            var forwardFunc = net.GetEvaluationFunction();

            foreach (var test in tests)
            {
                Console.Write(string.Join(", ", test.Item1.Select(n => n.ToString())));
                Console.Write(" => ");
                Console.WriteLine(string.Join(", ", forwardFunc(test.Item1).Select(n => n.ToString())));
            }

            Console.ReadKey();
        }
    }
}
