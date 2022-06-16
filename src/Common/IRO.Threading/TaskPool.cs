using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<T> Run<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            while (true)
            {
                Thread threadToBeStarted = null;
                Task<T> currentFuncTask = null;
                Task firstTaskFromPool = null;
                bool runInCurrentThread = false;

                //Must synchronously check.
                lock (_locker)
                {
                    if (RunningTasksCount < MaxThreadsCount)
                    {
                        (threadToBeStarted, currentFuncTask) = GenerateThreadAndTask(func, cancellationToken);
                        Add(currentFuncTask);
                        threadToBeStarted.Start();
                    }
                    else
                    {
                        if (_isCurrentThreadInPool.Value)
                        {
                            runInCurrentThread = true;
                        }
                        else
                        {
                            firstTaskFromPool = FirstOrDefault();
                        }
                    }
                }

                //If pool is starving and current thread is already in pool.
                if (runInCurrentThread)
                {
                    return await func().ConfigureAwait(false);
                }
                //If pool place free and can run current task async.
                if (currentFuncTask != null)
                {
                    return await currentFuncTask.ConfigureAwait(false);
                }
                //If pool is starving but current thread is not in this pool.
                if (firstTaskFromPool != null)
                {
                    await firstTaskFromPool.ConfigureAwait(false);
                }
            }
        }


        (Thread ThreadToBeStarted, Task<T> NewTask) GenerateThreadAndTask<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            var newTask = tcs.Task;

            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                Remove(newTask);
            });

            var thread = new Thread(async () =>
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
              });

            return (thread, newTask);
        }

        Task FirstOrDefault()
        {
            lock (_locker)
            {
                return _tasksHashSet.FirstOrDefault();
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
