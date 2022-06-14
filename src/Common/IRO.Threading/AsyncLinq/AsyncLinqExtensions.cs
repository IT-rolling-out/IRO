using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;

namespace IRO.Threading
{
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
            var cancelToken = asyncLinqContext.CancellationToken;
            var tasksHashSet = asyncLinqContext.RunningTasksHashSet;

            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }
            var position = 0;
            foreach (var item in @this)
            {
                cancelToken.ThrowIfCancellationRequested();
                while (Lock_HashSetGetCount(tasksHashSet) >= maxThreadsCount)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    var firstTask = Lock_HashSetFirstOrDefault(tasksHashSet);
                    if (firstTask != null)
                    {
                        await firstTask;
                    }
                }

                Task newTask = null;
                var positionLocal = position;
                newTask = new Task(async () =>
                {
                    try
                    {
                        await act(item, positionLocal);
                    }
                    finally
                    {
                        Lock_HashSetRemove(tasksHashSet, newTask);
                    }
                }, cancelToken);
                lock (tasksHashSet)
                {
                    tasksHashSet.Add(newTask);
                    newTask.Start();
                }
                position++;
            }

            while (Lock_HashSetGetCount(tasksHashSet) > 0)
            {
                cancelToken.ThrowIfCancellationRequested();
                var firstTask = Lock_HashSetFirstOrDefault(tasksHashSet);
                if (firstTask != null)
                {
                    await firstTask;
                }
            }

        }

        static int Lock_HashSetGetCount<T>(HashSet<T> hashSet)
        {
            lock (hashSet)
            {
                return hashSet.Count;
            }
        }

        static void Lock_HashSetRemove<T>(HashSet<T> hashSet, T item)
        {
            lock (hashSet)
            {
                if (!hashSet.Remove(item))
                {
                    throw new Exception();
                }
            }
        }

        static T Lock_HashSetFirstOrDefault<T>(HashSet<T> hashSet)
        {
            lock (hashSet)
            {
                return hashSet.FirstOrDefault();
            }
        }
    }
}
