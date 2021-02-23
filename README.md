# AsyncHelpers
Helper library for async operations.

Contains:
* RechargeableCompletionSource
* ValueTaskCompletionSource
* SinglePhaseAsyncBarrier
* ContinuationQueue
* TaskExtensions

### RechargeableCompletionSource
TaskCompletionSource analogue with some kind of reset operation that could be used multiple times in producer–consumer scenario.
```C#
var runContinuationsAsynchronously = false;
var rtcs = new RechargeableCompletionSource<int>(runContinuationsAsynchronously);

_ = Task.Run(async () =>
{
    while (true)
    {
        using var data = await rtcs.GetValueAsync();
        Console.WriteLine($"Get {data.Value} on Thread {Thread.CurrentThread.ManagedThreadId}");
    }
});

Thread.Sleep(1000);

_ = Task.Run(() =>
{
    int i = 0;
    while (true)
    {
        Thread.Sleep(1_000);
        Console.WriteLine($"Add {i} on Thread {Thread.CurrentThread.ManagedThreadId}");
        rtcs.SetResultAndWait(i++);
    }
});

Console.ReadKey();
```

Output
```
Add 0 on Thread 4
Get 0 on Thread 4
Add 1 on Thread 4
Get 1 on Thread 4
Add 2 on Thread 4
Get 2 on Thread 4
Add 3 on Thread 4
Get 3 on Thread 4
```
for 
```C#
var runContinuationsAsynchronously = true; 
```
we will have Thread switching in Get part

```
Add 0 on Thread 4
Get 0 on Thread 5
Add 1 on Thread 4
Get 1 on Thread 5
Add 2 on Thread 4
Get 2 on Thread 5
Add 3 on Thread 4
Get 3 on Thread 6
```

### ValueTaskCompletionSource

IValueTaskSource based analogue of TaskCompletionSource without allocation on async way.

```C#
ValueTaskCompletionSource<int> vtcs = new ValueTaskCompletionSource<int>(false);
AutoResetEvent are = new AutoResetEvent(false);

var t1 = Task.Run(() =>
{
    int v = 0;
    while (true)
    {
        Thread.Sleep(1_000); // attempts to execute await t before set result
        vtcs.SetResult(v++);
        are.WaitOne();
    }
});

var t2 = Task.Run(async () =>
{
    while (true)
    {
        ValueTask<int> t = vtcs.Task;
        var result = await t;
        Console.WriteLine($"{result}");
        are.Set();
    }
});

await Task.WhenAll(t1, t2);
```

### SinglePhaseAsyncBarrier

Async version of Barrier with single phase of work.

```C#
SinglePhaseAsyncBarrier spb = new SinglePhaseAsyncBarrier(3);

var t1 = Task.Run(async () =>
{
   Thread.Sleep(1_000);
   Console.WriteLine("Task A Added");
   await spb.SignalAndWaitAsync();
   Console.WriteLine("A Done");
});

var t2 = Task.Run(async () =>
{
    Thread.Sleep(2_000);
    Console.WriteLine("Task B Added");
    await spb.SignalAndWaitAsync();
    Console.WriteLine("B Done");
});

var t3 = Task.Run(async () =>
{
    Thread.Sleep(3_000);
    Console.WriteLine("Task C Added");
    await spb.SignalAndWaitAsync();
    Console.WriteLine("C Done");
});

await Task.WhenAll(t1, t2, t3);
```

Output

```
Task A Added
Task B Added
Task C Added
C Done
A Done
B Done
```

### ContinuationQueue
Queue that registers continuations and runs them one be one.

```C#
ContinuationQueue cq = new ContinuationQueue();

var t1 = Task.Run(async () =>
{
   Thread.Sleep(100);
   Console.WriteLine("Task A Added");
   await cq.WaitAsync();
   Console.WriteLine("A Done");
});

var t2 = Task.Run(async () =>
{
    Thread.Sleep(200);
    Console.WriteLine("Task B Added");
    await cq.WaitAsync();
    Console.WriteLine("B Done");
});

Thread.Sleep(1_000);
Console.WriteLine("FinishTask");
cq.FinishTask();
Thread.Sleep(1_000);
Console.WriteLine("FinishTask");
cq.FinishTask();

await Task.WhenAll(t1, t2);

```

Output

```
Task A Added
Task B Added
FinishTask
A Done
FinishTask
B Done
```

### TaskExtensions

#### WaitAllTasksButCheckAsync
Waits all tasks but invoke OnFaulted if any task is failed.

```C#
var t1 = Task.Run(() =>
{
    Thread.Sleep(1_000);
    throw new InvalidOperationException();
});

var t2 = Task.Run(() =>
{
    Thread.Sleep(10_000);
    Console.WriteLine("T2 Done");
});

await new[] { t1, t2 }.WaitAllTasksButCheckAsync(() =>
{
    Console.WriteLine("Rise error without waiting 10 seconds for second task.");
});
```

#### TryExecuteWithTimeoutAsync
Attempts to execute async operation within the expected time.

```C#
Task task = ...;
var timeout = 1000;
var isExecutedInTimeout = await task.TryExecuteWithTimeoutAsync(timeout, CancellationToken.None);
```
