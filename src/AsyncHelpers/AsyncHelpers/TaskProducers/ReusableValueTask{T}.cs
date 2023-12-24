using System.Threading.Tasks.Sources;

namespace AsyncHelpers.TaskProducers;

/// <summary>
/// Special <see cref="IValueTaskSource"/> implementation for <see cref="ValueTaskCompletionSource{T}"/>.
/// </summary>
/// <typeparam name="TResult">Type of Task result.</typeparam>
internal sealed class ReusableValueTask<TResult> : IValueTaskSource<TResult>, IValueTaskSource
{
    /// <summary>
    /// Creates instance of the <see cref="ReusableValueTask{TResult}"/>.
    /// </summary>
    /// <param name="runContinuationsAsynchronously">true is we need async continuation.</param>
    public ReusableValueTask(bool runContinuationsAsynchronously)
    {
        _mre.RunContinuationsAsynchronously = runContinuationsAsynchronously;
    }

    /// <summary>
    /// Gets the result fo <see cref="ReusableValueTask{TResult}"/>.
    /// </summary>
    /// <param name="token">The result of the <see cref="ReusableValueTask{TResult}"/>.</param>
    public TResult GetResult(short token)
    {
        lock (_tokenGuard)
        {
            try
            {
                var status = _mre.GetStatus(token);
                if (status == ValueTaskSourceStatus.Canceled)
                {
                    throw new TaskCanceledException();
                }

                return _mre.GetResult(token);
            }
            finally
            {
                _mre.Reset();
            }
        }
    }

    /// <summary>
    /// Gets the result fo <see cref="ReusableValueTask{TResult}"/>.
    /// </summary>
    /// <param name="token">The result of the <see cref="ReusableValueTask{TResult}"/>.</param>
    void IValueTaskSource.GetResult(short token) => GetResult(token);

    /// <summary>
    /// Gets the status of the operation.
    /// </summary>
    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _mre.GetStatus(token);
    }

    /// <summary>
    /// Schedules the continuation action for this operation.
    /// </summary>
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        _mre.OnCompleted(continuation, state, token, flags);
    }

    /// <summary>
    /// Attempt to completes with a successful result.
    /// </summary>
    public bool TrySetResult(TResult result, short token)
    {
        lock (_tokenGuard)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetResult(result);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Attempt to completes with a cancellation.
    /// </summary>
    public bool TrySetCanceled(short token)
        => TrySetException(new TaskCanceledException(), token);

    /// <summary>
    /// Attempt to completes with a error.
    /// </summary>
    public bool TrySetException(Exception error, short token)
    {
        lock (_tokenGuard)
        {
            if (token == _mre.Version && _mre.GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                _mre.SetException(error);

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Creates <see cref="ValueTask{TResult}"/> for <see cref="ReusableValueTask{TResult}"/>.
    /// </summary>
    public ValueTask<TResult> Task => new ValueTask<TResult>(this, Version);

    /// <summary>
    /// Gets the operation version.
    /// </summary>
    public short Version => _mre.Version;

    private ManualResetValueTaskSourceCore<TResult> _mre;
    private readonly object _tokenGuard = new();
}
