using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp3
{
    class Data2
    {
        public float Open { get; set; }
        public float Close { get; set; }
        public float CloseDiff { get; set; }
        public float SpreadOverLowStdDevs { get; set; }
        public float Low { get; set; }
        public float High { get; set; }
        public float Volume { get; set; }
        public bool IsEvent { get; set; }
    }

    class Program2
    {
        static void Main(string[] args)
        {
            var days = new Dictionary<string, List<float>>();

            foreach (var file in Directory.EnumerateFiles(@"C:\Users\patrick-sharbaugh\Downloads\NYSE\"))
            {
                Console.WriteLine(file);

                using (var streamReader = new StreamReader(File.OpenRead(file)))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var parts = streamReader.ReadLine().Split(',');
                        var day = parts[1];
                        var high = float.Parse(parts[3]);
                        var low = float.Parse(parts[4]);
                        var spreadOverLow = (high - low) * 100 / low;

                        if (!days.ContainsKey(day)) days.Add(day, new List<float>());
                        days[day].Add(spreadOverLow);
                    }
                }
            }

            var dayAvgs = days
                .Where(kvp => kvp.Value.Any(val => val > 0))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Where(val => val > 0 && val < 10).Average());

            var dayStdDev = days
                .Where(kvp => kvp.Value.Any(val => val > 0))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (float)Math.Sqrt(kvp.Value
                        .Where(val => val > 0 && val < 10)
                        .Select(x => Math.Pow(x - dayAvgs[kvp.Key], 2))
                        .Average()));

            var hists = new Dictionary<string, SortedList<int, Data2>>();

            foreach (var file in Directory.EnumerateFiles(@"C:\Users\patrick-sharbaugh\Downloads\NYSE\"))
            {
                Console.WriteLine(file);

                using (var streamReader = new StreamReader(File.OpenRead(file)))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var parts = streamReader.ReadLine().Split(',');
                        var key = parts[0];
                        var day = parts[1];
                        var date = DateTime.Parse(day);
                        var dayInt = Int32.Parse(date.ToString("yyyyMMdd"));
                        var open = float.Parse(parts[2]);
                        var high = float.Parse(parts[3]);
                        var low = float.Parse(parts[4]);
                        var close = float.Parse(parts[5]);
                        var vol = float.Parse(parts[6]);

                        if (open == 0) continue;

                        var dayDiff = (close - open) / open;

                        var spreadOverLow = (high - low) * 100 / low;
                        var stdDevsFromMean = spreadOverLow == 0 || !dayAvgs.ContainsKey(day)
                            ? (float?) null
                            : (spreadOverLow - dayAvgs[day]) / dayStdDev[day];

                        var isEvent = 
                            dayDiff != 0
                            && dayDiff > -.1 
                            && dayDiff < .1 
                            && stdDevsFromMean > 6.805544 * Math.Pow(Math.Abs(dayDiff), 0.09175393);
                        // y = 6.805544*x^0.09175393

                        if (!stdDevsFromMean.HasValue) continue;

                        if (!hists.ContainsKey(key)) hists.Add(key, new SortedList<int, Data2>());
                        hists[key].Add(dayInt, new Data2()
                        {
                            CloseDiff = dayDiff,
                            SpreadOverLowStdDevs = stdDevsFromMean.Value,
                            Open = open,
                            Close = close,
                            Low =  low,
                            High = high,
                            Volume = vol,
                            IsEvent = isEvent
                        });
                    }
                }
            }


            var evtCount = 0;
            using (var writer = File.CreateText(@".\AllEvents.csv"))
            {
                writer.WriteLine("Symbol,Day,Open,CloseDiff,SpreadOverLow,Volume,NextLow,TwoDayHigh,ThreeDayOpen");

                foreach (var hist in hists)
                {
                    Tuple<int, Data2> threeAgo = null;
                    Tuple<int, Data2> twoAgo = null;
                    Tuple<int, Data2> oneAgo = null;
                    Tuple<int, Data2> now = null;

                    foreach (var day in hist.Value)
                    {
                        threeAgo = twoAgo;
                        twoAgo = oneAgo;
                        oneAgo = now;
                        now = Tuple.Create(day.Key, day.Value);

                        if (threeAgo != null && threeAgo.Item2.IsEvent)
                        {
                            evtCount += 1;  
                            writer.WriteLine(
                                $"{hist.Key},{threeAgo.Item1},{threeAgo.Item2.Open},{threeAgo.Item2.CloseDiff},{threeAgo.Item2.SpreadOverLowStdDevs},{threeAgo.Item2.Volume},{twoAgo.Item2.Low},{oneAgo.Item2.High},{now.Item2.Open}");
                        }
                    }
                }
            }
            Console.WriteLine($"{evtCount} events");
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
