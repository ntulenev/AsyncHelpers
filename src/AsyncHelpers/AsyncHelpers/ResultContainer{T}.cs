using System;
using System.Data;

namespace AsyncHelpers
{
    public class ResultContainer<T> : IDisposable
    {
        public ResultContainer(Action resetAction, T value)
        {
            if (resetAction is null)
                throw new ArgumentNullException(nameof(resetAction));

            _resetAction = resetAction;
            Value = value;
        }

        public T Value { get; }

        private readonly Action _resetAction;

        public void Dispose()
        {
            if (_isDisposed)
            {
                _resetAction();
                _isDisposed = true;
            }
        }

        private bool _isDisposed;
    }
}
