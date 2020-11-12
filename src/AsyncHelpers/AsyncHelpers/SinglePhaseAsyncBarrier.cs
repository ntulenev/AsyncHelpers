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
            {
                throw new ArgumentException("Value should be positive.", nameof(participantCount));
            }

            _participantCount = participantCount;
            _tcsItems = new List<TaskCompletionSource<object>>(participantCount);
        }

        public Task SignalAndWaitAsync()
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            AddToBarrierAndCheck(tcs);

            return tcs.Task;
        }

        private void AddToBarrierAndCheck(TaskCompletionSource<object> tcs)
        {
            lock (_barrierCheckGuard)
            {
                _tcsItems.Add(tcs);

                if (_tcsItems.Count == _participantCount)
                {
                    foreach (var item in _tcsItems)
                    {
                        item.SetResult(null!);
                    }

                    _tcsItems.Clear();
                }
            }
        }

        private readonly object _barrierCheckGuard = new object();

        private readonly List<TaskCompletionSource<object>> _tcsItems;

        private readonly int _participantCount;
    }
}
