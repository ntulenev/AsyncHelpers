﻿namespace AsyncHelpers.Synchronization;

/// <summary>
/// Single phase async analogue of <see cref="System.Threading.Barrier"/>.
/// </summary>
public sealed class SinglePhaseAsyncBarrier
{
    /// <summary>
    /// Creates barrier for <paramref name="participantCount"/> participants.
    /// </summary>
    /// <param name="participantCount">Count of the participants.</param>
    /// <exception cref="ArgumentException">Throws exception if participantCount is incorrect.</exception>
    public SinglePhaseAsyncBarrier(int participantCount)
    {
        if (participantCount <= 0)
        {
            throw new ArgumentException("Value should be positive.", nameof(participantCount));
        }

        _participantCount = participantCount;
    }

    /// <summary>
    /// Signals about new participant and returns the task that will be finished when all participants will be added.
    /// </summary>
    public Task SignalAndWaitAsync()
    {
        lock (_barrierCheckGuard)
        {
            if (++_currentCount == _participantCount)
            {
                if (_participantCount > 1)
                {
                    _tcs.SetResult();
                    _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                }
                _currentCount = 0;
                return Task.CompletedTask;
            }
            return _tcs.Task;
        }
    }

    private readonly int _participantCount;
    private int _currentCount;
    private TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Lock _barrierCheckGuard = new();
}
