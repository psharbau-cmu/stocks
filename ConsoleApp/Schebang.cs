using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class Schebang
    {
        static void Main(string[] args)
        {
            var events = new List<Event>();
            using (var reader = new StreamReader(File.OpenRead(@".\AllEvents.csv")))
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

            Console.WriteLine($"Read {events.Count} events");

            var allTests = events.Select(evt => Tuple.Create(evt.GetInputArray(), evt.GetOutputArray()));
            var trainTests = new List<Tuple<float[], float[]>>();
            var testTests = new List<Tuple<float[], float[]>>();
            var random = new Random();

            foreach (var test in allTests)
            {
                var good = true;
                for (var i = 0; i < 4; i++)
                {
                    var val = test.Item1[i];
                    if (val < -1 || val > 1) good = false;
                }

                for (var i = 0; i < 2; i++)
                {
                    var val = test.Item2[i];
                    if (val < -1 || val > 1) good = false;
                }

                if (good)
                {
                    if (random.Next(20) == 4) // chosen by fair dice roll
                    {
                        testTests.Add(test);
                    }
                    else
                    {
                        trainTests.Add(test);
                    }
                }
            }
            
            Console.WriteLine($"Training size: {trainTests.Count}");
            Console.WriteLine($"Testing Size:  {testTests.Count}");

            var descripion = SimpleDescriptionBuilder.GetDescription(4, new[] {4, 4, 3, 3, 2});
            foreach (var node in descripion.Nodes.Where(n => descripion.Outputs.Contains(n.NodeId)))
            {
                node.Processor = null;
            }

            var nets = new List<Net>();
            var trainers = new List<Trainer>();
            var tasks = new List<Task>();

            for (var i = 0; i < 4; i++)
            {
                var net = Net.FromDescription(descripion);
                var trainer = new Trainer(trainTests, net);
                nets.Add(net);
                trainers.Add(trainer);

                tasks.Add(Task.Run(() => trainer.Train(.5f, 0f, .0001f, 100000, true)));
            }
            
            Task.WhenAll(tasks).GetAwaiter().GetResult();

            foreach (var net in nets)
            {
                Console.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));

                var fileName = $".\\{Path.GetRandomFileName()}.json";

                using (var writer = File.CreateText(fileName))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(net.Description, Formatting.Indented));
                }

                Console.WriteLine($"Wrote file {fileName}");
            }

            Console.ReadKey();
        }
    }
}
