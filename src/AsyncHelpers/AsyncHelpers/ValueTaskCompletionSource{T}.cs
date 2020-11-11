using System;
using System.Threading.Tasks;

namespace AsyncHelpers
{
    // Early version need to fix

    public class ValueTaskCompletionSource<T>
    {
        public bool TrySetResult(T result) => _vts.TrySetResult(result, _vts.Version);

        public void SetResult(T result)
        {
            if (!TrySetResult(result))
                throw new InvalidOperationException();
        }

        public bool TrySetCanceled() => _vts.TrySetCanceled(_vts.Version);

        public void SetCanceled()
        {
            if (!TrySetCanceled())
                throw new InvalidOperationException();
        }

        public bool TrySetException(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            return _vts.TrySetException(ex, _vts.Version);
        }

        public void SetException(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            if (!TrySetException(ex))
                throw new InvalidOperationException();
        }

        public ValueTaskCompletionSource(bool runContinuationsAsynchronously)
        {
            _vts = new ValueTaskCompletionSourceInternal<T>(runContinuationsAsynchronously);
        }

        public ValueTask<T> Task => _vts.Task;

        private ValueTaskCompletionSourceInternal<T> _vts;
    }
}
