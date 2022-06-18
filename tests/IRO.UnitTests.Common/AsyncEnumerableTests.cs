using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using IRO.Threading.AsyncLinq;
using System.Linq;
using IRO.UnitTests.Common.Helpers;

namespace IRO.UnitTests.Common
{

    internal class AsyncEnumerableTests
    {
        ThreadsCounter _threadsCounter;

        [SetUp]
        public void Init()
        {
            _threadsCounter = new ThreadsCounter();
        }


        [Test]
        public async Task AsyncForeachTest()
        {
            //Expected time is 500ms

            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
            };
            int counter = 0;
            var locker = new object();
            int expectedThreadsCount = 5;

            await list.ForEachAsync(async (item, position) =>
            {
                _threadsCounter.ThreadStart();
                await HardWait.Delay(100);
                Assert.AreEqual(item, position);
                lock (locker)
                    counter++;
                Console.WriteLine($"Item: {item}, position: {position}");
                _threadsCounter.ThreadEnd();
            });

            Console.WriteLine("Async foreach finished.");
            _threadsCounter.PrintMsg();
            Assert.AreEqual(20, counter);
            //Assert.AreEqual(expectedThreadsCount, _threadsCounter.MaxThreadsCount);
        }

        [Test]
        public async Task SelectAsyncTest()
        {
            var list = new List<int>();
            for(int i = 0; i < 10000; i++)
            {
                list.Add(i);
            }

            var newList = (await list.SelectAsync(item => 1000 + item)).ToList();

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

            var newList = (await list.WhereAsync(item => item % 2 == 0)).ToList();
            Assert.AreEqual(5, newList.Count);
            //Check order
            for (int i = 0; i < newList.Count; i++)
            {
                Assert.AreEqual(i * 2, newList[i]);
            }
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
            var elementsSum2 = 0;
            int expectedThreadsCount = 20;

            await threeD.ForEachAsync(async (twoD, position) =>
            {
                int twoDLevelCounter = 0;
                await twoD.ForEachAsync(async (oneD, position) =>
                {
                    await oneD.ForEachAsync(async (item, position) =>
                    {
                        _threadsCounter.ThreadStart();
                        await HardWait.Delay(50);
                        lock (locker)
                            elementsSum2 += item;
                        _threadsCounter.ThreadEnd();
                        lock (locker)
                            twoDLevelCounter++;
                    });
                    await HardWait.Delay(5);
                });
                await HardWait.Delay(5);

                //100 means that all iterations throuh threeD[i] completed
                Assert.AreEqual(100, twoDLevelCounter);
            });
            _threadsCounter.PrintMsg();
            Assert.AreEqual(5000, elementsSum2);
            //Assert.AreEqual(expectedThreadsCount, _threadsCounter.MaxThreadsCount);
        }
    }
}
