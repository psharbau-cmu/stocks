using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp3
{
    class ClusterSpreadSearch
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

        class Square
        {
            private int _count = 0;
            private SortedList<float, int> _nextLows = new SortedList<float, int>();
            private SortedList<float, int> _twoDayHighs = new SortedList<float, int>();

            public float MinDayDiff { get; set; }
            public float MaxDayDiff { get; set; }
            public float MinRelVolitility { get; set; }
            public float MaxRelVolitility { get; set; }

            public int Count => _count;

            public float NextLow95
            {
                get
                {
                    if (_count < 1) return 0f;

                    var sum = 0;
                    var target = _count * 19 / 20;
                    foreach (var kvp in _nextLows)
                    {
                        sum += kvp.Value;
                        if (sum >= target) return kvp.Key;
                    }

                    throw new Exception("Bad math");
                }
            }

            public float TwoDayHigh95
            {
                get
                {
                    if (_count < 1) return 0f;

                    var sum = 0;
                    var target = _count / 20;
                    foreach (var kvp in _twoDayHighs)
                    {
                        sum += kvp.Value;
                        if (sum >= target) return kvp.Key;
                    }

                    throw new Exception("Bad math");
                }
            }

            public float Spread95 => TwoDayHigh95 - NextLow95;

            public void Add(Data data)
            {
                var nextLowPercent = (data.NextLow - data.Open) / data.Open * 100;
                var twoDayHighPercent = (data.TwoDayHigh - data.Open) / data.Open * 100;

                if (!_nextLows.ContainsKey(nextLowPercent)) _nextLows.Add(nextLowPercent, 1);
                else _nextLows[nextLowPercent] += 1;

                if (!_twoDayHighs.ContainsKey(twoDayHighPercent)) _twoDayHighs.Add(twoDayHighPercent, 1);
                else _twoDayHighs[twoDayHighPercent] += 1;

                _count += 1;
            }
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

            var squares = new Square[256, 256];
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 256; y++)
                {
                    squares[x, y] = new Square()
                    {
                        MinDayDiff = -.1f + (x * .2f / 256f),
                        MaxDayDiff = -.1f + ((x + 1) * .2f / 256f),
                        MinRelVolitility = -2f + (y * 10f / 256f),
                        MaxRelVolitility = -2f + ((y + 1) * 10f / 256f)
                    };
                }
            }

            foreach (var record in history)
            {
                var x = (int)((record.CloseDiff + .1) / .2 * 256);
                var y = (int)((record.SpreadOverFlow + 2) / 10 * 256);

                if (x < 0 || x >= 256 || y < 0 || y >= 256) continue;

                squares[x, y].Add(record);
            }

            using (var writer = File.CreateText(@".\indexSpreadCountWide.json"))
            {
                writer.WriteLine("{");

                for (var x = 0; x < 256; x++)
                {
                    for (var y = 0; y < 256; y++)
                    {
                        writer.Write($"\"{x},{y}\":[{squares[x, y].Spread95},{squares[x, y].Count},{squares[x, y].MinDayDiff}, {squares[x,y].MinRelVolitility}],");
                        if (((x * 256) + y) % 10 == 0) writer.WriteLine();
                        if (((x * 256 + y) % 500 == 0)) Console.WriteLine($"{x},{y}");
                    }
                }

                writer.WriteLine("}");
            }

                Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
