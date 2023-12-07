using AsyncHelpers.Helpers;

namespace AsyncHelpers.TaskProducers;

/// <summary>
/// Container that contains <see cref="RechargeableCompletionSource{T}"/> result. 
/// Container should be disposed after result is processed.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates container for <see cref="RechargeableCompletionSource{T}"/> result.
/// </remarks>
/// <param name="resetAction">logic that should be run on dispose stage.</param>
/// <param name="value">result.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="resetAction"/> is null.</exception>
public class ResultContainer<T>(Action resetAction, T value) : ActionDispose(resetAction)
{
    /// <summary>
    /// Result value.
    /// </summary>
    public T Value { get; } = value;
}
