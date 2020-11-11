using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace AsyncHelpers
{
    // Early version need to fix

    internal class ValueTaskCompletionSourceInternal<T> : IValueTaskSource<T>, IValueTaskSource
    {
        public T GetResult(short token)
        {
            try
            {
                var status = _mre.GetStatus(token);
                if (status == ValueTaskSourceStatus.Canceled)
                    throw new TaskCanceledException();
                return _mre.GetResult(token);
            }
            finally
            {
                _mre.Reset();
            }

        }

        void IValueTaskSource.GetResult(short token) => GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _mre.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _mre.OnCompleted(continuation, state, token, flags);
        }

        public bool TrySetResult(T result, short token)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetResult(result);
                return true;
            }
            return false;
        }

        public bool TrySetCanceled(short token)
            => TrySetException(new TaskCanceledException(), token);


        public bool TrySetException(Exception error, short token)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetException(error);
                return true;
            }
            return false;
        }

        public ValueTaskCompletionSourceInternal(bool runContinuationsAsynchronously)
        {
            _mre.RunContinuationsAsynchronously = runContinuationsAsynchronously;
        }

        public ValueTask<T> Task => new ValueTask<T>(this, Version);

        public short Version => _mre.Version;

        private ManualResetValueTaskSourceCore<T> _mre = new ManualResetValueTaskSourceCore<T>();

    }
}
