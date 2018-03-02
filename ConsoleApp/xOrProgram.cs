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
            NetDescription description = null;

            using (var reader = new StreamReader(File.OpenRead("xOrNet.json")))
            {
                var text = reader.ReadToEnd();
                description = JsonConvert.DeserializeObject<NetDescription>(text);
            }

            var net = Net.FromDescription(description);
            
            var tests = new[]
            {
                Tuple.Create(new[] {1f, 1f}, new[] {0f}),
                Tuple.Create(new[] {1f, 0f}, new[] {1f}),
                Tuple.Create(new[] {0f, 1f}, new[] {1f}),
                Tuple.Create(new[] {0f, 0f}, new[] {0f})
            };

            var trainer = new Trainer(tests, net);

            trainer.Train(0.25f, 0.5f, 0.0001f, 1000);

            Console.ReadKey();
        }
        
    }
}
