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

        [Fact(DisplayName = "Edge could be added to the graph.")]
        [Trait("Category", "Unit")]
        public void CanAddEdgeToTheGraph()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertex2));

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Null edge could not be added to the graph.")]
        [Trait("Category", "Unit")]
        public void CantAddNullEdgeToTheGraph()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            RWAsyncDAGVertex vertex2 = null!;

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertex2));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }

        [Fact(DisplayName = "Null edges could not be added to the graph.")]
        [Trait("Category", "Unit")]
        public void CantAddNullEdgesToTheGraph()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            RWAsyncDAGVertex[] vertexArray = null!;

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertexArray));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentNullException>();
        }

        [Fact(DisplayName = "Empty edges array could not be added to the graph.")]
        [Trait("Category", "Unit")]
        public void CantAddEmptyEdgesToTheGraph()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            RWAsyncDAGVertex[] vertexArray = Array.Empty<RWAsyncDAGVertex>();

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertexArray));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }


        [Fact(DisplayName = "Duplicated edge could not be added to the graph.")]
        [Trait("Category", "Unit")]
        public void CantAddDuplicatedEdgeToTheGraph()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2);

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertex2));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }

        //TODO Test

        //1) 1 Single item Read lock 
        //2) 1 Single item Read lock + Read lock
        //3) 1 Single item Write lock
        //4) 1 Single item Write lock + Write lock
        //5) 1 Single item Write lock + Read lock
        //6) 1 Single item Read lock + Write lock

        //--5 Items graph (first and last item)--
        //7) First Write
        //8) First Read
        //9) Last Write
        //10) Last Read
        //11) First Write + Last Write
        //12) Last Write + First Write
        //13) First Read + Last Read
        //14) Last Read + First Read
        //15) First Read + Last Write
        //16) Last Read + First Write
        //17) First Write + Last Read
        //18) Last Write + First Read

        //19) Cancellation Token cancell case



    }
}
