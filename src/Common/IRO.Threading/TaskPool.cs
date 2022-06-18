using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;

namespace IRO.Threading
{
    public class TaskPool
    {
        public static TaskPool Global { get; } = new TaskPool();

        readonly SmartThreadPool _smartThreadPool;

        public TaskPool(SmartThreadPool smartThreadPool = null)
        {
            _smartThreadPool = smartThreadPool ?? ThreadPool.Global;
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

            (var wrapActionToStart, var runTask) = WrapToTaskCompletionSource(func, cancellationToken);
            _smartThreadPool.QueueWorkItem(wrapActionToStart);           
            return runTask;
        }

        (Func<Task> WrapActionToStart, Task<T> RunTask) WrapToTaskCompletionSource<T>(Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            var newTask = tcs.Task;

            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });

            Func<Task> actionToStart = async () =>
            {
                try
                {
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
