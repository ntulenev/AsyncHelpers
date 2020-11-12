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
ValueTaskCompletionSource<int> i = new ValueTaskCompletionSource<int>(false);
AutoResetEvent are = new AutoResetEvent(false);

var t1 = Task.Run(() =>
{
    int v = 0;
    while (true)
    {
        i.SetResult(v++);
        are.WaitOne();
    }
});

var t2 = Task.Run(async () =>
{
    while (true)
    {
        ValueTask<int> t = i.Task;
        var result = await t;
        Console.WriteLine($"{result}");
        are.Set();
    }
});
await Task.WhenAll(t1, t2);
```

###Benchmark

```C#
[MemoryDiagnoser]
public class Test
{

    public const int ITERATIONS = 1_000_000;

    [Benchmark]
    public async Task ValueTaskTest()
    {
        int indexSet = ITERATIONS;
        int indexRead = ITERATIONS;
        var i = new ValueTaskCompletionSource<int>(false);
        AutoResetEvent are = new AutoResetEvent(false);

        var t1 = Task.Run(() =>
        {
            int v = 0;
            while (indexSet-- >= 0)
            {
                i.SetResult(v++);
                are.WaitOne();
            }
        });

        var t2 = Task.Run(async () =>
        {
            while (indexRead-- >= 0)
            {
                ValueTask<int> t = i.Task;
                var result = await t;
                are.Set();
            }
        });
        await Task.WhenAll(t1, t2);
    }

    [Benchmark]
    public  async Task TaskTest()
    {
        int indexSet = ITERATIONS;
        int indexRead = ITERATIONS;
        var i = new TaskCompletionSource<int>();
        AutoResetEvent are = new AutoResetEvent(false);

        var t1 = Task.Run(() =>
        {
            int v = 0;
            while (indexSet-- >= 0)
            {
                i.SetResult(v++);
                are.WaitOne();
            }
        });

        var t2 = Task.Run(async () =>
        {
            while (indexRead-- >= 0)
            {
                Task<int> t = i.Task;
                var result = await t;
                i = new TaskCompletionSource<int>(); //reset TCS
                are.Set();
            }
        });
        await Task.WhenAll(t1, t2);
    }

}

```

|        Method |     Mean |    Error |   StdDev |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|-------------- |---------:|---------:|---------:|-----------:|------:|------:|-----------:|
| ValueTaskTest | 588.5 ms | 11.59 ms | 25.93 ms |          - |     - |     - |     1016 B |
|      TaskTest | 606.3 ms | 11.76 ms | 25.06 ms | 22000.0000 |     - |     - | 96001080 B |
