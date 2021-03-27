using AsyncHelpers.Helpers;
using AsyncHelpers.Synchronization;
using AsyncHelpers.TaskProducers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncHelpers.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //RechargeableCompletionSourceExample();
            //await ValueTaskCompletionSourceExampleAsync();
            //await SinglePhaseAsyncBarrierExampleAsync();
            //await ContinuationQueueExampleAsync();
            //await WaitAllTasksButCheckAsyncExampleAsync();
            await Task.Yield(); 
        }

        static void RechargeableCompletionSourceExample()
        {
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
        }

        static async Task ValueTaskCompletionSourceExampleAsync()
        {
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
        }

        public static async Task SinglePhaseAsyncBarrierExampleAsync()
        {
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
        }
        public static async Task ContinuationQueueExampleAsync()
        {
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
            });
        }
    }
}
