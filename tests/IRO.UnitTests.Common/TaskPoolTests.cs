using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Threading;
using NUnit.Framework;

namespace IRO.UnitTests.Common
{
    public class TaskPoolTests
    {
        ThreadsCounter _threadsCounter;

        [SetUp]
        public void Init()
        {
            _threadsCounter = new ThreadsCounter();
        }

        [Test]
        public async Task Test1()
        {
            int counter = 0;
            int expectedThreadsCount = 10;

            var tasksList1 = new List<Task>();
            var taskPool = new TaskPool(expectedThreadsCount);
            for (var i = 0; i < 100; i++)
            {
                var t1 = taskPool.Run(async () =>
                     {
                         var tasksList2 = new List<Task>();
                         for (var j = 0; j < 10; j++)
                         {
                             var t2 = taskPool.Run(async () =>
                             {
                                 _threadsCounter.ThreadStart();
                                 lock (_threadsCounter)
                                     counter++;
                                 await HardWait.Delay(2);
                                 _threadsCounter.ThreadEnd();

                             });
                             tasksList2.Add(t2);
                         }
                         await Task.WhenAll(tasksList2);
                     });
                tasksList1.Add(t1);
            }
            await Task.WhenAll(tasksList1);
            _threadsCounter.PrintMsg();
            Assert.AreEqual(1000, counter);
            Assert.AreEqual(expectedThreadsCount, _threadsCounter.MaxThreadsCount);
        }
    }
}
