using FluentAssertions;
using System;
using Xunit;

namespace AsyncHelpers.Tests
{
    public class ResultContainerTests
    {
        [Fact(DisplayName = "The ResultContainer can be constructed with null action.")]
        [Trait("Category", "Unit")]
        public void CantBeConstructedNullAction()
        {
            // Arrange
            Action action = null!;

            // Act
            var exception = Record.Exception(
                () => new ResultContainer<object>(action, null!));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "The ResultContainer can be constructed with any value.")]
        [Trait("Category", "Unit")]
        public void CanBeConstructedActionAndAnyValue()
        {
            // Arrange
            static void action() { }

            // Act
            var exception = Record.Exception(
                () => new ResultContainer<object>(action, default!));

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Correct value can be get from the ResultContainer.")]
        [Trait("Category", "Unit")]
        public void CanGetCorrectValueFromResult()
        {
            // Arrange
            static void action() { }
            object o = new object();

            // Act
            var result = new ResultContainer<object>(action, o);

            // Assert
            result.Value.Should().Be(o);
        }

        [Fact(DisplayName = "The ResultContainer runs action on dispose.")]
        [Trait("Category", "Unit")]
        public void CanRunActionOnDispose()
        {
            // Arrange
            bool isDisposed = false;
            void action() { isDisposed = true; }

            // Act
            var result = new ResultContainer<object>(action, null!);
            result.Dispose();
            // Assert

            isDisposed.Should().BeTrue();
        }

        [Fact(DisplayName = "The ResultContainer runs action on dispose only once.")]
        [Trait("Category", "Unit")]
        public void CanRunActionOnDisposeOnce()
        {
            // Arrange
            int countDispose = 0;
            void action() { countDispose++; }

            // Act
            var result = new ResultContainer<object>(action, null!);
            result.Dispose();
            result.Dispose();

            // Assert
            countDispose.Should().Be(1);
        }
    }
}
