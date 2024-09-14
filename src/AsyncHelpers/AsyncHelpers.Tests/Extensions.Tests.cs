using FluentAssertions;

using Xunit;

using AsyncHelpers.Helpers;

namespace AsyncHelpers.Tests;

public class ExtensionsTests
{
    [Fact(DisplayName = "WaitAllTasksButCheck should throw exception when tasks are null.")]
    [Trait("Category", "Unit")]
    public async Task WaitAllTasksButCheckExceptionOnNullTasks()
    {
        //Arrange
        IEnumerable<Task> tasks = null!;

        // Act
        var exception = await Record.ExceptionAsync(
            () => Extensions.WaitAllTasksButCheckAsync(tasks, () => { }));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = "WaitAllTasksButCheck should throw exception when action is null.")]
    [Trait("Category", "Unit")]
    public async Task WaitAllTasksButCheckExceptionOnNullAction()
    {
        //Arrange
        IEnumerable<Task> tasks = [Task.CompletedTask];
        Action a = null!;

        // Act
        var exception = await Record.ExceptionAsync(
            () => Extensions.WaitAllTasksButCheckAsync(tasks, a));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = "WaitAllTasksButCheck should complete both tasks correct if no errors.")]
    [Trait("Category", "Unit")]
    public async Task WaitAllWithCorrectTasks()
    {
        //Arrange
        var tcs1 = new TaskCompletionSource();
        var t1 = tcs1.Task;

        var tcs2 = new TaskCompletionSource();
        var t2 = tcs2.Task;

        bool isFailed = false;

        // Act
        var task = Extensions.WaitAllTasksButCheckAsync([t1, t2], () => isFailed = true);
        var timeoutTask = Task.Delay(500); // Attempts to ensure that task will finish.

        tcs1.SetResult();
        tcs2.SetResult();

        var result = await Task.WhenAny(task, timeoutTask);

        // Assert
        isFailed.Should().BeFalse();
        result.Should().Be(task);
    }

    [Fact(DisplayName = "WaitAllTasksButCheck should run action of any task failed.")]
    [Trait("Category", "Unit")]
    public async Task WaitAllWithFailedTasks()
    {
        //Arrange
        var tcs1 = new TaskCompletionSource();
        var t1 = tcs1.Task;

        var tcs2 = new TaskCompletionSource();
        var t2 = tcs2.Task;

        bool isFailed = false;

        // Act
        var task = Extensions.WaitAllTasksButCheckAsync([t1, t2], () =>
        {
            isFailed = true;
        });

        tcs1.SetException(new InvalidOperationException());

        await Task.Delay(100); // Attempts to ensure that continuation action runs.

        // Assert
        isFailed.Should().BeTrue();
        task.IsCompleted.Should().BeFalse();
    }

    [Fact(DisplayName = "TryExecuteWithTimeoutAsync should throw exception when task is null.")]
    [Trait("Category", "Unit")]
    public async Task TryExecuteWithTimeoutAsyncExceptionOnNullTasks()
    {
        //Arrange
        Task task = null!;
        var timeout = 1000;

        // Act
        var exception = await Record.ExceptionAsync(
            () => Extensions.TryExecuteWithTimeoutAsync(task, timeout));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
    }

    [Theory(DisplayName = "TryExecuteWithTimeoutAsync should throw exception when timeout is wrong.")]
    [Trait("Category", "Unit")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TryExecuteWithTimeoutAsyncExceptionWrongTimeout(int timeout)
    {
        //Arrange
        Task task = Task.CompletedTask;

        // Act
        var exception = await Record.ExceptionAsync(
            () => Extensions.TryExecuteWithTimeoutAsync(task, timeout));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
    }

    [Fact(DisplayName = "TryExecuteWithTimeoutAsync works properly with completed task.")]
    [Trait("Category", "Unit")]
    public async Task TryExecuteWithTimeoutWorksOnCompletedTask()
    {
        //Arrange
        Task task = Task.CompletedTask!;
        var timeout = 1000;
        bool result = false;

        // Act
        var exception = await Record.ExceptionAsync(
            async () => result = await Extensions.TryExecuteWithTimeoutAsync(task, timeout));

        // Assert
        exception.Should().BeNull();
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "TryExecuteWithTimeoutAsync works properly with uncompleted task.")]
    [Trait("Category", "Unit")]
    public async Task TryExecuteWithTimeoutWorksOnUncompletedTask()
    {
        //Arrange
        Task task = new TaskCompletionSource().Task;
        var timeout = 1000;
        bool result = true;

        // Act
        var exception = await Record.ExceptionAsync(
            async () => result = await Extensions.TryExecuteWithTimeoutAsync(task, timeout));

        // Assert
        exception.Should().BeNull();
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "WithCancellation can't be run on null task.")]
    [Trait("Category", "Unit")]
    public async Task WithCancellationCantRunNullTask()
    {

        // Act
        var exception = await Record.ExceptionAsync(
            async () => _ = await Extensions.WithCancellation((Task<object>)null!, CancellationToken.None));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = "WithCancellation runs on finished task.")]
    [Trait("Category", "Unit")]
    public async Task WithCancellationAndTask()
    {
        // Arrange
        var exceptedResult = 42;
        var tcs = new TaskCompletionSource<int>();
        tcs.SetResult(exceptedResult);

        using var cts = new CancellationTokenSource();

        // Act
        var result = await Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        result.Should().Be(exceptedResult);
    }

    [Fact(DisplayName = "WithCancellation runs on finished task 2.")]
    [Trait("Category", "Unit")]
    public void WithCancellationAndTask2()
    {
        // Arrange
        var exceptedResult = 42;
        var tcs = new TaskCompletionSource<int>();
        tcs.SetResult(exceptedResult);

        using var cts = new CancellationTokenSource();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.Should().Be(tcs.Task);
    }

    [Fact(DisplayName = "WithCancellation runs on error task.")]
    [Trait("Category", "Unit")]
    public void WithCancellationAndTaskError()
    {
        // Arrange

        var tcs = new TaskCompletionSource<int>();
        tcs.SetException(new InvalidOperationException());

        using var cts = new CancellationTokenSource();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.Should().Be(tcs.Task);
    }

    [Fact(DisplayName = "WithCancellation runs on cancel task.")]
    [Trait("Category", "Unit")]
    public void WithCancellationAndTaskCancel()
    {
        // Arrange

        var tcs = new TaskCompletionSource<int>();
        tcs.SetCanceled();

        using var cts = new CancellationTokenSource();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.Should().Be(tcs.Task);
    }

    [Fact(DisplayName = "WithCancellation runs on canceled token.")]
    [Trait("Category", "Unit")]
    public void WithCancellationOnCancelledToken()
    {
        // Arrange

        var tcs = new TaskCompletionSource<int>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.Should().NotBe(tcs.Task);
        task.IsCanceled.Should().BeTrue();
    }

    [Fact(DisplayName = "WithCancellation runs after canceled token.")]
    [Trait("Category", "Unit")]
    public async Task WithCancellationAfterCancelledToken()
    {
        // Arrange

        var tcs = new TaskCompletionSource<int>();

        using var cts = new CancellationTokenSource();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.IsCanceled.Should().BeFalse();

        cts.Cancel();

        var exception = await Record.ExceptionAsync(async () => await task);

        exception.Should().NotBeNull().And.BeOfType<OperationCanceledException>();
    }

    [Fact(DisplayName = "WithCancellation completed.")]
    [Trait("Category", "Unit")]
    public void WithCancellationCompleted()
    {
        // Arrange
        var value = 42;
        var tcs = new TaskCompletionSource<int>();

        using var cts = new CancellationTokenSource();

        // Act
        var task = Extensions.WithCancellation(tcs.Task, cts.Token);

        // Assert
        task.IsCanceled.Should().BeFalse();

        tcs.SetResult(value);

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        task.GetAwaiter().GetResult().Should().Be(value);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
    }

    [Fact(DisplayName = "WhenAllOrError can't be run on null tasks.")]
    [Trait("Category", "Unit")]
    public async Task WhenAllOrErrorCantCheckNullTasks()
    {

        // Act
        var exception = await Record.ExceptionAsync(
            async () => _ = await Extensions.WhenAllOrError((Task<object>[])null!));

        // Assert
        exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = "WhenAllOrError finished on completed tasks.")]
    [Trait("Category", "Unit")]
    public async Task WhenAllOrErrorFinishOnCompletedTasks()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource<int>();
        var tcs2 = new TaskCompletionSource<int>();
        tcs1.SetResult(1);
        tcs2.SetResult(2);

        // Act
        var result = await Extensions.WhenAllOrError(tcs1.Task, tcs2.Task);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain([1, 2]);
    }

    [Fact(DisplayName = "WhenAllOrError finished on normal tasks.")]
    [Trait("Category", "Unit")]
    public void WhenAllOrErrorFinishOnNormalTasks()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource<int>();
        var tcs2 = new TaskCompletionSource<int>();


        // Act
        var resultTask = Extensions.WhenAllOrError(tcs1.Task, tcs2.Task);

        // Assert
        resultTask.IsCompleted.Should().BeFalse();

        tcs1.SetResult(1);
        tcs2.SetResult(2);

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        var result = resultTask.GetAwaiter().GetResult();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        result.Should().HaveCount(2);
        result.Should().Contain([1, 2]);
    }

    [Fact(DisplayName = "WhenAllOrError stops on error.")]
    [Trait("Category", "Unit")]
    public async Task WhenAllOrErrorStopsOnError()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource<int>();
        var tcs2 = new TaskCompletionSource<int>();


        // Act
        var resultTask = Extensions.WhenAllOrError(tcs1.Task, tcs2.Task);
        tcs2.SetException(new InvalidOperationException());
        var exception = await Record.ExceptionAsync(async () => await resultTask);

        // Assert
        exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
    }

    [Fact(DisplayName = "WhenAllOrError stops on cancel.")]
    [Trait("Category", "Unit")]
    public async Task WhenAllOrErrorStopsOnCancel()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource<int>();
        var tcs2 = new TaskCompletionSource<int>();

        // Act
        var resultTask = Extensions.WhenAllOrError(tcs1.Task, tcs2.Task);
        tcs2.SetCanceled();
        var exception = await Record.ExceptionAsync(async () => await resultTask);

        // Assert
        exception.Should().NotBeNull().And.BeOfType<TaskCanceledException>();
    }

    [Fact(DisplayName = "WhenAllOrError stops on several errors.")]
    [Trait("Category", "Unit")]
    public async Task WhenAllOrErrorStopsOnSeveralErrors()
    {
        // Arrange
        var tcs1 = new TaskCompletionSource<int>();
        var tcs2 = new TaskCompletionSource<int>();
        var tcs3 = new TaskCompletionSource<int>();

        // Act
        var resultTask = Extensions.WhenAllOrError(tcs1.Task, tcs2.Task, tcs3.Task);
        tcs1.SetException(new InvalidOperationException());
        tcs2.SetCanceled();
        var exception = await Record.ExceptionAsync(async () => await resultTask);

        // Assert
        (exception is InvalidOperationException || exception is TaskCanceledException).Should().BeTrue();
    }
}
