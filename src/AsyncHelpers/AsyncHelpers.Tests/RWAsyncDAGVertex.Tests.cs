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

        [Fact(DisplayName = "Write lock on single node could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on single node could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Second Write lock on single node could not be taken.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeSecondWriteLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();
            var firstLock = await vertex.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetWriteLockAsync(CancellationToken.None);

            // Assert
            secondLockTask.IsCompleted.Should().BeFalse();

            firstLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            secondLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Second Read lock on single node could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeSecondReadLockOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();
            _ = await vertex.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetReadLockAsync(CancellationToken.None);

            // Assert
            secondLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Second Read lock after Write on single node could not be taken.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeSecondReadLockAfterWriteOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();
            var firstLock = await vertex.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetReadLockAsync(CancellationToken.None);

            // Assert
            secondLockTask.IsCompleted.Should().BeFalse();

            firstLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            secondLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Second Write lock after Read on single node could not be taken.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeSecondWriteLockAfterReadOnSingleNodeAsync()
        {
            // Arrange
            var vertex = new RWAsyncDAGVertex();
            var firstLock = await vertex.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetWriteLockAsync(CancellationToken.None);

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
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            // Act
            var exception = await Record.ExceptionAsync(
                async () => await vertex1.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on single node of graph could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnSingleNodeOfGraphAsync()
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
            var exception = await Record.ExceptionAsync(
                async () => await vertex1.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on last node of graph could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnLastNodeOfGraphAsync()
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
            var exception = await Record.ExceptionAsync(
                async () => await vertex4.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on last node of graph could be taken.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnLastNodeOfGraphAsync()
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
            var exception = await Record.ExceptionAsync(
                async () => await vertex4.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "Write lock on last node of graph could not be taken if first has write lock.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeWriteLockOnLastNodeOfGraphAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var firstLock = await vertex1.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var lastLockTask = vertex4.GetWriteLockAsync(CancellationToken.None);

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
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var lastLock = await vertex4.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var firstLockTask = vertex1.GetWriteLockAsync(CancellationToken.None);

            // Assert
            firstLockTask.IsCompleted.Should().BeFalse();

            lastLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            firstLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Read lock on last node of graph could be taken if fist has read lock.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnLastNodeOfGraphIfFirstReadLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            _ = await vertex1.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var lastLockTask = vertex4.GetReadLockAsync(CancellationToken.None);

            // Assert
            lastLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Read lock on last node of graph could be taken if fisrt has read lock.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnFirstNodeOfGraphIfLastReadLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            _ = await vertex4.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var firstLockTask = vertex1.GetReadLockAsync(CancellationToken.None);

            // Assert
            firstLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Read lock on last node of graph could be taken if fisrt has write lock.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeReadLockOnLastNodeOfGraphIfFirstWriteLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            _ = await vertex1.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var lastLockTask = vertex4.GetReadLockAsync(CancellationToken.None);

            // Assert
            lastLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Read lock on first node of graph could not be taken if last has write lock.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeReadLockOnFirstNodeOfGraphIfLastWriteLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var lastLock = await vertex4.GetWriteLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var firstLockTask = vertex1.GetReadLockAsync(CancellationToken.None);

            // Assert
            firstLockTask.IsCompleted.Should().BeFalse();

            lastLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            firstLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Write lock on last node of graph could not be taken if first has read lock.")]
        [Trait("Category", "Unit")]
        public async Task CantTakeWriteLockOnLastNodeOfGraphIfFirstReadLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var firstLock = await vertex1.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var lastLockTask = vertex4.GetWriteLockAsync(CancellationToken.None);

            // Assert
            lastLockTask.IsCompleted.Should().BeFalse();

            firstLock.Dispose();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            lastLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Write lock on first node of graph could be taken if last has read lock.")]
        [Trait("Category", "Unit")]
        public async Task CanTakeWriteLockOnFirstNodeOfGraphIfLastReadLockAsync()
        {
            // Arrange
            var vertex1 = new RWAsyncDAGVertex();
            var vertex2 = new RWAsyncDAGVertex();
            var vertex3 = new RWAsyncDAGVertex();
            var vertex4 = new RWAsyncDAGVertex();
            vertex1.AddEdgesTo(vertex2, vertex3);
            vertex2.AddEdgesTo(vertex4);
            vertex3.AddEdgesTo(vertex4);

            var _ = await vertex4.GetReadLockAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            var firstLockTask = vertex1.GetWriteLockAsync(CancellationToken.None);

            // Assert
            firstLockTask.IsCompleted.Should().BeTrue();
        }

        [Fact(DisplayName = "Locks is cancelled if token is cancelled.")]
        [Trait("Category", "Unit")]
        public async Task LocksIfCanceledIfTokenIsCancelled()
        {

            var cts = new CancellationTokenSource();

            // Arrange
            var vertex = new RWAsyncDAGVertex();

            _ = await vertex.GetWriteLockAsync(cts.Token).ConfigureAwait(false);

            // Act
            var secondLockTask = vertex.GetWriteLockAsync(cts.Token);

            // Assert
            secondLockTask.IsCompleted.Should().BeFalse();

            cts.Cancel();

            await Task.Delay(500).ConfigureAwait(false); // Attempts to ensure that task is complete.

            secondLockTask.IsCanceled.Should().BeTrue();
        }

    }
}
