# AsyncHelpers
Helper library for async operations.

### RechargeableCompletionSource

```C#
var runContinuationsAsynchronously = false;
var rtcs = new RechargeableCompletionSource<int>(runContinuationsAsynchronously);
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

while (true)
{
    Thread.Sleep(2_000);
    using var data = await rtcs.GetValueAsync();
    Console.WriteLine($"Get {data.Value} on Thread {Thread.CurrentThread.ManagedThreadId}");
}
```

Output
```
Add 0 on Thread 4
Get 0 on Thread 1
Add 1 on Thread 4
Get 1 on Thread 1
Add 2 on Thread 4
Get 2 on Thread 1
Add 3 on Thread 4
Get 3 on Thread 1
Add 4 on Thread 4
```
