using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NNRunner.StockEvents;

namespace NNRunner
{
    interface IEventRepository
    {
        IImmutableList<Event> TrainingEvents { get; }
        IImmutableList<Event> TestingEvents { get; }
    }
}
