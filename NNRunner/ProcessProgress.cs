using System;
using System.Threading;
using System.Threading.Tasks;

namespace NNRunner
{
    public class ProcessProgress<TSnapshot>
    {
        private Task _task;
        private int _snapshotsSent;
        private DateTimeOffset _lastDateTime;
        private TSnapshot _snapshot;
        private string _error;
        private readonly CancellationTokenSource _tokenSource;

        public ProcessProgress(Action<Action<TSnapshot>, CancellationToken> process)
        {
            Id = Guid.NewGuid();
            _snapshotsSent = 0;
            _lastDateTime = DateTimeOffset.Now;
            _snapshot = default(TSnapshot);
            _error = null;

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;

            try
            {
                _task = Task.Run(() => process(snap =>
                {
                    _snapshotsSent += 1;
                    _lastDateTime = DateTimeOffset.Now;
                    _snapshot = snap;
                }, token));
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }
        
        public Guid Id { get; }
        public bool Running => !_task.IsCompleted;
        public int SnapshotsSent => _snapshotsSent;
        public DateTimeOffset LastSnapshot => _lastDateTime;
        public TSnapshot Snapshot => _snapshot;
    }
}