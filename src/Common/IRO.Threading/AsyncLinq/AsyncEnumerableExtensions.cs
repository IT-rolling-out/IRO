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
            if (threadsCount < 1)
            {
                throw new ArgumentException($"Value is '{threadsCount}'. Must be bigger than 1.", nameof(threadsCount));
            }

            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            var position = 0;
            var tasksHashSet = new ConcurrentHashSet<Task>();
            foreach (var item in @this)
            {
                if (tasksHashSet.Count >= threadsCount)
                {
                    await tasksHashSet.First();
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
                          while (!tasksHashSet.TryRemove(newTask))
                          {
                          }
                      }
                  });
                tasksHashSet.Add(newTask);
                newTask.Start();
                position++;
            }

            while (tasksHashSet.Count > 0)
            {
                await tasksHashSet.FirstOrDefault();
            }
        }
    }
}
