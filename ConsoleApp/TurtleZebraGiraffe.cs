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
            var nothing = new [] {.1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f};
            var turtle = new [] {1, .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f, .1f};
            var zebra = new [] {1, 1, 1, 1, .1f, .1f, .1f, .1f, .1f, .1f};
            var giraffe = new [] {1f, 1, 1, 1, 1, 1, 1, 1, 1, 1};

            var tests = new[]
            {
                Tuple.Create(nothing, nothing),
                Tuple.Create(turtle, turtle),
                Tuple.Create(turtle, turtle),
                Tuple.Create(turtle, turtle),
                Tuple.Create(turtle, turtle),
                Tuple.Create(turtle, turtle),
                Tuple.Create(zebra, zebra),
                Tuple.Create(giraffe, giraffe)
            };

            var description = SimpleDescriptionBuilder.GetDescription(10, new[] {3, 3, 5, 5, 10});
            var net = Net.FromDescription(description);

            var trainer = new Trainer(tests, net);

            trainer.Train(.01f, 0, .0001f, 1000000, true);

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
