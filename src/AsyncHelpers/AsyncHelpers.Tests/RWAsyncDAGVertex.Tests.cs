using System;

using FluentAssertions;

using Xunit;

using AsyncHelpers.Synchronization;

namespace AsyncHelpers.Tests
{
    public class RWAsyncDAGVertexTests
    {
        [Fact(DisplayName = "The RWAsyncDAGVertex could be constructed.")]
        [Trait("Category", "Unit")]
        public void CantBeConstructed()
        {
            // Act
            var exception = Record.Exception(
                () => new RWAsyncDAGVertex());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The RWAsyncDAGVertex could be validated without edges.")]
        [Trait("Category", "Unit")]
        public void SingleNodeCouldBeValidated()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();

            // Act
            var exception = Record.Exception(
                () => vertex.ValidateGraph());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The single edge graph could be validated.")]
        [Trait("Category", "Unit")]
        public void SingleEdgeGraphCouldBeValidated()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();

            vertex1.AddEdgesTo(vertex2);

            // Act
            var exception = Record.Exception(
                () => vertex1.ValidateGraph());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The graph could be validated.")]
        [Trait("Category", "Unit")]
        public void GraphCouldBeValidated()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();

            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            // Act
            var exception = Record.Exception(
                () => vertex1.ValidateGraph());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The graph could not be validated if loop exists.")]
        [Trait("Category", "Unit")]
        public void GraphWithLoopCouldNotBeValidated()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();

            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);
            vertex4.AddEdgesTo(vertex1);

            // Act
            var exception = Record.Exception(
                () => vertex1.ValidateGraph());

            // Assert
            exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
        }
    }
}
