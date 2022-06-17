﻿using System;
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

        readonly ThreadLocal<int> _currentThreadNestingLevel = new ThreadLocal<int>();
        int CurrentThreadNestingLevel
        {
            get => _currentThreadNestingLevel.Value;
            set => _currentThreadNestingLevel.Value = value;
        }
        bool IsCurrentThreadInPool
        {
            get => CurrentThreadNestingLevel > 0;
        }


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

        public (Task StartTask, Task<T> RunTask) Start<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }


            (var wrapActionToStart, var runTask) = WrapToTaskCompletionSource(func, cancellationToken);
            Task startTask;

            //Must synchronously check and synchronously write to HashSet.
            lock (_locker)
            {
                if (IsStarving())
                {
                    if (IsCurrentThreadInPool)
                    {
                        startTask = wrapActionToStart();
                        //startTask = Task.FromResult<object>(null);
                    }
                    else
                    {
                        var previousTaskFromPool = TryPeekOne();
                        Add(runTask);
                        previousTaskFromPool.ContinueWith(async (t) =>
                        {
                            await wrapActionToStart();
                        });
                        startTask = previousTaskFromPool;
                    }
                }
                else
                {
                    Add(runTask);
                    Task.Run(wrapActionToStart);
                    startTask = Task.FromResult<object>(null);
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
                CurrentThreadNestingLevel++;
                try
                {
                    Console.WriteLine(
                        $"---{{\n" +
                        //$"Thread id: {Thread.CurrentThread.ManagedThreadId}\n" +
                        //$"Task id: {Task.CurrentId}\n" +
                        //$"Delegate name: '{func.Method.Name}'\n" +
                        $"Nesting: {CurrentThreadNestingLevel}\n" +
                        $"}}---"
                        );
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        var res = await func();
                        CurrentThreadNestingLevel--;
                        Remove(newTask);
                        tcs.TrySetResult(res);
                    }
                }
                catch (Exception ex)
                {
                    CurrentThreadNestingLevel--;
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
