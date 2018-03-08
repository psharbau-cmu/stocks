using System;
using System.Collections.Generic;
using System.Threading;

namespace NNRunner.NeuralNet
{
    public class TrainingJobRepository : IProcessRepository<TrainingJob, float>
    {
        private readonly Dictionary<Guid, ProcessProgress<TrainingJob, float>> _jobs = new Dictionary<Guid, ProcessProgress<TrainingJob, float>>();

        public Guid CreateProcess(Action<Action<TrainingJob>, CancellationToken> process)
        {
            var progress = new ProcessProgress<TrainingJob, float>(process, job => job.AvgError);
            _jobs.Add(progress.Id, progress);
            return progress.Id;
        }

        public ProcessProgress<TrainingJob, float> GetProcessProgress(Guid id)
        {
            if (!_jobs.ContainsKey(id))
            {
                throw new KeyNotFoundException();
            }

            return _jobs[id];
        }

        public IEnumerable<Guid> GetIds()
        {
            return _jobs.Keys;
        }

        public void StopProcess(Guid id)
        {
            _jobs[id].Cancel();
        }
    }
}
