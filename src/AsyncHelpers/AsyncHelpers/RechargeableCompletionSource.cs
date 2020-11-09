using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncHelpers
{

    public class RechargeableCompletionSource<T>
    {
        public RechargeableCompletionSource(bool runContinuationsAsynchronously)
        {
            _runContinuationsAsynchronously = runContinuationsAsynchronously;
            CreateCompletionSource();
        }

        private void CreateCompletionSource()
        {
            if (_runContinuationsAsynchronously)
                _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            else
                _tcs = new TaskCompletionSource<T>();
        }

        public void SetResultAndWait(T data)
        {
            lock (_setResultGuard)
            {
                _tcs.SetResult(data);
                _are.WaitOne();
            }
        }

        public async Task<ResultContainer<T>> GetValueAsync()
        {
            lock (_getValueAsyncGuard)
            {
                if (_isValueIsWork)
                {
                    throw new InvalidOperationException("ResultContainer was already got but not disposed yet");
                }
                _isValueIsWork = true;
            }

            await _tcs.Task.ConfigureAwait(false);

            return new ResultContainer<T>(() =>
            {
                CreateCompletionSource();
                _isValueIsWork = false;
                _are.Set();

            }, _tcs.Task.Result);
        }

        private TaskCompletionSource<T> _tcs = default!;

        private volatile bool _isValueIsWork = false;

        private readonly bool _runContinuationsAsynchronously;

        private readonly AutoResetEvent _are = new AutoResetEvent(false);

        private readonly object _setResultGuard = new object();
        private readonly object _getValueAsyncGuard = new object();


    }
}
