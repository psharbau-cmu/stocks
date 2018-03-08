using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NNRunner
{
    public interface IProcessRepository<TSnapshot, TSummary>
    {
        IEnumerable<Guid> GetIds();
        Guid CreateProcess(Action<Action<TSnapshot>, CancellationToken> process);
        ProcessProgress<TSnapshot, TSummary> GetProcessProgress(Guid id);
        void StopProcess(Guid id);
    }
}
