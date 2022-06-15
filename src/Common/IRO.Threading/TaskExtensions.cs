using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IRO.Threading
{
    public static class TaskExtensions
    {
        /// <summary>
        /// WhenAll with timeout.
        /// <para></para>
        /// Wait all tasks completition with try|cathc block.
        /// </summary>
        /// <returns>True if thread terminated because of timeout.</returns>
        public static async Task<bool> WhenAll(IEnumerable<Task> tasks, TimeSpan? timeout=null)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var mainTask = Task.Run(async () =>
            {
                foreach (var t in tasks)
                {
                    try
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        await t;
                    }
                    catch
                    {
                    }
                }
            }, cancellationTokenSource.Token);

            if (timeout == null)
            {
                await mainTask;
                return false;
            }
            else
            {
                var delayTask = Task.Delay(timeout.Value);
                await Task.WhenAny(delayTask, mainTask);
                if (mainTask.Status == TaskStatus.Running)
                {
                    cancellationTokenSource.Cancel();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
