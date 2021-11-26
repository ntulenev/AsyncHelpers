namespace AsyncHelpers.Helpers
{
    /// <summary>
    /// Container for action that fill be run on container dispose.
    /// </summary>
    public class ActionDispose : IDisposable
    {
        /// <summary>
        /// Creates <see cref="ActionDispose"/>.
        /// </summary>
        /// <param name="disposeAction">Action that will de run on dispose.</param>
        /// <exception cref="ArgumentNullException">Throws if action is null.</exception>
        public ActionDispose(Action disposeAction)
        {
            if (disposeAction is null)
            {
                throw new ArgumentNullException(nameof(disposeAction));
            }

            _disposeAction = disposeAction;
        }

        private readonly Action _disposeAction;

        /// <inheritdoc/>
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public virtual void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            if (!_isDisposed)
            {
                RunDisposeAction();
                _isDisposed = true;
            }
        }

        protected void RunDisposeAction()
        {
            ThrowIfDisposed();
            _disposeAction();
        }

        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        protected bool _isDisposed;
    }
}
