using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;

namespace IRO.Threading
{
    public delegate Task ForEachAsyncDelegate<T>(T item, int position);

    public delegate Task<R> SelectAsyncDelegate<T, R>(T item, int position);

    public delegate Task<bool> WhereAsyncDelegate<T>(T item, int position);

    public static class AsyncEnumerableExtensions
    {
        const int DefaultThreadsCount = 16;

        public static async Task<IList<T>> WhereAsync<T>(
            this ICollection<T> @this,
            WhereAsyncDelegate<T> act,
            int threadsCount = DefaultThreadsCount
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
            }, threadsCount);
            return resList;
        }

        public static async Task<ICollection<R>> SelectAsync<T, R>(
            this ICollection<T> @this,
            SelectAsyncDelegate<T, R> act,
            int threadsCount = DefaultThreadsCount
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
            }, threadsCount);
            return resArray;
        }



        public static async Task ForEachAsync<T>(
            this IEnumerable<T> @this,
            ForEachAsyncDelegate<T> act,
            int threadsCount = DefaultThreadsCount
            )
        {
            await Task.Run(async () =>
            {
                if (threadsCount < 1)
                {
                    throw new ArgumentException($"Value is '{threadsCount}'. Must be bigger than 1.", nameof(threadsCount));
                }

                if (act == null)
                {
                    throw new ArgumentNullException(nameof(act));
                }

                var position = 0;
                var tasksHashSet = new HashSet<Task>();
                foreach (var item in @this)
                {
                    while (Lock_HashSetGetCount(tasksHashSet) >= threadsCount)
                    {
                        var firstTask = Lock_HashSetFirstOrDefault(tasksHashSet);
                        if (firstTask != null)
                        {
                            await firstTask;
                        }
                        Lock_HashSetRemove(tasksHashSet, firstTask);
                    }

                    Task newTask = null;
                    newTask = new Task(async () =>
                    {
                        try
                        {
                            await act(item, position);
                        }
                        finally
                        {
                            Lock_HashSetRemove(tasksHashSet, newTask);
                        }
                    });
                    lock (tasksHashSet)
                    {
                        tasksHashSet.Add(newTask);
                        newTask.Start();
                    }
                    position++;
                }

                while (Lock_HashSetGetCount(tasksHashSet) > 0)
                {
                    var firstTask = Lock_HashSetFirstOrDefault(tasksHashSet);
                    if (firstTask != null)
                    {
                        await firstTask;
                    }
                    Lock_HashSetRemove(tasksHashSet, firstTask);
                }
            });
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
                    throw new Exception();
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
