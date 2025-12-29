namespace AsyncHelpers.TaskProducers;

/// <summary>
/// <see cref="TaskCompletionSource{TResult}"/> that could be recharge.
/// </summary>
/// <typeparam name="T"></typeparam>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public sealed class RechargeableCompletionSource<T>
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{

    /// <summary>
    /// Creates instance of <see cref="RechargeableCompletionSource{T}"/>.
    /// </summary>
    /// <param name="runContinuationsAsynchronously">true is we need 
    /// <see cref="TaskCompletionSource{TResult}"/> async continuation.</param>
    public RechargeableCompletionSource(bool runContinuationsAsynchronously)
    {
        _runContinuationsAsynchronously = runContinuationsAsynchronously;
        CreateCompletionSource();
    }

    /// <summary>
    /// Sets result and blocks until result is awaited and processed.
    /// </summary>
    /// <param name="data">result</param>
    public void SetResultAndWait(T data)
    {
        lock (_setResultGuard)
        {
            _tcs.SetResult(data);
            _ = _are.WaitOne();
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
            _ = _are.Set();

        }, value);
    }

    private void CreateCompletionSource()
    {
        _tcs = _runContinuationsAsynchronously
            ? new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously)
            : new TaskCompletionSource<T>();
    }

    private TaskCompletionSource<T> _tcs = default!;
    private volatile bool _isValueInWork;
    private readonly bool _runContinuationsAsynchronously;
    private readonly AutoResetEvent _are = new(false);
    private readonly Lock _setResultGuard = new();
    private readonly Lock _getValueAsyncGuard = new();
}
