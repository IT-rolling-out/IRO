using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;

namespace IRO.Threading.AsyncLinq
{

    internal struct WhereSelectionTuple<T>
    {
        public T Item { get; set; }

        public bool IsIncluded { get; set; }
    }

    public static class AsyncLinqExtensions
    {
        public static async Task<IList<T>> WhereAsync<T>(
            this ICollection<T> @this,
            WhereAsyncDelegate<T> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            var resList = new List<T>();
            await @this.ForEachAsync(async (item, position) =>
            {
                var isIncluded = await act(item, position);
                if (isIncluded)
                {
                    lock (resList)
                    {
                        resList.Add(item);
                    }
                }
            }, asyncLinqContext);
            return resList;
        }

        public static async Task<IList<R>> SelectAsync<T, R>(
            this ICollection<T> @this,
            SelectAsyncDelegate<T, R> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            var resArray = new R[@this.Count];
            await @this.ForEachAsync(async (item, position) =>
            {
                var selectedRes = await act(item, position);
                lock (resArray)
                {
                    resArray[position] = selectedRes;
                }
            }, asyncLinqContext);
            return resArray;
        }



        public static async Task ForEachAsync<T>(
            this IEnumerable<T> @this,
            ForEachAsyncDelegate<T> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            asyncLinqContext ??= AsyncLinqContext.Create();
            var maxThreadsCount = asyncLinqContext.MaxThreadsCount;

            //Because last thread run synchronously.
            maxThreadsCount--;

            var cancelToken = asyncLinqContext.CancellationToken;
            var runCtx = new RunningTasksContext(asyncLinqContext);

            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }
            var position = 0;
            foreach (var item in @this)
            {
                var positionLocal = position;
                position++;
                cancelToken.ThrowIfCancellationRequested();

                if (runCtx.GetGlobalCount() >= maxThreadsCount)
                {
                    await act(item, positionLocal);
                }
                else
                {

                    Task newTask = null;
                    newTask = new Task(async () =>
                    {
                        try
                        {
                            await act(item, positionLocal);
                        }
                        finally
                        {
                            runCtx.Remove(newTask);
                        }
                    }, cancelToken);

                    newTask.Start();
                    runCtx.Add(newTask);
                }
            }

            var taskToWait = runCtx.FirstOrDefault();
            while (taskToWait != null)
            {
                cancelToken.ThrowIfCancellationRequested();
                await taskToWait;
                taskToWait = runCtx.FirstOrDefault();
            }
        }

    }
}
