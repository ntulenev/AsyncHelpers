namespace AsyncHelpers.Helpers
{
    /// <summary>
    /// Extensions for tasks
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Waits all tasks but invoke <paramref name="OnFaulted"/> if any task is failed.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if any arg is null.</exception>
        /// <exception cref="ArgumentNullException">Throws if any arg is null.</exception>
        public static async Task WaitAllTasksButCheckAsync(this IEnumerable<Task> tasks, Action onFaulted)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            if (onFaulted == null)
            {
                throw new ArgumentNullException(nameof(onFaulted));
            }

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
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout should be positive.", nameof(timeout));
            }

            var resultTask = await Task.WhenAny(task, Task.Delay(timeout, ct)).ConfigureAwait(false);

            return resultTask == task;
        }
    }
}
