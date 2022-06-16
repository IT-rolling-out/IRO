using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IRO.Threading
{
    public class TaskPool
    {
        readonly object _locker = new object();

        readonly HashSet<Task> _tasksHashSet = new HashSet<Task>();

        readonly ThreadLocal<bool> _isCurrentThreadInPool = new ThreadLocal<bool>();

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

        public Task Start<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }


            (Func<Task> wrapActionToStart, Task<T> newTask) = WrapToTaskCompletionSource(func, cancellationToken);

            //Must synchronously check and synchronously write to HashSet.
            lock (_locker)
            {
                if (RunningTasksCount < MaxThreadsCount)
                {
                    Add(newTask);
                    wrapActionToStart();
                }
                else
                {
                    if (_isCurrentThreadInPool.Value)
                    {
                        var firstTaskFromPool = FirstOrDefault();
                        firstTaskFromPool.ContinueWith((t) =>
                        {
                            wrapActionToStart();
                        });
                    }
                    else
                    {
                        return wrapActionToStart();
                    }
                }
            }
            return Task.FromResult<object>(null);
        }


        (Func<Task> WrapActionToStart, Task<T> NewTask) WrapToTaskCompletionSource<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            var newTask = tcs.Task;

            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                Remove(newTask);
            });

            Func<Task> actionToStart = async () =>
               {
                   _isCurrentThreadInPool.Value = true;
                   try
                   {
                       if (cancellationToken.IsCancellationRequested)
                       {
                           tcs.TrySetCanceled();
                       }
                       else
                       {
                           var res = await func();
                           tcs.TrySetResult(res);
                       }
                   }
                   catch (Exception ex)
                   {
                       tcs.TrySetException(ex);
                   }
                   finally
                   {
                       Remove(newTask);
                       _isCurrentThreadInPool.Value = false;
                   }
               };

            return (actionToStart, newTask);
        }

        Task FirstOrDefault()
        {
            lock (_locker)
            {
                return _tasksHashSet.FirstOrDefault();
            }
        }

        Task TryPeekOne()
        {
            lock (_locker)
            {
                var firstTask = _tasksHashSet.First();
                _tasksHashSet.Remove(firstTask);
                RunningTasksCount--;
                return firstTask;
            }
        }

        void Remove(Task t)
        {
            lock (_locker)
            {
                _tasksHashSet.Remove(t);
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
