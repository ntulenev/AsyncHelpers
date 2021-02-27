using System;

using FluentAssertions;

using Xunit;

namespace AsyncHelpers.Tests
{
    public class ActionDisposeTests
    {
        [Fact(DisplayName = "The ActionDispose can't be constructed with null action.")]
        [Trait("Category", "Unit")]
        public void CantBeConstructedNullAction()
        {
            // Arrange
            Action action = null!;

            // Act
            var exception = Record.Exception(
                () => new ActionDispose(action));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "The ActionDispose can be constructed with any action.")]
        [Trait("Category", "Unit")]
        public void CantBeConstructedAnyAction()
        {
            // Arrange
            static void action() { }

            // Act
            var exception = Record.Exception(
                () => new ActionDispose(action));

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The ActionDispose don't run action before dispose.")]
        [Trait("Category", "Unit")]
        public void DontRunActionBeforeDispose()
        {
            // Arrange
            bool isDisposed = false;
            void action() { isDisposed = true; }

            // Act
            var result = new ActionDispose(action);

            // Assert
            isDisposed.Should().BeFalse();
        }

    }
}
