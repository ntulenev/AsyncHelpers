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
    }
}
