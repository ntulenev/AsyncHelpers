using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncHelpers
{
    /// <summary>
    /// <see cref="TaskCompletionSource{TResult}"/> that could be recharge.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RechargeableCompletionSource<T>
    {

        /// <summary>
        /// Creates instance of <see cref="RechargeableCompletionSource{T}"/>.
        /// </summary>
        /// <param name="runContinuationsAsynchronously">true is we need <see cref="TaskCompletionSource{TResult}"/> async continuation</param>
        public RechargeableCompletionSource(bool runContinuationsAsynchronously)
        {
            _runContinuationsAsynchronously = runContinuationsAsynchronously;
            CreateCompletionSource();
        }

        /// <summary>
        /// Sets result and blocks until result is awaited and processed
        /// </summary>
        /// <param name="data">result</param>
        public void SetResultAndWait(T data)
        {
            lock (_setResultGuard)
            {
                _tcs.SetResult(data);
                _are.WaitOne();
            }
        }

        /// <summary>
        /// Gets value container <see cref="ResultContainer{T}"/>. Container should be disposed after result is processed.
        /// </summary>
        /// <returns></returns>
        public async Task<ResultContainer<T>> GetValueAsync()
        {
            lock (_getValueAsyncGuard)
            {
                if (_isValueInWork)
                {
                    throw new InvalidOperationException("ResultContainer was already got but not disposed yet.");
                }
                _isValueInWork = true;
            }

            var value = await _tcs.Task.ConfigureAwait(false);

            return new ResultContainer<T>(() =>
            {
                CreateCompletionSource();
                _isValueInWork = false;
                _are.Set();

            }, value);
        }

        private void CreateCompletionSource()
        {
            if (_runContinuationsAsynchronously)
                _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            else
                _tcs = new TaskCompletionSource<T>();
        }

        private TaskCompletionSource<T> _tcs = default!;

        private volatile bool _isValueInWork = false;

        private readonly bool _runContinuationsAsynchronously;

        private readonly AutoResetEvent _are = new AutoResetEvent(false);

        private readonly object _setResultGuard = new object();

        private readonly object _getValueAsyncGuard = new object();


    }
}
