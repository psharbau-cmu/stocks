using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp3
{
    class ClusterSearch
    {
        class Data
        {
            public Data(string[] parts)
            {
                Symbol = parts[0];
                Day = Int32.Parse(parts[1]);
                Open = float.Parse(parts[2]);
                CloseDiff = float.Parse(parts[3]);
                SpreadOverFlow = float.Parse(parts[4]);
                NextLow = float.Parse(parts[5]);
                TwoDayHigh = float.Parse(parts[6]);
            }

            public string Symbol { get; }
            public int Day { get;  }
            public float Open { get; }
            public float CloseDiff { get; }
            public float SpreadOverFlow { get; }
            public float NextLow { get; }
            public float TwoDayHigh { get; }
        }

        static void Main(string[] args)
        {
            var history = new List<Data>();

            using (var reader = new StreamReader(File.OpenRead(@".\allPoints.csv")))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    history.Add(new Data(reader.ReadLine().Split(',')));
                    if (history.Count % 1000 == 0) Console.WriteLine(history.Count);
                }
            }

            var counts = new int[256, 256];

            foreach (var record in history)
            {
                var x = (int)((record.CloseDiff + 0.03) / 0.06 * 256);
                var y = (int)((record.SpreadOverFlow + 2) / 5 * 256);

                if (x < 0 || x >= 256 || y < 0 || y >= 256) continue;

                counts[x, y] += 1;
            }

            using (var writer = File.CreateText(@".\counts.json"))
            {
                writer.WriteLine("[");

                for (var x = 0; x < 256; x++)
                {
                    for (var y = 0; y < 256; y++)
                    {
                        writer.WriteLine($"{{\"x\":{x},\"y\":{y},\"sum\":{counts[x, y]}}},");
                    }
                }

                writer.WriteLine("]");
            }

                Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
