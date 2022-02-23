using AsyncHelpers.Helpers;
using AsyncHelpers.Synchronization;
using AsyncHelpers.TaskProducers;

namespace AsyncHelpers.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //await RechargeableCompletionSourceExampleAsync().ConfigureAwait(false);
            //await ValueTaskCompletionSourceExampleAsync().ConfigureAwait(false);
            //await SinglePhaseAsyncBarrierExampleAsync().ConfigureAwait(false);
            //await ContinuationQueueExampleAsync().ConfigureAwait(false);
            //await WaitAllTasksButCheckAsyncExampleAsync().ConfigureAwait(false);
            //await TryExecuteWithTimeoutAsyncExampleAsync().ConfigureAwait(false);
            //await RWAsyncDAGVertexExampleAsync().ConfigureAwait(false);4
            //await RunLongTaskWithCancellation().ConfigureAwait(false);
            await WhenAllOrErrorTest().ConfigureAwait(false);
        }

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

            Thread.Sleep(1000);

            var t2 = Task.Run(() =>
            {
                int i = 0;
                while (true)
                {
                    Thread.Sleep(1_000);
                    Console.WriteLine($"Add {i} on Thread {Environment.CurrentManagedThreadId}");
                    rtcs.SetResultAndWait(i++);
                }
            });

            await Task.WhenAll(t1, t2).ConfigureAwait(false);
        }

        static async Task ValueTaskCompletionSourceExampleAsync()
        {
            var vtcs = new ValueTaskCompletionSource<int>(false);
            var are = new AutoResetEvent(false);

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
                    var result = await t.ConfigureAwait(false);
                    Console.WriteLine($"{result}");
                    are.Set();
                }
            });

            await Task.WhenAll(t1, t2).ConfigureAwait(false);
        }

        public static async Task SinglePhaseAsyncBarrierExampleAsync()
        {
            var spb = new SinglePhaseAsyncBarrier(3);

            var t1 = Task.Run(async () =>
            {
                Thread.Sleep(1_000);
                Console.WriteLine("Task A Added");
                await spb.SignalAndWaitAsync().ConfigureAwait(false);
                Console.WriteLine("A Done");
            });

            var t2 = Task.Run(async () =>
            {
                Thread.Sleep(2_000);
                Console.WriteLine("Task B Added");
                await spb.SignalAndWaitAsync().ConfigureAwait(false);
                Console.WriteLine("B Done");
            });

            var t3 = Task.Run(async () =>
            {
                Thread.Sleep(3_000);
                Console.WriteLine("Task C Added");
                await spb.SignalAndWaitAsync().ConfigureAwait(false);
                Console.WriteLine("C Done");
            });

            await Task.WhenAll(t1, t2, t3).ConfigureAwait(false);
        }
        public static async Task ContinuationQueueExampleAsync()
        {
            var cq = new ContinuationQueue();

            var t1 = Task.Run(async () =>
            {
                Thread.Sleep(100);
                Console.WriteLine("Task A Added");
                await cq.WaitAsync().ConfigureAwait(false);
                Console.WriteLine("A Done");
            });

            var t2 = Task.Run(async () =>
            {
                Thread.Sleep(200);
                Console.WriteLine("Task B Added");
                await cq.WaitAsync().ConfigureAwait(false);
                Console.WriteLine("B Done");
            });

            Thread.Sleep(1_000);
            Console.WriteLine("FinishTask");
            cq.FinishTask();
            Thread.Sleep(1_000);
            Console.WriteLine("FinishTask");
            cq.FinishTask();

            await Task.WhenAll(t1, t2).ConfigureAwait(false);
        }

        public static async Task WaitAllTasksButCheckAsyncExampleAsync()
        {
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
            }).ConfigureAwait(false);
        }

        public static async Task TryExecuteWithTimeoutAsyncExampleAsync()
        {
            var task = Task.Delay(5000);
            var timeout = 1000;
            var isExecutedInTimeout = await task.TryExecuteWithTimeoutAsync(timeout, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine(isExecutedInTimeout);
        }

        public static async Task RunLongTaskWithCancellation()
        {
            var tcs = new TaskCompletionSource<int>();
            var timeout = 1000;
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                await tcs.Task.WithCancellation(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task was canceled");
            }
        }

        public static async Task WhenAllOrErrorTest()
        {
            var tcs1 = new TaskCompletionSource<int>();
            var tcs2 = new TaskCompletionSource<int>();
            var tcs3 = new TaskCompletionSource<int>();

            _ = Task.Delay(1000).ContinueWith(_ => tcs3.SetException(new InvalidOperationException()));

            try
            {
                await Extensions.WhenAllOrError(tcs1.Task, tcs2.Task, tcs3.Task).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Error on await");
            }
        }

        public class VertexWithValue : AsyncLockDAGVertex
        {
            public int Value { get; set; }
            public int Id { get; }
            public VertexWithValue(int id)
            {
                Id = id;
            }
            public override string ToString() => $"V{Id} => {Value}";
        }

        public static async Task RWAsyncDAGVertexExampleAsync()
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
                    await Task.Delay(500); // Run interval.
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
                    await Task.Delay(400); // Run interval.
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
                    await Task.Delay(400); // Run interval.
                    using var _ = await vertex3.GetLockAsync(CancellationToken.None).ConfigureAwait(false);
                    Console.WriteLine($"    Start updating [{vertex3}]");
                    vertex3.Value++;
                    await Task.Delay(1000).ConfigureAwait(false);  // Some async work.
                    Console.WriteLine($"    End updating [{vertex3}]");
                }
            });

            await Task.WhenAll(vertex1Task, vertex2Task, vertex3Task).ConfigureAwait(false);
        }
    }
}
