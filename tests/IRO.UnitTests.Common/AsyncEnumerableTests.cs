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
            }, AsyncLinqContext.Create(maxThreadsCount: 5));

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
            }, AsyncLinqContext.Create(maxThreadsCount: 1));

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
            });

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
            });

            Assert.AreEqual(5, newList.Count);
        }

        [Test]
        public async Task NestingTest()
        {
            var twoDimensionalArray = new int[10][];
            for (int i = 0; i < twoDimensionalArray.Length; i++)
            {
                var innerArray = new int[10];
                twoDimensionalArray[i] = innerArray;
                for (int j = 0; j < innerArray.Length; j++)
                {
                    innerArray[j] = 5;
                }
            }

            var locker = new object();
            //var elementsSum1 = 0;
            //await twoDimensionalArray.ForEachAsync(async (innerArray, position) =>
            //{
            //    await innerArray.ForEachAsync(async (item, position) =>
            //    {
            //        lock (locker)
            //            elementsSum1 += item;
            //    });
            //});
            //Assert.AreEqual(500, elementsSum1);

            var context = AsyncLinqContext.Create(2);
            var elementsSum2 = 0;
            await twoDimensionalArray.ForEachAsync(async (innerArray, position) =>
            {
                await innerArray.ForEachAsync(async (item, position) =>
                {
                    lock (locker)
                        elementsSum2 += item;
                }, context);
            }, context);
            Assert.AreEqual(500, elementsSum2);
        }
    }
}
