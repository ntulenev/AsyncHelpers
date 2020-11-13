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
        public static async Task WaitAllTaskButCheck(this IEnumerable<Task> tasks, Action OnFaulted)
        {
            if (OnFaulted == null)
                throw new ArgumentNullException(nameof(OnFaulted));

            var any = Task.WhenAny(tasks).Unwrap();
            _ = any.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    OnFaulted();
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
