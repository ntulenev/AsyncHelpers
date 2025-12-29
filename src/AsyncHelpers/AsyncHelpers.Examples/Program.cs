using AsyncHelpers.Helpers;
using AsyncHelpers.Synchronization;
using AsyncHelpers.TaskProducers;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
Console.WriteLine("Hello World!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

await RechargeableCompletionSourceExampleAsync().ConfigureAwait(false);
await ValueTaskCompletionSourceExampleAsync().ConfigureAwait(false);
await SinglePhaseAsyncBarrierExampleAsync().ConfigureAwait(false);
await ContinuationQueueExampleAsync().ConfigureAwait(false);
await WaitAllTasksButCheckAsyncExampleAsync().ConfigureAwait(false);
await TryExecuteWithTimeoutAsyncExampleAsync().ConfigureAwait(false);
await RWAsyncDAGVertexExampleAsync().ConfigureAwait(false);
await RunLongTaskWithCancellation().ConfigureAwait(false);
await WhenAllOrErrorTest().ConfigureAwait(false);


static async Task RechargeableCompletionSourceExampleAsync()
{
    var runContinuationsAsynchronously = false;
    var rtcs = new RechargeableCompletionSource<int>(runContinuationsAsynchronously);

    var t1 = Task.Run(async () =>
    {
        while (true)
        {
            using var data = await rtcs.GetValueAsync().ConfigureAwait(false);
            Console.WriteLine($"Get {data.Value} on Thread {Environment.CurrentManagedThreadId}");
        }
    });

#pragma warning disable CA1849 // Call async methods when in an async method
    Thread.Sleep(1000);
#pragma warning restore CA1849 // Call async methods when in an async method

    var t2 = Task.Run(() =>
    {
        var i = 0;
        while (true)
        {
#pragma warning disable CA1849 // Call async methods when in an async method
            Thread.Sleep(1_000);
#pragma warning restore CA1849 // Call async methods when in an async method
            Console.WriteLine($"Add {i} on Thread {Environment.CurrentManagedThreadId}");
            rtcs.SetResultAndWait(i++);
        }
    });

    await Task.WhenAll(t1, t2).ConfigureAwait(false);
}

static async Task ValueTaskCompletionSourceExampleAsync()
{
    var vtcs = new ValueTaskCompletionSource<int>(false);
    using var are = new AutoResetEvent(false);

    var t1 = Task.Run(() =>
    {
        var v = 0;
        while (true)
        {
#pragma warning disable CA1849 // Call async methods when in an async method
            Thread.Sleep(1_000); // attempts to execute await t before set result
#pragma warning restore CA1849 // Call async methods when in an async method
            vtcs.SetResult(v++);
            _ = are.WaitOne();
        }
    });

    var t2 = Task.Run(async () =>
    {
        while (true)
        {
            var t = vtcs.Task;
            var result = await t.ConfigureAwait(false);
            Console.WriteLine($"{result}");
            _ = are.Set();
        }
    });

    await Task.WhenAll(t1, t2).ConfigureAwait(false);
}

static async Task SinglePhaseAsyncBarrierExampleAsync()
{
    var spb = new SinglePhaseAsyncBarrier(3);

    var t1 = Task.Run(async () =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(1_000);
#pragma warning restore CA1849 // Call async methods when in an async method
        Console.WriteLine("Task A Added");
        await spb.SignalAndWaitAsync().ConfigureAwait(false);
        Console.WriteLine("A Done");
    });

    var t2 = Task.Run(async () =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(2_000);
#pragma warning restore CA1849 // Call async methods when in an async method
        Console.WriteLine("Task B Added");
        await spb.SignalAndWaitAsync().ConfigureAwait(false);
        Console.WriteLine("B Done");
    });

    var t3 = Task.Run(async () =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(3_000);
#pragma warning restore CA1849 // Call async methods when in an async method
        Console.WriteLine("Task C Added");
        await spb.SignalAndWaitAsync().ConfigureAwait(false);
        Console.WriteLine("C Done");
    });

    await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
}
static async Task ContinuationQueueExampleAsync()
{
    var cq = new ContinuationQueue();

    var t1 = Task.Run(async () =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(100);
#pragma warning restore CA1849 // Call async methods when in an async method
        Console.WriteLine("Task A Added");
        await cq.WaitAsync().ConfigureAwait(false);
        Console.WriteLine("A Done");
    });

    var t2 = Task.Run(async () =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(200);
#pragma warning restore CA1849 // Call async methods when in an async method
        Console.WriteLine("Task B Added");
        await cq.WaitAsync().ConfigureAwait(false);
        Console.WriteLine("B Done");
    });

