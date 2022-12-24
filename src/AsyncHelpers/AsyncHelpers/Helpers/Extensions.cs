namespace AsyncHelpers.Helpers;

/// <summary>
/// Extensions for tasks
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Waits all tasks but invoke <paramref name="OnFaulted"/> if any task is failed.
    /// </summary>
    /// <exception cref="ArgumentNullException">Throws if any arg is null.</exception>
    public static async Task WaitAllTasksButCheckAsync(this IEnumerable<Task> tasks, Action onFaulted)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        ArgumentNullException.ThrowIfNull(onFaulted);

        var any = Task.WhenAny(tasks).Unwrap();

        _ = any.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                onFaulted();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Waits all tasks but stops if any is failed or cancelled.
    /// </summary>
    /// <typeparam name="TResult">Task return type.</typeparam>
    /// <param name="tasks">Task to await.</param>
    /// <exception cref="ArgumentNullException">Throws if tasks is null.</exception>
    public static async Task<TResult[]> WhenAllOrError<TResult>(params Task<TResult>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var killTask = new TaskCompletionSource<TResult[]>();

        foreach (var task in tasks)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    _ = killTask.TrySetCanceled();
                }
                else if (t.IsFaulted)
                {
                    _ = killTask.TrySetException(t.Exception!.InnerException!);
                }
            });
        }

        return await await Task.WhenAny(killTask.Task, Task.WhenAll(tasks));
    }

    /// <summary>
    /// Attempts to execute async operation within the expected time.
    /// </summary>
    /// <param name="task">Task to execute.</param>
    /// <param name="timeout">timeout in ms.</param>
    /// <param name="ct">Token for cancel.</param>
    /// <returns>True if task was finished before timout.</returns>
    /// <exception cref="ArgumentNullException">Throws if task is null.</exception>
    /// <exception cref="ArgumentException">Throws if timeout is not positive.</exception>
    public static async Task<bool> TryExecuteWithTimeoutAsync(this Task task, int timeout, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (timeout <= 0)
        {
            throw new ArgumentException("Timeout should be positive.", nameof(timeout));
        }

        var resultTask = await Task.WhenAny(task, Task.Delay(timeout, ct)).ConfigureAwait(false);

        return resultTask == task;
    }

    /// <summary>
    /// Cancels awaiting of <paramref name="task"/> when <paramref name="cancellationToken"/> is canceled.
    /// </summary>
    /// <typeparam name="T">Type return type</typeparam>
    /// <param name="task">Task to cancell.</param>
    /// <param name="cancellationToken">cancellation token.</param>
    /// <exception cref="ArgumentNullException">Throws if task is null.</exception>
    public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (!cancellationToken.CanBeCanceled)
        {
            return task;
        }

        if (task.IsCompleted)
        {
            return task;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<T>(cancellationToken);
        }

        return WithCancellationCoreAsync(task, cancellationToken);
    }

    private static async Task<T> WithCancellationCoreAsync<T>(Task<T> task, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource();

        using var _ = ct.Register(() => tcs.SetResult());

        if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
        }

        return await task.ConfigureAwait(false);
    }
}
