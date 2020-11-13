using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncHelpers
{
    /// <summary>
    /// Extensions for tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits all tasks but invoke <paramref name="OnFaulted"/> if any task is failed
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if any arg is null</exception>
        /// <exception cref="ArgumentNullException">Throws if any arg is null</exception>
        public static async Task WaitAllTasksButCheck(this IEnumerable<Task> tasks, Action onFaulted)
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
    }
}
