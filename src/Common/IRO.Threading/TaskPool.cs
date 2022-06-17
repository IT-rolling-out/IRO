using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using NeoSmart.AsyncLock;

namespace IRO.Threading
{
    public class TaskPool
    {
        readonly SmartThreadPool _smartThreadPool;

        public int MaxThreadsCount { get; }

        public TaskPool(int? maxThreadsCount = null)
        {
            if (maxThreadsCount.HasValue)
            {
                if (maxThreadsCount < 1)
                {
                    throw new ArgumentException($"Value is '{maxThreadsCount}'. Must be bigger than 1.", nameof(maxThreadsCount));
                }
                MaxThreadsCount = maxThreadsCount.Value;
            }
            else
            {
                MaxThreadsCount = (Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1);
                //MaxThreadsCount = Environment.ProcessorCount;
            }

            _smartThreadPool = new SmartThreadPool(new STPStartInfo()
            {

            });
            _smartThreadPool.MaxThreads = MaxThreadsCount;

        }

        public Task Run(Func<Task> func, CancellationToken cancellationToken = default)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            Func<Task<bool>> funcWithRes = async () =>
            {
                await func();
                return false;
            };
            return Run(funcWithRes, cancellationToken);
        }

        public Task<T> Run<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            WaitWhileActiveThreadRun();
            //Console.WriteLine($"Active: {_smartThreadPool.ActiveThreads} | InUse {_smartThreadPool.InUseThreads}");
            (var wrapActionToStart, var runTask) = WrapToTaskCompletionSource(func, cancellationToken);

            //Must synchronously check and synchronously write to HashSet.
            _smartThreadPool.QueueWorkItem(wrapActionToStart);
            return runTask;
        }

        void WaitWhileActiveThreadRun()
        {
            if (_smartThreadPool.ActiveThreads >= MaxThreadsCount)
            {
                try
                {
                    _smartThreadPool.WaitForIdle();
                }
                catch { }
            }
        }

        (Func<Task> WrapActionToStart, Task<T> RunTask) WrapToTaskCompletionSource<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>(TaskContinuationOptions.ExecuteSynchronously);
            var newTask = tcs.Task;

            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });

            Func<Task> actionToStart = async () =>
            {
                try
                {
                    //Console.WriteLine(
                    //            $"---{{\n" +
                    //            $"Thread id: {Thread.CurrentThread.ManagedThreadId}\n" +
                    //            $"Task id: {Task.CurrentId}\n" +
                    //            $"Delegate name: '{func.Method.Name}'\n" +
                    //            $"Nesting: {CurrentThreadNestingLevel}\n"
                    //            $"}}---"
                    //    );

                    if (cancellationToken.IsCancellationRequested)
                    {                       
                        tcs.TrySetCanceled();
                        return;
                    }
                    var res = await func();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    tcs.TrySetResult(res);

                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            return (actionToStart, newTask);
        }

    }
}
