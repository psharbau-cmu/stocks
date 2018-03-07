using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp
{
    class EventSplitter
    {
        static void Main(string[] args)
        {
            var allEvents = new List<string>();
            var topLine = "";
            using (var reader = new StreamReader(File.OpenRead(@".\AllEvents.csv")))
            {
                topLine = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(",");
                    var evt = new Event()
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
                    };

                    var ok = true;
                    var ins = evt.GetInputArray();
                    for (var i = 0; i < ins.Length; i++)
                        if (ins[i] < -1 || ins[i] > 1) ok = false;
                    var outs = evt.GetOutputArray();
                    for (var i = 0; i < outs.Length; i++)
                        if (outs[i] < -1 || outs[i] > 1) ok = false;

                    if (ok) allEvents.Add(line);

                    if (allEvents.Count % 1000 == 0) Console.WriteLine($"Read {allEvents.Count} events");
                }
            }

            var trainingEvents = new SortedList<double, string>();
            var testingEvents = new List<string>();
            var random = new Random();

            foreach (var evt in allEvents)
            {
                if (random.Next(20) == 4) // chosen by fair dice roll
                {
                    testingEvents.Add(evt);
                }
                else
                {
                    var key = random.NextDouble();
                    while (trainingEvents.ContainsKey(key)) key = random.NextDouble();
                    trainingEvents.Add(key, evt);
                }
            }

            Console.WriteLine($"Training Events: {trainingEvents.Count}");
            Console.WriteLine($"Testing Events:  {testingEvents.Count}");

            using (var writer = File.CreateText(@".\trainingEvents.csv"))
            {
                writer.WriteLine(topLine);
                foreach (var evt in trainingEvents.Values)
                {
                    writer.WriteLine(evt);
                }
            }

            using (var writer = File.CreateText(@".\testingEvents.csv"))
            {
                writer.WriteLine(topLine);
                foreach (var evt in testingEvents)
                {
                    writer.WriteLine(evt);
                }
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
