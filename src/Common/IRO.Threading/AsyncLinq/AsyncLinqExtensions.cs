using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;

namespace IRO.Threading.AsyncLinq
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
            //Because last thread run synchronously.
            maxThreadsCount--;
            var cancelToken = asyncLinqContext.CancellationToken;
            var tasksHashSet = asyncLinqContext.RunningTasksHashSet;

            var isCurrentLevelNested = asyncLinqContext.IsNesting;
            asyncLinqContext.IsNesting = true;

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

                if (Lock_HashSetGetCount(tasksHashSet) >= maxThreadsCount)
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
                            Lock_HashSetRemove(tasksHashSet, newTask);
                        }
                    }, cancelToken);
                    lock (tasksHashSet)
                    {
                        tasksHashSet.Add(newTask);
                        newTask.Start();
                    }
                }
            }

            if (!isCurrentLevelNested)
            {
                while (Lock_HashSetGetCount(tasksHashSet) > 0)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    var firstTask = Lock_HashSetFirstOrDefault(tasksHashSet);
                    if (firstTask != null)
                    {
                        await firstTask;
                    }
                }
                asyncLinqContext.IsNesting = false;
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
