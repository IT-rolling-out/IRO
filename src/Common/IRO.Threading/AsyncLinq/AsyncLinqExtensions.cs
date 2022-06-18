using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;
using IRO.Common.Collections;
using System.Threading;

namespace IRO.Threading.AsyncLinq
{

    public static class AsyncLinqExtensions
    {
        public static async Task<IEnumerable<R>> SelectAsync<T, R>(
            this IEnumerable<T> @this,
            Func<T, R> act,
            CancellationToken cancellationToken = default
            )
        {
            return await @this.SelectAsync((item, position) =>
            {
                var res = act(item);
                return Task.FromResult<R>(res);
            }, cancellationToken);
        }

        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> @this,
            Func<T, bool> act,
            CancellationToken cancellationToken = default
            )
        {
            return await @this.WhereAsync((item, position) =>
            {
                var isInclude = act(item);
                return Task.FromResult<bool>(isInclude);
            }, cancellationToken);
        }


        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> @this,
            WhereAsyncDelegate<T> act,
            CancellationToken cancellationToken = default
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
            }, cancellationToken);

            var resEnum = intermediateList
                .Where(r => r.IsIncluded)
                .Select(r => r.Item);
            return resEnum;
        }

        public static async Task<IEnumerable<R>> SelectAsync<T, R>(
            this IEnumerable<T> @this,
            SelectAsyncDelegate<T, R> act,
            CancellationToken cancellationToken = default
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
            }, cancellationToken);
            return resArray;
        }

        public static async Task ForEachAsync<T>(
            this IEnumerable<T> @this,
            Func<T, Task> act,
            CancellationToken cancellationToken = default
        )
        {
            await @this.ForEachAsync(async (item, position) =>
            {
                await act(item);
            }, cancellationToken);
        }

        public static async Task ForEachAsync<T>(
            this IEnumerable<T> @this,
            ForEachAsyncDelegate<T> act,
            CancellationToken cancellationToken = default
        )
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }
            var tasksList = new List<Task>();
            var position = 0;

            foreach (var item in @this)
            {
                var positionLocal = position;
                position++;
                cancellationToken.ThrowIfCancellationRequested();

                var runTask = TaskPool.Global.Run(async () =>
                {
                    await act(item, positionLocal);
                }, cancellationToken);
                tasksList.Add(runTask);
            }

            foreach (var task in tasksList)
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}
