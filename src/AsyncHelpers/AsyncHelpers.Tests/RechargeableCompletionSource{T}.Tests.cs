using AsyncHelpers.TaskProducers;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests;

public class RechargeableCompletionSourceTests
{
    [Theory(DisplayName = "The RechargeableCompletionSource can be constructed")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "Unit")]
    public void CanBeConstructedActionAndAnyValue(bool runContinuationsAsynchronously)
    {
        // Act
        var exception = Record.Exception(
            () => new RechargeableCompletionSource<object>(runContinuationsAsynchronously));

        // Assert
        exception.Should().BeNull();
    }

    [Theory(DisplayName = "The RechargeableCompletionSource throws exception if run GetValueAsync twice")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "Unit")]
    public void RechargeableCompletionSourceThrowsErrorOnGetValueAsync(bool runContinuationsAsynchronously)
    {
        // Arrange
        var rcs = new RechargeableCompletionSource<object>(runContinuationsAsynchronously);

        // Act
        var t1 = rcs.GetValueAsync();
        var t2 = rcs.GetValueAsync();

        // Assert
        t1.IsFaulted.Should().BeFalse();
        t2.IsFaulted.Should().BeTrue();
    }

    [Theory(DisplayName = "The RechargeableCompletionSource blocks set until get result and dispose (case with await after)")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "Unit")]
    public async Task RechargeableCompletionSourceBlocksSetAwaitAfter(bool runContinuationsAsynchronously)
    {
        // Arrange
        var rcs = new RechargeableCompletionSource<object>(runContinuationsAsynchronously);
        var tFinishCheck = new TaskCompletionSource();

        // Act
        var t = Task.Run(() => rcs.SetResultAndWait(null!));
        _ = t.ContinueWith(_ => tFinishCheck.SetResult(), TaskContinuationOptions.ExecuteSynchronously);

        await Task.Delay(1_000); // Attempts to ensure that task t never completes.

        // Assert
        t.IsCompleted.Should().BeFalse();

        var result = await rcs.GetValueAsync();

        t.IsCompleted.Should().BeFalse();

        result.Dispose();

        await tFinishCheck.Task;

        t.IsCompleted.Should().BeTrue();
    }

    [Theory(DisplayName = "The RechargeableCompletionSource blocks set until get result and dispose (case with await after)")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "Unit")]
    public async Task RechargeableCompletionSourceBlocksSetAwaitBefore(bool runContinuationsAsynchronously)
    {
        // Arrange
        var rcs = new RechargeableCompletionSource<object>(runContinuationsAsynchronously);
        var tFinishCheck = new TaskCompletionSource();
        var result = rcs.GetValueAsync();

        // Act
        var t = Task.Run(() => rcs.SetResultAndWait(null!));
        _ = t.ContinueWith(_ => tFinishCheck.SetResult(), TaskContinuationOptions.ExecuteSynchronously);

        await Task.Delay(1_000); // Attempts to ensure that task t never completes iteself.

        // Assert
        t.IsCompleted.Should().BeFalse();
        result.IsCompleted.Should().BeTrue();

        result.Result.Dispose();

        await tFinishCheck.Task;

        t.IsCompleted.Should().BeTrue();
    }


}
