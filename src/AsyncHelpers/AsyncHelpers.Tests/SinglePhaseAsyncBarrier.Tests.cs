using System;

using AsyncHelpers.Synchronization;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests
{
    public class SinglePhaseAsyncBarrierTests
    {
        [Fact(DisplayName = "SinglePhaseAsyncBarrier could be created")]
        [Trait("Category", "Unit")]
        public void SinglePhaseAsyncBarrierCouldBeCreated()
        {
            //Arrange
            var participantCount = 1;

            // Act
            var exception = Record.Exception(
                () => new SinglePhaseAsyncBarrier(participantCount));

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "SinglePhaseAsyncBarrier could not be created with incorrect participantCount")]
        [Trait("Category", "Unit")]
        public void SinglePhaseAsyncBarrierCouldNotBeCreated()
        {
            //Arrange
            var participantCount = 0;

            // Act
            var exception = Record.Exception(
                () => new SinglePhaseAsyncBarrier(participantCount));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }

        [Fact(DisplayName = "SinglePhaseAsyncBarrier could finished immediate with single participant")]
        [Trait("Category", "Unit")]
        public void SinglePhaseAsyncShouldFinishedImmediateWithSingleParticipant()
        {
            //Arrange
            var participantCount = 1;
            var barrier = new SinglePhaseAsyncBarrier(participantCount);

            // Act
            var t = barrier.SignalAndWaitAsync();

            // Assert
            t.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "SinglePhaseAsyncBarrier could return not finished task if not all signal")]
        [Trait("Category", "Unit")]
        public void SinglePhaseAsyncShouldRetornNotFinishedInNotAllSignal()
        {
            //Arrange
            var participantCount = 2;
            var barrier = new SinglePhaseAsyncBarrier(participantCount);

            // Act
            var t = barrier.SignalAndWaitAsync();

            // Assert
            t.IsCompleted.Should().BeFalse();
        }

        [Fact(DisplayName = "SinglePhaseAsyncBarrier could return finished task if all signal")]
        [Trait("Category", "Unit")]
        public void SinglePhaseAsyncShouldRetornFinishedInAllSignal()
        {
            //Arrange
            var participantCount = 2;
            var barrier = new SinglePhaseAsyncBarrier(participantCount);

            // Act
            var t1 = barrier.SignalAndWaitAsync();
            var t2 = barrier.SignalAndWaitAsync();

            // Assert
            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeTrue();
        }
    }
}
