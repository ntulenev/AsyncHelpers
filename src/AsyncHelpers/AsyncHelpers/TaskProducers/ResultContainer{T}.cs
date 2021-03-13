using System;

using AsyncHelpers.Helpers;

namespace AsyncHelpers.TaskProducers
{
    /// <summary>
    /// Container that contains <see cref="RechargeableCompletionSource{T}"/> result. Container should be disposed after result is processed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultContainer<T> : ActionDispose
    {
        /// <summary>
        /// Creates container for <see cref="RechargeableCompletionSource{T}"/> result.
        /// </summary>
        /// <param name="resetAction">logic that should be run on dispose stage.</param>
        /// <param name="value">result.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resetAction"/> is null.</exception>
        public ResultContainer(Action resetAction, T value) : base(resetAction)
        {
            Value = value;
        }

        /// <summary>
        /// Result value.
        /// </summary>
        public T Value { get; }
    }
}
