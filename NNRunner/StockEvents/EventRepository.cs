using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNRunner.StockEvents
{
    public class EventRepository : IEventRepository
    {
        private IImmutableList<Event> _trainingEvents;
        private IImmutableList<Event> _testingEvents;

        public IImmutableList<Event> TrainingEvents
        {
            get
            {
                if (_trainingEvents != null) return _trainingEvents;
                _trainingEvents = ReadEventFile(@"trainingEvents.csv").ToImmutableList();
                return _trainingEvents;
            }
        }

        public IImmutableList<Event> TestingEvents
        {
            get
            {
                if (_testingEvents != null) return _testingEvents;
                _testingEvents = ReadEventFile(@"testingEvents.csv").ToImmutableList();
                return _testingEvents;
            }
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
