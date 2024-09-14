using AsyncHelpers.TaskProducers;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests;

public class ValueTaskCompletionSource
{
    [Theory(DisplayName = "The ValueTaskCompletionSource can be constructed.")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "Unit")]
    public void CanBeConstructedActionAndAnyValue(bool runContinuationsAsynchronously)
    {
        // Act
        var exception = Record.Exception(
            () => new ValueTaskCompletionSource<object>(runContinuationsAsynchronously));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "The ValueTaskCompletionSource returns not finished task.")]
    [Trait("Category", "Unit")]
    public void ReturnNotFinishedTask()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);

        // Act
        var t = vts.Task;

        // Assert
        t.IsCompleted.Should().BeFalse();
    }

    [Fact(DisplayName = "The ValueTaskCompletionSource returns finished task after result.")]
    [Trait("Category", "Unit")]
    public void ReturnFinishedTaskOnResult()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;

        // Act
        vts.SetResult(null!);

        // Assert
        t.IsCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "The ValueTaskCompletionSource returns cancel task after result.")]
    [Trait("Category", "Unit")]
    public void ReturnCancelTaskOnResult()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;

        // Act
        vts.SetCanceled();

        // Assert
        t.IsCanceled.Should().BeTrue();
    }

    [Fact(DisplayName = "The ValueTaskCompletionSource returns failed task after result.")]
    [Trait("Category", "Unit")]
    public void ReturnFailedTaskOnResult()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;

        // Act
        vts.TrySetException(new InvalidOperationException());

        // Assert
        t.IsFaulted.Should().BeTrue();
    }

    [Fact(DisplayName = "Double set result throws exception.")]
    [Trait("Category", "Unit")]
    public void DoubleSetResultThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        vts.SetResult(null!);

        // Act
        var exception = Record.Exception(
            () => vts.SetResult(null!));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
    }

    [Fact(DisplayName = "Double set cancel throws exception.")]
    [Trait("Category", "Unit")]
    public void DoubleSetCancelThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        vts.SetCanceled();

        // Act
        var exception = Record.Exception(
            () => vts.SetCanceled());

        // Assert
        exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
    }

    [Fact(DisplayName = "Double set failed throws exception.")]
    [Trait("Category", "Unit")]
    public void DoubleSetFailedThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        vts.SetException(new NotImplementedException());

        // Act
        var exception = Record.Exception(
            () => vts.SetException(new NotImplementedException()));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
    }


    [Fact(DisplayName = "Try Double set result not throws exception.")]
    [Trait("Category", "Unit")]
    public void TryDoubleSetResultNotThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        bool result1 = false;
        bool result2 = false;

        result1 = vts.TrySetResult(null!);

        // Act
        var exception = Record.Exception(
            () => result2 = vts.TrySetResult(null!));

        // Assert
        exception.Should().BeNull();
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }

    [Fact(DisplayName = "Double try set cancel not throws exception.")]
    [Trait("Category", "Unit")]
    public void TryDoubleSetCancelNotThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        bool result1 = false;
        bool result2 = false;

        result1 = vts.TrySetCanceled();

        // Act
        var exception = Record.Exception(
            () => result2 = vts.TrySetCanceled());

        // Assert
        exception.Should().BeNull();
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }

    [Fact(DisplayName = "Double try set failed not throws exception.")]
    [Trait("Category", "Unit")]
    public void TryDoubleSetFailedNotThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        bool result1 = false;
        bool result2 = false;

        result1 = vts.TrySetException(new NotImplementedException());

        // Act
        var exception = Record.Exception(
            () => result2 = vts.TrySetException(new NotImplementedException()));

        // Assert
        exception.Should().BeNull();
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }

    [Fact(DisplayName = "Unable to get result twice.")]
    [Trait("Category", "Unit")]
    public async Task DoubleGettingResultThrowsException()
    {
        //Assert
        var vts = new ValueTaskCompletionSource<object>(false);
        var t = vts.Task;
        vts.SetResult(null!);
        _ = await t;

        // Act
        var exception = await Record.ExceptionAsync(
            async () => _ = await t);

        // Assert
        exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
    }



}
