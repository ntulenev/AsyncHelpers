# AsyncHelpers
Helper library for async operations.

### RechargeableCompletionSource

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
ValueTaskCompletionSource<int> vtvs = new ValueTaskCompletionSource<int>(false);
AutoResetEvent are = new AutoResetEvent(false);

var t1 = Task.Run(() =>
{
    int v = 0;
    while (true)
    {
        Thread.Sleep(1_000); // attempts to execute await t before set result
        vtvs.SetResult(v++);
        are.WaitOne();
    }
});

var t2 = Task.Run(async () =>
{
    while (true)
    {
        ValueTask<int> t = vtvs.Task;
        var result = await t;
        Console.WriteLine($"{result}");
        are.Set();
    }
});

await Task.WhenAll(t1, t2);
```
