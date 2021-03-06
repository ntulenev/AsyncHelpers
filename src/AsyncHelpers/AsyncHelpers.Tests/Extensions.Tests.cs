﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

using AsyncHelpers.Helpers;

namespace AsyncHelpers.Tests
{
    public class ExtensionsTests
    {
        [Fact(DisplayName = "WaitAllTasksButCheck shoud thow exception when tasks are null.")]
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

        [Fact(DisplayName = "WaitAllTasksButCheck shoud thow exception when action is null.")]
        [Trait("Category", "Unit")]
        public async Task WaitAllTasksButCheckExceptionOnNullAction()
        {
            //Arrange
            IEnumerable<Task> tasks = new[] { Task.CompletedTask };
            Action a = null!;

            // Act
            var exception = await Record.ExceptionAsync(
                () => Extensions.WaitAllTasksButCheckAsync(tasks, a));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "WaitAllTasksButCheck shoud complete both tasks correct if no errors.")]
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
            var task = Extensions.WaitAllTasksButCheckAsync(new[] { t1, t2 }, () => isFailed = true);
            var timeoutTask = Task.Delay(500); // Attempts to ensure that task will finish.

            tcs1.SetResult();
            tcs2.SetResult();

            var result = await Task.WhenAny(task, timeoutTask);

            // Assert
            isFailed.Should().BeFalse();
            result.Should().Be(task);
        }

        [Fact(DisplayName = "WaitAllTasksButCheck shoud run action of any task failed.")]
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
            var task = Extensions.WaitAllTasksButCheckAsync(new[] { t1, t2 }, () =>
            {
                isFailed = true;
            });

            tcs1.SetException(new InvalidOperationException());

            await Task.Delay(100); // Attempts to ensure that continuation action runs.

            // Assert
            isFailed.Should().BeTrue();
            task.IsCompleted.Should().BeFalse();
        }

        [Fact(DisplayName = "TryExecuteWithTimeoutAsync shoud thow exception when task is null.")]
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

        [Theory(DisplayName = "TryExecuteWithTimeoutAsync shoud thow exception when timeout is wrong.")]
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
    }
}
