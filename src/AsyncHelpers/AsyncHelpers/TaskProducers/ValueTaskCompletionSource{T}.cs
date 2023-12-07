namespace AsyncHelpers.TaskProducers;

/// <summary>
/// <see cref="ValueTask{TResult}"/> implemetation of <see cref="TaskCompletionSource{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">Type of Task result.</typeparam>
/// <remarks>
/// Creates <see cref="ValueTaskCompletionSource{TResult}"/>.
/// </remarks>
/// <param name="runContinuationsAsynchronously">true is we need async continuation.</param>
public class ValueTaskCompletionSource<TResult>(bool runContinuationsAsynchronously)
{
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
        {
            throw new InvalidOperationException();
        }
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
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Attempt to completes with a error.
    /// </summary>
    public bool TrySetException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return _vts.TrySetException(ex, _vts.Version);
    }

    /// <summary>
    /// Completes with a error.
    /// </summary>
    public void SetException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        if (!TrySetException(ex))
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// ValueTask that returns for awating.
    /// </summary>
    public ValueTask<TResult> Task => _vts.Task;

    private readonly ReusableValueTask<TResult> _vts = new(runContinuationsAsynchronously);
}
