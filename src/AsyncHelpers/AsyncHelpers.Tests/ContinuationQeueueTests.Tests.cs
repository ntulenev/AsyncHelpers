using System;

using AsyncHelpers.Synchronization;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests
{
    public class ContinuationQeueueTests
    {
        [Fact(DisplayName = "FinishTask on empty queue throws exception.")]
        [Trait("Category", "Unit")]
        public void CanNotRunFinishTaskOnEmptyQueue()
        {
            //Arrange
            ContinuationQueue continuationQueue = new ContinuationQueue();

            // Act
            var exception = Record.Exception(
                () => continuationQueue.FinishTask());

            // Assert
            exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
        }

        [Fact(DisplayName = "WaitAsync returns not complete task.")]
        [Trait("Category", "Unit")]
        public void TaskFromQueueIsNotComplete()
        {
            //Arrange
            ContinuationQueue continuationQueue = new ContinuationQueue();

            // Act
            var t1 = continuationQueue.WaitAsync();

            // Assert
            t1.IsCompleted.Should().BeFalse();
        }

        [Fact(DisplayName = "WaitAsync task completes after finish calls.")]
        [Trait("Category", "Unit")]
        public void TaskFromQueueCompletesAfterFinish()
        {
            //Arrange
            ContinuationQueue continuationQueue = new ContinuationQueue();

            // Act
            var t1 = continuationQueue.WaitAsync();
            var t2 = continuationQueue.WaitAsync();
            continuationQueue.FinishTask();

            // Assert
            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeFalse();
        }
    }
}
