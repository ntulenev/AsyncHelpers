using System;
using System.Threading.Tasks;

namespace AsyncHelpers
{

    /// <summary>
    /// <see cref="ValueTask{TResult}"/> implemetation of <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">Type of Task result.</typeparam>
    public class ValueTaskCompletionSource<TResult>
    {
        /// <summary>
        /// Creates <see cref="ValueTaskCompletionSource{TResult}"/>.
        /// </summary>
        /// <param name="runContinuationsAsynchronously">true is we need async continuation.</param>
        public ValueTaskCompletionSource(bool runContinuationsAsynchronously)
        {
            _vts = new ReusableValueTask<TResult>(runContinuationsAsynchronously);
        }

        /// <summary>
        /// Attempt to completes with a successful result.
        /// </summary>
        public bool TrySetResult(TResult result) => _vts.TrySetResult(result, _vts.Version);

        /// <summary>
        /// Completes with a successful result.
        /// </summary>
        public void SetResult(TResult result)
        {
            if (!TrySetResult(result))
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Attempt to completes with a cancellation.
        /// </summary>
        public bool TrySetCanceled() => _vts.TrySetCanceled(_vts.Version);

        /// <summary>
        /// Completes with a cancellation.
        /// </summary>
        public void SetCanceled()
        {
            if (!TrySetCanceled())
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Attempt to completes with a error.
        /// </summary>
        public bool TrySetException(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            return _vts.TrySetException(ex, _vts.Version);
        }

        /// <summary>
        /// Completes with a error.
        /// </summary>
        public void SetException(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (!TrySetException(ex))
                throw new InvalidOperationException();
        }

        public ValueTask<TResult> Task => _vts.Task;

        private readonly ReusableValueTask<TResult> _vts;
    }
}
