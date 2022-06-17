using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoSmart.AsyncLock;

namespace IRO.Threading
{
    public class TaskPool
    {
        readonly object _locker = new object();

        readonly AsyncLock _asyncLock = new AsyncLock();

        readonly HashSet<Task> _tasksHashSet = new HashSet<Task>();


        public int MaxThreadsCount { get; }

        public int RunningTasksCount { get; private set; }

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
        }

        public Task Run(Func<Task> func, CancellationToken cancellationToken = default)
        {
            Func<Task<bool>> funcWithRes = async () =>
             {
                 await func();
                 return false;
             };
            var (startTask, runTask) = Start(funcWithRes, cancellationToken);
            if (startTask != null)
                startTask.Wait();
            return runTask;
        }

        public Task<T> Run<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            var (startTask, runTask) = Start(func, cancellationToken);
            if (startTask != null)
                startTask.Wait();
            return runTask;
        }

        (Task StartTask, Task<T> RunTask) Start<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }


            (var wrapActionToStart, var runTask) = WrapToTaskCompletionSource(func, cancellationToken);
            Task startTask = null;

            //Must synchronously check and synchronously write to HashSet.
            lock (_locker)
            {
                Add(runTask);
                var startedTask = Task.Run(wrapActionToStart);
                if (IsStarving())
                {
                    startTask = startedTask;
                }
                else
                {

                }
            }
            return (startTask, runTask);
        }

        //public async Task WaitWhilePoolStarving()
        //{
        //    Task taskToWait = null;
        //    lock (_locker)
        //    {
        //        if (IsStarving() && !IsCurrentThreadInPool)
        //        {
        //            taskToWait = FirstOrDefault();
        //        }
        //    }
        //    if (taskToWait != null)
        //        await taskToWait;
        //}

        bool IsStarving() => RunningTasksCount >= MaxThreadsCount;

        (Func<Task> WrapActionToStart, Task<T> RunTask) WrapToTaskCompletionSource<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>(TaskContinuationOptions.ExecuteSynchronously);
            var newTask = tcs.Task;

            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                Remove(newTask);
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


                    var res = await func();
                    Remove(newTask);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(res);
                    }
                }
                catch (Exception ex)
                {
                    Remove(newTask);
                    tcs.TrySetException(ex);
                }
            };

            return (actionToStart, newTask);
        }

        Task TryPeekOne()
        {
            lock (_locker)
            {
                var firstTask = _tasksHashSet.First();
                if (_tasksHashSet.Remove(firstTask))
                    RunningTasksCount--;
                return firstTask;
            }
        }

        void Remove(Task t)
        {
            lock (_locker)
            {
                if (_tasksHashSet.Remove(t))
                    RunningTasksCount--;
            }
        }

        void Add(Task t)
        {
            lock (_locker)
            {
                _tasksHashSet.Add(t);
                RunningTasksCount++;
            }
        }
    }
}
