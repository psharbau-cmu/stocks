using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NerualNet.Builder;
using NerualNet.Logic;
using NerualNet.Training;
using Newtonsoft.Json;

namespace UnsupThenSup
{
    class Program
    {
        static void Main(string[] args)
        {
            var events = ReadEventFile("trainingEvents.csv");
            var ins = events
                .Where(e => (e.Open - e.NextLow) / e.Open > .1f)
                .Select(evts => evts.GetInputArray());

            Console.WriteLine($"Qualified Events: {ins.Count()}");
            var unsupervisedTests = ins.Select(i => Tuple.Create(i, i));
            var supervisedTests = events.Select(evt => Tuple.Create(evt.GetInputArray(), evt.GetOutputArray()));

            var builder = new LayerBuilder();
            var description = builder.BuildDescription(5, new[]
            {
                new LayerBuilder.LayerSpec(10, "sum", "softplus"),
                new LayerBuilder.LayerSpec(10, "sum", "softplus"),
                new LayerBuilder.LayerSpec(10, "sum", "tanh"),
                new LayerBuilder.LayerSpec(10, "sum", "softplus"),
                new LayerBuilder.LayerSpec(10, "sum", "softplus"),
                new LayerBuilder.LayerSpec(5, "sum", null)
            });
            //var description = builder.BuildDescription(5, new[]
            //{
            //    new LayerBuilder.LayerSpec(5, "sum", "softplus"),
            //    new LayerBuilder.LayerSpec(6, "sum", "tanh"),
            //    new LayerBuilder.LayerSpec(4, "sum", "softplus"),
            //    new LayerBuilder.LayerSpec(5, "sum", null)
            //});
            var net = Net.FromDescription(description);
            var trainer = new SimpleTrainer();
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 5e-6f,
                maxEpochs: 200,
                learningRate: 0.75f);
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 5e-6f,
                maxEpochs: 9500,
                learningRate: 0.5f);
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 200,
                learningRate: .25f);
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 200,
                learningRate: .125f);
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 200,
                learningRate: .0625f);


            for (var i = 0; i < 5; i++)
            {
                var nextDescription = net.Description;
                var firstSigmoidId = nextDescription.Nodes.First(n => n.Processor == "tanh").NodeId;
                nextDescription.Nodes = nextDescription.Nodes.Where(n => n.NodeId != firstSigmoidId).ToArray();
                foreach (var node in nextDescription.Nodes)
                {
                    node.Inputs = node.Inputs
                        .Where(inp => inp.FromInputVector || inp.InputId != firstSigmoidId)
                        .ToArray();
                }
                net = Net.FromDescription(nextDescription);

                Console.WriteLine($"Removed {i + 1} sigmoids");

                trainer.Train(
                    net: net,
                    tests: unsupervisedTests,
                    desiredError: 1e-6f,
                    maxEpochs: 400,
                    learningRate: 0.5f);
            }

            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 1e-6f,
                maxEpochs: 500,
                learningRate: 0.5f);
            trainer.Train(
                net: net,
                tests: unsupervisedTests,
                desiredError: 1e-6f,
                maxEpochs: 400,
                learningRate: 0.25f);

            var finalDescription = net.Description;
            var outsRemoved = new[] { finalDescription.Outputs[1], finalDescription.Outputs[2], finalDescription.Outputs[3], finalDescription.Outputs[4] };
            finalDescription.Nodes = finalDescription.Nodes
                .Where(n => !outsRemoved.Contains(n.NodeId))
                .ToArray();
            finalDescription.Outputs = new[] { finalDescription.Outputs[0] };
            var nextNet = Net.FromDescription(finalDescription);
            

            trainer.Train(
                net: nextNet,
                tests: supervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 20000,
                learningRate: .25f);
            trainer.Train(
                net: nextNet,
                tests: supervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 20000,
                learningRate: .125f);
            trainer.Train(
                net: nextNet,
                tests: supervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 2000,
                learningRate: .0625f);
            trainer.Train(
                net: nextNet,
                tests: supervisedTests,
                desiredError: 1e-8f,
                maxEpochs: 2000,
                learningRate: .03125f);
            var final = net.Description;
            var finalText = JsonConvert.SerializeObject(final);
            using (var writer = File.CreateText("out3.json"))
            {
                writer.Write(finalText);
            }
            Console.WriteLine();

                Console.ReadLine();
        }

        private static IEnumerable<Event> ReadEventFile(string filePath)
        {
            var path = Path.Combine(".", filePath);
            var events = new List<Event>();
            using (var reader = new StreamReader(File.OpenRead(path), Encoding.UTF8))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var parts = reader.ReadLine().Split(",");

                    events.Add(new Event()
                    {
                        Symbol = parts[0],
                        Day = int.Parse(parts[1]),
                        Open = float.Parse(parts[2]),
                        CloseDiff = float.Parse(parts[3]),
                        SpreadOverLow = float.Parse(parts[4]),
                        Volume = float.Parse(parts[5]),
                        NextLow = float.Parse(parts[6]),
                        TwoDayHigh = float.Parse(parts[7]),
                        ThreeDayOpen = float.Parse(parts[8])
                    });

                    if (events.Count % 1000 == 0) Console.WriteLine($"Read {events.Count} events");
                }
            }

            return events;
        }
    }
}
