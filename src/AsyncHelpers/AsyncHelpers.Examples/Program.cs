using AsyncHelpers.TaskProducers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncHelpers.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //RechargeableCompletionSourceExample();
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
    }
}
