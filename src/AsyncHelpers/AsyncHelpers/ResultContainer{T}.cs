using System;

namespace AsyncHelpers
{
    /// <summary>
    /// Container that contains <see cref="RechargeableCompletionSource{T}"/> result. Container should be disposed after result is processed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultContainer<T> : IDisposable
    {
        /// <summary>
        /// Creates container for <see cref="RechargeableCompletionSource{T}"/> result.
        /// </summary>
        /// <param name="resetAction">logic that should be run on dispose stage</param>
        /// <param name="value">result</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resetAction"/> is null</exception>
        public ResultContainer(Action resetAction, T value)
        {
            if (resetAction is null)
                throw new ArgumentNullException(nameof(resetAction));

            _resetAction = resetAction;
            Value = value;
        }

        /// <summary>
        /// Result value
        /// </summary>
        public T Value { get; }

        private readonly Action _resetAction;

        /// <summary>
        /// Dispose container and reset <see cref="RechargeableCompletionSource{T}"/>
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _resetAction();
                _isDisposed = true;
            }
        }

        private bool _isDisposed;
    }
}
