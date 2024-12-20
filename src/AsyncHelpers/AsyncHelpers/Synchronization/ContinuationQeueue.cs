﻿namespace AsyncHelpers.Synchronization;

/// <summary>
/// Queue that registers continuations and runs them one be one.
/// </summary>
public sealed class ContinuationQueue
{
    /// <summary>
    /// Register task for continuation.
    /// </summary>
    public Task WaitAsync()
    {
        lock (_queueGuard)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _queue.Enqueue(tcs);
            return tcs.Task;
        }
    }

    /// <summary>
    /// Set task as finished to run continuation.
    /// </summary>
    /// <exception cref=" InvalidOperationException">Throws if no tasks in queue.</exception>
    public void FinishTask()
    {
        lock (_queueGuard)
        {
            if (_queue.Any())
            {
                var tcs = _queue.Dequeue();
                tcs.SetResult();
            }
            else
            {
                throw new InvalidOperationException("No tasks in queue");
            }
        }
    }

    private readonly Lock _queueGuard = new();
    private readonly Queue<TaskCompletionSource> _queue = new();
}
