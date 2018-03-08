using System;
using System.Collections.Generic;

namespace NNRunner.NeuralNet
{
    public class TrainingJobRepository : IProcessRepository<TrainingJob>
    {
        private readonly Dictionary<Guid, ProcessProgress<TrainingJob>> _jobs = new Dictionary<Guid, ProcessProgress<TrainingJob>>();

        public Guid CreateProcess(Action<Action<TrainingJob>> process)
        {
            var progress = new ProcessProgress<TrainingJob>(process);
            _jobs.Add(progress.Id, progress);
            return progress.Id;
        }

        public ProcessProgress<TrainingJob> GetProcessProgress(Guid id)
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
    }
}
