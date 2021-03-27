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
    }
}
