using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            bool active = true;
            var runner = new Runner();

            while (active)
            {

                Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                Console.WriteLine("(1) Process Sync");
                Console.WriteLine("(2) Process Async (basic)");
                Console.WriteLine("(3) Process Async (Task)");
                Console.WriteLine("(4) Process Async (aync/await)");
                Console.WriteLine("(5) Process Async (aync/await with error)");
                Console.WriteLine("(6) Process Async (aync/await) in parallel");
                Console.WriteLine();
                Console.WriteLine("Press any other key to exit...");

                var input = Console.ReadKey();

                Console.WriteLine();

                switch (input.KeyChar)
                {
                    case '1':
                        runner.ProcessSync();
                        break;

                    case '2':
                        runner.ProcessAsyncBasic();
                        break;

                    case '3':
                        runner.ProcessAsyncTask();
                        break;

                    case '4':
                        runner.ProcessAsyncAwait(false);
                        break;

                    case '5':
                        runner.ProcessAsyncAwait(true);
                        break;

                    case '6':
                        runner.ProcessAsyncAwaitParallel();
                        break;

                    default:
                        active = false;
                        break;
                }
            }
        }
    }
    
    public class Runner : ILogger
    {

        public void ProcessSync()
        {
            new SyncProcessor(this).Process();
        }

        public void ProcessAsyncBasic()
        {
            new AsyncProcessorBasic(this).Process();
        }

        public void ProcessAsyncTask()
        {
            new AsyncProcessorTask(this).Process();
        }

        public void ProcessAsyncAwait(bool forceError)
        {
            new AsyncProcessorAwait(this).Process(forceError);
        }

        public void ProcessAsyncAwaitParallel()
        {
            new AsyncProcessorParallel(this).Process();
        }

        public void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");
        }
    }

    public class SyncProcessor
    {
        private ILogger _logger;

        public SyncProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public void Process()
        {
            _logger.LogMessage("SyncProcessor.Process() started");

            var client = new WebClient();

            var result = client.DownloadString("http://www.google.co.uk");
            Thread.Sleep(3000);

            _logger.LogMessage("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            _logger.LogMessage(result.Substring(0, 50));
            _logger.LogMessage(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

            _logger.LogMessage("SyncProcessor.Process() completed");
        }
    }

    public class AsyncProcessorBasic
    {
        private ILogger _logger;

        public AsyncProcessorBasic(ILogger logger)
        {
            _logger = logger;
        }

        public void Process()
        {
            _logger.LogMessage("AsyncProcessorV1.Process() started");

            var client = new WebClient();

            client.DownloadStringAsync(new Uri("http://www.google.co.uk"));
            client.DownloadStringCompleted += Client_DownloadStringCompleted;

            _logger.LogMessage("AsyncProcessorV1.Process() completed");
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _logger.LogMessage("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            _logger.LogMessage(e.Result.Substring(0, 50));
            _logger.LogMessage(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }
    }

    public class AsyncProcessorTask
    {
        private ILogger _logger;

        public AsyncProcessorTask(ILogger logger)
        {
            _logger = logger;
        }

        public void Process()
        {
            // setup long running task
            var task = Task.Run(() =>
            {

                // note: Task.Run() should only be used to run asyncronous methods!

                // uncomment to simulate error handling
                //throw new Exception("who knew?");

                _logger.LogMessage("AsyncProcessorTask.Process() started");
                Thread.Sleep(2000);
                _logger.LogMessage("AsyncProcessorTask.Process() completed");

                return "all done";
            });

            /*
            // setup continuation task, executed after long running task completes
            task.ContinueWith((t) =>
            {

                if (t.IsFaulted)
                {
                    _logger.LogMessage($"Exception caught: { t.Exception.Message }");
                    return;
                }

                _logger.LogMessage($"Continuation received result: {t.Result}");
            });
            */

            task
                .ConfigureAwait(true)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    _logger.LogMessage($"Continuation received result: {task.Result}");
                });
        }
    }

    public class AsyncProcessorAwait
    {
        private ILogger _logger;

        public AsyncProcessorAwait(ILogger logger)
        {
            _logger = logger;
        }

        public async void Process(bool forceError)
        {
            try
            {

                _logger.LogMessage("Starting long running task...");

                var result = await LongRunningTaskAsync(forceError);

                _logger.LogMessage($"Long running task completed, result = { result }");
            }
            catch(Exception ex)
            {
                // note: with error handling covered by inner method, this shouldn't happen...
                _logger.LogMessage($"Unhandled error occurred in async operation: { ex.Message }");
            }
        }

        private async Task<string> LongRunningTaskAsync(bool forceError)
        {
            try
            {
                if (forceError)
                {
                    _logger.LogMessage("Forcing exception...");
                    throw new Exception("Forced error");
                }

                var result = await Task.Run(() => {

                    Thread.Sleep(2000);

                    return "success!";
                });

                return result;
            }
            catch(Exception ex)
            {
                return $"failed: { ex.Message }";
            }
        }
    }

    public class AsyncProcessorParallel
    {
        private ILogger _logger;

        public AsyncProcessorParallel(ILogger logger)
        {
            _logger = logger;
        }

        public async void Process()
        {
            try
            {
                var task100 = LongRunningTask(100, 3);
                var task200 = LongRunningTask(200, 4);
                var task300 = LongRunningTask(300, 5);

                await Task.WhenAll(task100, task200, task300);

                _logger.LogMessage("All tasks complete");
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"Unhandled error occurred in async operation: { ex.Message }");
            }
        }

        private async Task LongRunningTask(int id, int iterations)
        {
            try
            {

                _logger.LogMessage($"Task { id } started");

                for (int i = 1; i <= iterations; i++)
                {
                    await Task.Delay(1000);
                    _logger.LogMessage($"Task { id } completed iteration { i }");
                }

                _logger.LogMessage($"Task { id } completed");
            }
            catch (Exception ex)
            {
                // TODO: handle accordingly
            }
        }
    }
}
