﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests
{
    public class TaskExtensionsTests
    {
        [Fact(DisplayName = "WaitAllTasksButCheck shoud thow exception on tasks are null.")]
        [Trait("Category", "Unit")]
        public async Task WaitAllTasksButCheckExceptionOnNullTasks()
        {
            //Arrange
            IEnumerable<Task> tasks = null!;

            // Act
            var exception = await Record.ExceptionAsync(
                () => TaskExtensions.WaitAllTasksButCheck(tasks, () => { }));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "WaitAllTasksButCheck shoud thow exception on action is null.")]
        [Trait("Category", "Unit")]
        public async Task WaitAllTasksButCheckExceptionOnNullAction()
        {
            //Arrange
            IEnumerable<Task> tasks = new[] { Task.CompletedTask };
            Action a = null!;

            // Act
            var exception = await Record.ExceptionAsync(
                () => TaskExtensions.WaitAllTasksButCheck(tasks, a));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "WaitAllTasksButCheck shoud complete both tasks correct if no errors.")]
        [Trait("Category", "Unit")]
        public async Task WaitAllWithCorrectTasks()
        {
            //Arrange
            var tcs1 = new TaskCompletionSource<object>();
            var t1 = tcs1.Task;

            var tcs2 = new TaskCompletionSource<object>();
            var t2 = tcs2.Task;

            bool isFailed = false;

            // Act
            var task = TaskExtensions.WaitAllTasksButCheck(new[] { t1, t2 }, () => isFailed = true);
            var timeoutTask = Task.Delay(500); // Attempts to ensure that task will finish.

            tcs1.SetResult(null!);
            tcs2.SetResult(null!);

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
            var tcs1 = new TaskCompletionSource<object>();
            var t1 = tcs1.Task;

            var tcs2 = new TaskCompletionSource<object>();
            var t2 = tcs2.Task;

            bool isFailed = false;

            // Act
            var task = TaskExtensions.WaitAllTasksButCheck(new[] { t1, t2 }, () =>
            {
                isFailed = true;
            });

            tcs1.SetException(new InvalidOperationException());

            await Task.Delay(100); // Attempts to ensure that continuation action runs.

            // Assert
            isFailed.Should().BeTrue();
            task.IsCompleted.Should().BeFalse();
        }
    }
}
