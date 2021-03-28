using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

using AsyncHelpers.Synchronization;

namespace AsyncHelpers.Tests
{
    public class RWAsyncDAGVertexTests
    {
        [Fact(DisplayName = "The AsyncLockDAGVertex could be constructed.")]
        [Trait("Category", "Unit")]
        public void CantBeConstructed()
        {
            // Act
            var exception = Record.Exception(
                () => new AsyncLockDAGVertex());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "The RWAsyncDAGVertex could be validated without edges.")]
        [Trait("Category", "Unit")]
        public void SingleNodeCouldBeValidated()
        {
            // Arrange
            var vertex = new AsyncLockDAGVertex();

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
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();

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
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();

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
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();

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
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();

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
            var vertex1 = new AsyncLockDAGVertex();
            AsyncLockDAGVertex vertex2 = null!;

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
            var vertex1 = new AsyncLockDAGVertex();
            AsyncLockDAGVertex[] vertexArray = null!;

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
            var vertex1 = new AsyncLockDAGVertex();
            AsyncLockDAGVertex[] vertexArray = Array.Empty<AsyncLockDAGVertex>();

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
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            vertex1.AddEdgesTo(vertex2);

            // Act
            var exception = Record.Exception(
                () => vertex1.AddEdgesTo(vertex2));

            // Assert
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
        }

        [Fact(DisplayName = "Write lock on single node could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new AsyncLockDAGVertex();

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex.GetLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Second Write lock on single node could not be taken.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeSecondWriteLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new AsyncLockDAGVertex();
            var firstLock = await vertex.GetLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetLockAsync(CancellationToken.None);

            // Assert
            secondLockTask.IsCompleted.Should().BeFalse();

            firstLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            secondLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Write lock on single node of graph could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnSingleNodeOfGraphAsync()
        {
            // Arrange
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex1.GetLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on last node of graph could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnLastNodeOfGraphAsync()
        {
            // Arrange
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex4.GetLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on last node of graph could not be taken if first has write lock.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeWriteLockOnLastNodeOfGraphAsync()
        {
            // Arrange
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var firstLock = await vertex1.GetLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var lastLockTask = vertex4.GetLockAsync(CancellationToken.None);

            // Assert
            lastLockTask.IsCompleted.Should().BeFalse();

            firstLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            lastLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Write lock on first node of graph could not be taken if last has write lock.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeWriteLockOnFirstNodeOfGraphAsync()
        {
            // Arrange
            var vertex1 = new AsyncLockDAGVertex();
            var vertex2 = new AsyncLockDAGVertex();
            var vertex3 = new AsyncLockDAGVertex();
            var vertex4 = new AsyncLockDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var lastLock = await vertex4.GetLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var firstLockTask = vertex1.GetLockAsync(CancellationToken.None);

            // Assert
            firstLockTask.IsCompleted.Should().BeFalse();

            lastLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            firstLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Locks is cancelled if token is cancelled.")]
        [Trait("Category", "Unit")]
        public async Task LocksIfCanceledIfTokenIsCancelled()
        {

            var cts = new CancellationTokenSource();

            // Arrange
            var vertex = new AsyncLockDAGVertex();

            _ = await vertex.GetLockAsync(cts.Token).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetLockAsync(cts.Token);

            // Assert
            secondLockTask.IsCompleted.Should().BeFalse();

            cts.Cancel();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            secondLockTask.IsCanceled.Should().BeTrue();
        }

        //3 items tests
        //Lock on parent locks all childs
        //Lock on one child locks parent but dont locks other child

    }
}
