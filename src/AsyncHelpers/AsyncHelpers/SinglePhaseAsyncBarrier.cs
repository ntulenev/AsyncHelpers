using System;
using System.Threading.Tasks;

namespace AsyncHelpers
{
    /// <summary>
    /// Single phases async analogue of <see cref="System.Threading.Barrier"/>
    /// </summary>
    public class SinglePhaseAsyncBarrier
    {
        /// <summary>
        /// Creates barrier for <paramref name="participantCount"/> participants.
        /// </summary>
        /// <param name="participantCount">Count of the participants.</param>
        /// <exception cref="ArgumentException">Throws exception if participantCount is incorrest</exception>
        public SinglePhaseAsyncBarrier(int participantCount)
        {
            if (participantCount < 0)
                throw new ArgumentException("Value should be positive.", nameof(participantCount));

            _participantCount = participantCount;
        }

        /// <summary>
        /// Signals about new participant and reutrns the task that will be finished when all participants will be added.
        /// </summary>
        public Task SignalAndWaitAsync()
        {
            lock (_barrierCheckGuard)
            {
                if (++_currentCount == _participantCount)
                {
                    if (_participantCount > 1)
                    {
                        _tcs.SetResult(null!);
                        _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                    _currentCount = 0;
                    return Task.CompletedTask;
                }
                return _tcs.Task;
            }
        }

        private readonly int _participantCount;

        private int _currentCount;

        private TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly object _barrierCheckGuard = new object();
    }
}
