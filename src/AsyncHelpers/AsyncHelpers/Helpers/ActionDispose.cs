namespace AsyncHelpers.Helpers;

/// <summary>
/// Container for action that fill be run on container dispose.
/// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
public class ActionDispose : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
{
    /// <summary>
    /// Creates <see cref="ActionDispose"/>.
    /// </summary>
    /// <param name="disposeAction">Action that will de run on dispose.</param>
    /// <exception cref="ArgumentNullException">Throws if action is null.</exception>
    public ActionDispose(Action disposeAction)
    {
        ArgumentNullException.ThrowIfNull(disposeAction);
        _disposeAction = disposeAction;
    }

    /// <inheritdoc/>
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public virtual void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        if (!_isDisposed)
        {
            RunDisposeAction();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Runs the dispose action if the object has not been disposed.
    /// </summary>
    private void RunDisposeAction()
    {
        ThrowIfDisposed();
        _disposeAction();
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the object has been disposed.
    /// </summary>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, GetType());

    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _isDisposed;
    private readonly Action _disposeAction;
}
