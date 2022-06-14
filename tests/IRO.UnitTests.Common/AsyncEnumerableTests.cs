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
        [Test]
        public async Task AsyncForeachTest()
        {
            var list = new List<int>()
            {
                0,1,2,3,4,5,6,7,8,9
            };

            int counter = 0;
            await list.ForEachAsync(async (item, position) =>
            {
                await Task.Delay(1000);
                Assert.AreEqual(item, position);
                counter++;
                Console.WriteLine($"Item: {item}, position: {position}");
            }, 100);

            Assert.AreEqual(10, counter);
            Console.WriteLine("Async foreach finished.");
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
    }
}
