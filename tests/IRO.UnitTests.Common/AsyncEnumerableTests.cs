using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Threading;
using NUnit.Framework;

namespace IRO.UnitTests.Common
{
    internal class AsyncEnumerableTests
    {
        static int _threadsCount;
        static int ThreadsCount
        {
            get
            {
                return _threadsCount;
            }
            set
            {
                _threadsCount = value;
                Console.WriteLine($"Threads count: {_threadsCount}.");

            }
        }

        [Test]
        public async Task AsyncForeachTest()
        {
            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9
            };
            object locker = new object();
            int counter = 0;

            await list.ForEachAsync(async (item, position) =>
            {
                lock (locker)
                    ThreadsCount++;
                //await Task.Delay(2000);
                Assert.AreEqual(item, position);
                lock (locker)
                    counter++;
                Console.WriteLine($"Item: {item}, position: {position}");
                lock (locker)
                    ThreadsCount--;
            }, 5);

            Console.WriteLine("Async foreach finished.");
            Assert.AreEqual(10, counter);
        }


        [Test]
        public async Task AsyncForeachOneThreadTest()
        {
            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9
            };

            var newList = new List<int>();

            await list.ForEachAsync(async (item, position) =>
            {
                Console.WriteLine($"Item: {item}, position: {position}");
                newList.Add(item);
            }, 1);

            Console.WriteLine("Async foreach finished.");
            for (int i = 0; i < newList.Count; i++)
            {
                Assert.AreEqual(i, newList[i]);
            }
        }

        [Test]
        public async Task SelectAsyncTest()
        {
            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9
            };

            var newList = await list.SelectAsync(async (item, position) =>
            {
                return 1000 + item;
            }, 100);

            for (int i = 0; i < newList.Count; i++)
            {
                Assert.AreEqual(1000 + i, newList[i]);
            }
        }

        [Test]
        public async Task WhereAsyncTest()
        {
            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9
            };

            var newList = await list.WhereAsync(async (item, position) =>
            {
                return item % 2 == 0;
            }, 100);

            Assert.AreEqual(5, newList.Count);
        }
    }
}
