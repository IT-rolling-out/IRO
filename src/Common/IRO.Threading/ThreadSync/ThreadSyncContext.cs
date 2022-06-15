using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IRO.Threading.ThreadSync;

namespace IRO.Threading.ThreadSync
{
    /// <summary>
    /// Class that help execute code on specific thread.
    /// Specially designed to pass all invoked delegates results and exceptions
    /// to calling thread and make work with it more simple.
    /// </summary>
    public class ThreadSyncContext
    {
        public IThreadSyncInvoker Invoker => _invoker;

        readonly IThreadSyncInvoker _invoker;

        public ThreadSyncContext(IThreadSyncInvoker threadSyncInvoker)
        {
            _invoker = threadSyncInvoker ?? throw new ArgumentNullException(nameof(threadSyncInvoker));
        }

        /// <summary>
        /// Invoke in specific tread synchronously and return result or throw 
        /// exception to calling (not specific) thread.
        /// </summary>
        public TResult Invoke<TResult>(Func<TResult> func)
        {
            var res = default(TResult);
            Exception origException = null;
            _invoker.Invoke(() =>
            {
                try
                {
                    res = func();
                }
                catch (Exception ex)
                {
                    origException = ex;
                }
            });

            if (origException == null)
            {
                return res;
            }
            else
            {
                var exToThrow = new ThreadSyncException(origException);
#if DEBUG
                Debug.WriteLine(exToThrow.ToString());
#endif
                throw exToThrow;
            }
        }

        /// <summary>
        /// Invoke in specific tread asynchronously.
        /// </summary>
        public async Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _invoker.InvokeAsync(async () =>
            {
                try
                {
                    var res = await func();
                    tcs.SetResult(res);
                }
                catch (Exception ex)
                {
                    var exToThrow = new ThreadSyncException(ex);
#if DEBUG
                    Debug.WriteLine(exToThrow.ToString());
#endif
                    tcs.SetException(exToThrow);
                }
            });
            return await tcs.Task;
        }

        /// <summary>
        /// Invoke in specific tread synchronously and if exception - throw 
        /// exception to WAITER (not ui) thread.
        /// </summary>
        public void Invoke(Action act)
        {
            Invoke<object>(() =>
            {
                act();
                return null;
            });
        }

        /// <summary>
        /// Invoke in specific thread asynchronously and if exception - throw 
        /// exception to AWAITER thread.
        /// </summary>
        public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
        {
            return await InvokeAsync(async () => func());
        }

        /// <summary>
        /// Invoke in specific thread asynchronously and if exception - throw 
        /// exception to AWAITER thread.
        /// </summary>
        public async Task InvokeAsync(Action act)
        {
            await InvokeAsync(async () => act());
        }

        /// <summary>
        /// Invoke in specific tread asynchronously with try/catch.
        /// </summary>
        public async Task TryInvokeAsync(Action act)
        {
            try
            {
                await InvokeAsync(act);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Invoke in specific tread synchronously with try/catch.
        /// </summary>
        public void TryInvoke(Action act)
        {
            try
            {
                Invoke(act);
            }
            catch
            {

            }
        }
    }
}
