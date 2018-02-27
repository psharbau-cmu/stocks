using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp3
{
    class Data
    {
        public float Open { get; set; }
        public float CloseDiff { get; set; }
        public float SpreadOverLowStdDevs { get; set; }
        public float Low { get; set; }
        public float High { get; set; }
    }

    class Program
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

            var hists = new Dictionary<string, SortedList<int, Data>>();

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

                        if (open == 0) continue;

                        var dayDiff = (close - open) / open;

                        var spreadOverLow = (high - low) * 100 / low;
                        var stdDevsFromMean = spreadOverLow == 0 || !dayAvgs.ContainsKey(day)
                            ? (float?) null
                            : (spreadOverLow - dayAvgs[day]) / dayStdDev[day];

                        if (!stdDevsFromMean.HasValue) continue;

                        if (!hists.ContainsKey(key)) hists.Add(key, new SortedList<int, Data>());
                        hists[key].Add(dayInt, new Data()
                        {
                            CloseDiff = dayDiff,
                            SpreadOverLowStdDevs = stdDevsFromMean.Value,
                            Open = open,
                            Low =  low,
                            High = high
                        });
                    }
                }
            }

            using (var writer = File.CreateText(@".\allPoints.csv"))
            {
                writer.WriteLine("Symbol,Day,Open,CloseDiff,SpreadOverLow,NextLow,TwoDayHigh");

                foreach (var hist in hists)
                {
                    Tuple<int, Data> twoAgo = null;
                    Tuple<int, Data> oneAgo = null;
                    Tuple<int, Data> now = null;

                    foreach (var day in hist.Value)
                    {
                        twoAgo = oneAgo;
                        oneAgo = now;
                        now = Tuple.Create(day.Key, day.Value);

                        if (twoAgo != null)
                        {
                            writer.WriteLine(
                                $"{hist.Key},{twoAgo.Item1},{twoAgo.Item2.Open},{twoAgo.Item2.CloseDiff},{twoAgo.Item2.SpreadOverLowStdDevs},{oneAgo.Item2.Low},{now.Item2.High}");
                        }
                    }
                }
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
