using FluentAssertions;

using Xunit;

using AsyncHelpers.Synchronization;

namespace AsyncHelpers.Tests;

public class RWAsyncDAGVertexTests
{
    [Fact(DisplayName = "The graph node could be constructed.")]
    [Trait("Category", "Unit")]
    public void CantBeConstructed()
    {
        // Act
        var exception = Record.Exception(
            () => new AsyncLockDAGVertex());

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "The signle node graph could be validated.")]
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

    [Fact(DisplayName = "Lock on single node could be taken.")]
    [Trait("Category", "Unit")]
    public async Task CanTakeLockOnSingleNodeAsync()
    {
        // Arrange
        var vertex = new AsyncLockDAGVertex();

        // Act
        var exception = await Record.ExceptionAsync(
            async () => await vertex.GetLockAsync(CancellationToken.None));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "Second lock on single node could not be taken.")]
    [Trait("Category", "Unit")]
    public async Task CantTakeSecondLockOnSingleNodeAsync()
    {
        // Arrange
        var vertex = new AsyncLockDAGVertex();
        var firstLock = await vertex.GetLockAsync(CancellationToken.None);

        // Act
        var secondLockTask = vertex.GetLockAsync(CancellationToken.None);

        // Assert
        secondLockTask.IsCompleted.Should().BeFalse();

        firstLock.Dispose();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        secondLockTask.IsCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Lock on single node of graph could be taken.")]
    [Trait("Category", "Unit")]
    public async Task CanTakeLockOnSingleNodeOfGraphAsync()
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
            async () => await vertex1.GetLockAsync(CancellationToken.None));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "Lock on last node of graph could be taken.")]
    [Trait("Category", "Unit")]
    public async Task CanTakeLockOnLastNodeOfGraphAsync()
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
            async () => await vertex4.GetLockAsync(CancellationToken.None));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "Lock on last node of graph could not be taken if first has write lock.")]
    [Trait("Category", "Unit")]
    public async Task CantTakeLockOnLastNodeOfGraphAsync()
    {
        // Arrange
        var vertex1 = new AsyncLockDAGVertex();
        var vertex2 = new AsyncLockDAGVertex();
        var vertex3 = new AsyncLockDAGVertex();
        var vertex4 = new AsyncLockDAGVertex();
        vertex1.AddEdgesTo(vertex2, vertex3);
        vertex2.AddEdgesTo(vertex4);
        vertex3.AddEdgesTo(vertex4);

        var firstLock = await vertex1.GetLockAsync(CancellationToken.None);

        // Act
        var lastLockTask = vertex4.GetLockAsync(CancellationToken.None);

        // Assert
        lastLockTask.IsCompleted.Should().BeFalse();

        firstLock.Dispose();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        lastLockTask.IsCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Lock on first node of graph could not be taken if last has write lock.")]
    [Trait("Category", "Unit")]
    public async Task CantTakeLockOnFirstNodeOfGraphAsync()
    {
        // Arrange
        var vertex1 = new AsyncLockDAGVertex();
        var vertex2 = new AsyncLockDAGVertex();
        var vertex3 = new AsyncLockDAGVertex();
        var vertex4 = new AsyncLockDAGVertex();
        vertex1.AddEdgesTo(vertex2, vertex3);
        vertex2.AddEdgesTo(vertex4);
        vertex3.AddEdgesTo(vertex4);

        var lastLock = await vertex4.GetLockAsync(CancellationToken.None);

        // Act
        var firstLockTask = vertex1.GetLockAsync(CancellationToken.None);

        // Assert
        firstLockTask.IsCompleted.Should().BeFalse();

        lastLock.Dispose();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        firstLockTask.IsCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Locks is cancelled if token is cancelled.")]
    [Trait("Category", "Unit")]
    public async Task LocksIfCanceledIfTokenIsCancelled()
    {

        var cts = new CancellationTokenSource();

        // Arrange
        var vertex = new AsyncLockDAGVertex();

        _ = await vertex.GetLockAsync(cts.Token);

        // Act
        var secondLockTask = vertex.GetLockAsync(cts.Token);

        // Assert
        secondLockTask.IsCompleted.Should().BeFalse();

        cts.Cancel();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        secondLockTask.IsCanceled.Should().BeTrue();
    }

    [Fact(DisplayName = "Child nodes of one parent does not blocks each other.")]
    [Trait("Category", "Unit")]
    public async Task ChildsLocksDoesnotBlocksEachOther()
    {
        // Arrange
        var vertex1 = new AsyncLockDAGVertex();
        var vertex2 = new AsyncLockDAGVertex();
        var vertex3 = new AsyncLockDAGVertex();
        var vertex4 = new AsyncLockDAGVertex();
        vertex1.AddEdgesTo(vertex2, vertex3);
        vertex2.AddEdgesTo(vertex4);
        vertex3.AddEdgesTo(vertex4);

        _ = await vertex2.GetLockAsync(CancellationToken.None);

        // Act
        var lock3Task = vertex3.GetLockAsync(CancellationToken.None);

        // Assert
        lock3Task.IsCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Child with few parents wait that blocks from both parents.")]
    [Trait("Category", "Unit")]
    public async Task ChildWaitAllParentsLocks()
    {
        // Arrange
        var vertex1 = new AsyncLockDAGVertex();
        var vertex2 = new AsyncLockDAGVertex();
        var vertex3 = new AsyncLockDAGVertex();
        var vertex4 = new AsyncLockDAGVertex();
        vertex1.AddEdgesTo(vertex2, vertex3);
        vertex2.AddEdgesTo(vertex4);
        vertex3.AddEdgesTo(vertex4);

        var lock2 = await vertex2.GetLockAsync(CancellationToken.None);
        var lock3 = await vertex3.GetLockAsync(CancellationToken.None);

        // Act
        var lock4Task = vertex4.GetLockAsync(CancellationToken.None);

        // Assert
        lock4Task.IsCompleted.Should().BeFalse();

        lock2.Dispose();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        lock4Task.IsCompleted.Should().BeFalse();

        lock3.Dispose();

        await Task.Delay(500); // Attempts to ensure that task is complete.

        lock4Task.IsCompleted.Should().BeTrue();

    }
}
