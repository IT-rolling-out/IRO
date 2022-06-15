using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IRO.Threading.AsyncLinq;

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
                if (MaxThreadsCount < _threadsCount)
                    MaxThreadsCount = _threadsCount;
                //Console.WriteLine($"Threads count: {_threadsCount}.");

            }
        }

        static int MaxThreadsCount { get; set; }

        [Test]
        public async Task AsyncForeachTest()
        {
            //Expected time is 500ms

            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
            };
            object locker = new object();
            int counter = 0;

            await list.ForEachAsync(async (item, position) =>
            {
                lock (locker)
                    ThreadsCount++;
                await Task.Delay(100);
                Assert.AreEqual(item, position);
                lock (locker)
                    counter++;
                Console.WriteLine($"Item: {item}, position: {position}");
                lock (locker)
                    ThreadsCount--;
            }, AsyncLinqContext.Create(maxThreadsCount: 5));

            Console.WriteLine("Async foreach finished.");
            Console.WriteLine($"Max threads count: {MaxThreadsCount}.");
            Assert.AreEqual(20, counter);
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
            //Expected time is 3000ms

            //Make three-dimensional array 10x10x10, filled with 5
            var threeD = new int[10][][];
            for (int i = 0; i < threeD.Length; i++)
            {
                var twoD = new int[10][];
                threeD[i] = twoD;
                for (int j = 0; j < twoD.Length; j++)
                {
                    var oneD = new int[10];
                    twoD[j] = oneD;
                    for (int h = 0; h < oneD.Length; h++)
                    {
                        oneD[h] = 5;
                    }
                }
            }

            var locker = new object();
            var context = AsyncLinqContext.Create(20);
            var elementsSum2 = 0;

            await threeD.ForEachAsync(async (twoD, position) =>
            {
                int twoDLevelCounter = 0;
                await twoD.ForEachAsync(async (oneD, position) =>
                {
                    await oneD.ForEachAsync(async (item, position) =>
                    {
                        lock (locker)
                            ThreadsCount++;
                        await Task.Delay(50);
                        lock (locker)
                            elementsSum2 += item;
                        lock (locker)
                            ThreadsCount--;

                        twoDLevelCounter++;
                    }, context);
                    await Task.Delay(5);


                    twoDLevelCounter++;
                }, context);
                await Task.Delay(5);

                //100 means that all iterations throuh threeD[i] completed
                Assert.AreEqual(100, twoDLevelCounter);
            }, context);
            Console.WriteLine($"Max threads count: {MaxThreadsCount}.");
            Assert.AreEqual(5000, elementsSum2);
        }
    }
}
