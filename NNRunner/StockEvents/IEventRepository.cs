using System.Collections.Immutable;

namespace NNRunner.StockEvents
{
    public interface IEventRepository
    {
        IImmutableList<Event> TrainingEvents { get; }
        IImmutableList<Event> TestingEvents { get; }
    }
}
