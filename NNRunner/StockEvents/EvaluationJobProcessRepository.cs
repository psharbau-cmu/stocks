using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NNRunner.StockEvents
{
    public class EvaluationJobProcessRepository : IProcessRepository<EvaluationJob, float>
    {
        public Dictionary<Guid, ProcessProgress<EvaluationJob, float>> _jobs
            = new Dictionary<Guid, ProcessProgress<EvaluationJob, float>>();

        public IEnumerable<Guid> GetIds()
        {
            return _jobs.Keys;
        }

        public Guid CreateProcess(Action<Action<EvaluationJob>, CancellationToken> process)
        {
            var progress = new ProcessProgress<EvaluationJob, float>(process, j => j.AvgError);
            _jobs.Add(progress.Id, progress);
            return progress.Id;
        }

        public ProcessProgress<EvaluationJob, float> GetProcessProgress(Guid id)
        {
            return _jobs[id];
        }

        public void StopProcess(Guid id)
        {
            _jobs[id].Cancel();
        }
    }
}
