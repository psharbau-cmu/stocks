using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class xOrProgram
    {
        static void Main(string[] args)
        {
            //NetDescription description = null;

            //using (var reader = new StreamReader(File.OpenRead("xOrNet.json")))
            //{
            //    var text = reader.ReadToEnd();
            //    description = JsonConvert.DeserializeObject<NetDescription>(text);
            //}
            var description = SimpleDescriptionBuilder.GetDescription(2, new[] {3, 4, 1});

            var net = Net.FromDescription(description);
            
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var trainer = new Trainer(tests, net);

            trainer.Train(2f, 0.9f, 0.0001f, 1000000, true);

            Console.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));

            var forwardFunc = net.GetEvaluationFunction();

            foreach (var test in tests)
            {
                Console.WriteLine($"{test.Item1[0]}, {test.Item1[1]} => {forwardFunc(test.Item1)[0]}");
            }

            Console.ReadKey();
        }
        
    }
}
