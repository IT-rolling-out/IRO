using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;
using IRO.Common.Collections;

namespace IRO.Threading.AsyncLinq
{

    public static class AsyncLinqExtensions
    {
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> @this,
            Action<T> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            await @this.ForEachAsync((item, position) =>
            {
                act(item);
                return Task.FromResult<object>(null);
            }, asyncLinqContext);
        }

        public static async Task<IEnumerable<R>> SelectAsync<T, R>(
            this IEnumerable<T> @this,
            Func<T, R> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            return await @this.SelectAsync((item, position) =>
            {
                var res = act(item);
                return Task.FromResult<R>(res);
            }, asyncLinqContext);
        }

        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> @this,
            Func<T, bool> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            return await @this.WhereAsync((item, position) =>
            {
                var isInclude = act(item);
                return Task.FromResult<bool>(isInclude);
            }, asyncLinqContext);
        }


        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> @this,
            WhereAsyncDelegate<T> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }
            var intermediateList = new WhereSelectionTuple<T>[@this.GetCount()];
            await @this.ForEachAsync(async (item, position) =>
            {
                var isIncluded = await act(item, position);
                intermediateList[position] = new WhereSelectionTuple<T>()
                {
                    IsIncluded = isIncluded,
                    Item = item
                };
            }, asyncLinqContext);

            var resEnum = intermediateList
                .Where(r => r.IsIncluded)
                .Select(r => r.Item);
            return resEnum;
        }

        public static async Task<IEnumerable<R>> SelectAsync<T, R>(
            this IEnumerable<T> @this,
            SelectAsyncDelegate<T, R> act,
            AsyncLinqContext asyncLinqContext = null
            )
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            var resArray = new R[@this.GetCount()];
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
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }
            asyncLinqContext ??= AsyncLinqContext.Create();
            var cancelToken = asyncLinqContext.CancellationToken;
            var position = 0;
            var taskPool = asyncLinqContext.TaskPool;
            var tasksList = new List<Task>();

            foreach (var item in @this)
            {
                var positionLocal = position;
                position++;
                cancelToken.ThrowIfCancellationRequested();

                var (startTask, runTask) = taskPool.Start<object>(async () =>
                  {
                      await act(item, positionLocal);
                      return null;
                  }, cancelToken);
                await startTask;
                tasksList.Add(runTask);
            }

            foreach (var task in tasksList)
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}