#pragma warning disable CA1849 // Call async methods when in an async method
    Thread.Sleep(1_000);
#pragma warning restore CA1849 // Call async methods when in an async method
    Console.WriteLine("FinishTask");
    cq.FinishTask();
#pragma warning disable CA1849 // Call async methods when in an async method
    Thread.Sleep(1_000);
#pragma warning restore CA1849 // Call async methods when in an async method
    Console.WriteLine("FinishTask");
    cq.FinishTask();

    await Task.WhenAll(t1, t2).ConfigureAwait(false);
}

static async Task WaitAllTasksButCheckAsyncExampleAsync()
{
    var t1 = Task.Run(() =>
    {
#pragma warning disable CA1849 // Call async methods when in an async method
        Thread.Sleep(1_000);
#pragma warning restore CA1849 // Call async methods when in an async method
        throw new InvalidOperationException();
    });

    var t2 = Task.Run(() =>
    {
        Thread.Sleep(10_000);
        Console.WriteLine("T2 Done");
    });

    await new[] { t1, t2 }.WaitAllTasksButCheckAsync(() =>
    Console.WriteLine("Rise error without waiting 10 seconds for second task.")).ConfigureAwait(false);
}

static async Task TryExecuteWithTimeoutAsyncExampleAsync()
{
    var task = Task.Delay(5000);
    var timeout = 1000;
    var isExecutedInTimeout = await task.TryExecuteWithTimeoutAsync(timeout, CancellationToken.None).ConfigureAwait(false);
    Console.WriteLine(isExecutedInTimeout);
}

#pragma warning disable IDE1006 // Naming Styles
static async Task RunLongTaskWithCancellation()
{
    var tcs = new TaskCompletionSource<int>();
    var timeout = 1000;
    using var cts = new CancellationTokenSource(timeout);

    try
    {
        _ = await tcs.Task.WithCancellation(cts.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Task was canceled");
    }
}
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
static async Task WhenAllOrErrorTest()
{
    var tcs1 = new TaskCompletionSource<int>();
    var tcs2 = new TaskCompletionSource<int>();
    var tcs3 = new TaskCompletionSource<int>();

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
    _ = Task.Delay(1000).ContinueWith(_ => tcs3.SetException(new InvalidOperationException()));
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

    try
    {
        _ = await Extensions.WhenAllOrError(tcs1.Task, tcs2.Task, tcs3.Task).ConfigureAwait(false);
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("Error on await");
    }
}
#pragma warning restore IDE1006 // Naming Styles


static async Task RWAsyncDAGVertexExampleAsync()
{

    var vertex1 = new VertexWithValue(1);
    var vertex2 = new VertexWithValue(2);
    var vertex3 = new VertexWithValue(3);
    vertex1.AddEdgesTo(vertex2, vertex3);
    vertex1.ValidateGraph();

    var vertex1Task = Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(500).ConfigureAwait(false); // Run interval.
            using var _ = await vertex1.GetLockAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"Start updating [{vertex1}]");
            vertex1.Value++;
            await Task.Delay(5000).ConfigureAwait(false);  // Some async work.
            Console.WriteLine($"End updating [{vertex1}]");
        }
    });

    var vertex2Task = Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(400).ConfigureAwait(false); // Run interval.
            using var _ = await vertex2.GetLockAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"    Start updating [{vertex2}]");
            vertex2.Value++;
            await Task.Delay(1000).ConfigureAwait(false);  // Some async work.
            Console.WriteLine($"    End updating [{vertex2}]");
        }
    });

    var vertex3Task = Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(400).ConfigureAwait(false); // Run interval.
            using var _ = await vertex3.GetLockAsync(CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"    Start updating [{vertex3}]");
            vertex3.Value++;
            await Task.Delay(1000).ConfigureAwait(false);  // Some async work.
            Console.WriteLine($"    End updating [{vertex3}]");
        }
    });

    await Task.WhenAll(vertex1Task, vertex2Task, vertex3Task).ConfigureAwait(false);
}


#pragma warning disable CA1050 // Declare types in namespaces
/// <summary>
/// Represents a vertex in an asynchronous lock DAG with an associated integer value.
/// </summary>
internal sealed class VertexWithValue(int id) : AsyncLockDAGVertex
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Gets or sets the value associated with this vertex.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets the identifier for this vertex.
    /// </summary>
    public int Id { get; } = id;

    /// <inheritdoc/>
    public override string ToString() => $"V{Id} => {Value}";
}
