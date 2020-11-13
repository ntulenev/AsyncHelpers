using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncHelpers
{

    //TODO Add Comments
    public class SinglePhaseAsyncBarrier
    {
        public SinglePhaseAsyncBarrier(int participantCount)
        {
            if (participantCount < 0)
                throw new ArgumentException("Value should be positive.", nameof(participantCount));

            _participantCount = participantCount;
        }

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
