using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache;
using NUnit.Framework;

namespace IRO.SlnUnitTests
{
    public class RamCacheTests
    {
        [Test]
        public async Task FitInsertPositionTest()
        {
            int limit = 500;
            var cache = new RamCache(recordsLimit: limit);
            var doubleLimit = limit * 2;
            for (int i = 0; i < doubleLimit; i++)
            {
                await cache.Set("key" + i.ToString(), i);
            }

            for (int i = 0; i < limit-1; i++)
            {
                var val = await cache.TryGet<int?>("key" + i.ToString());
                Assert.IsNull(val);
            }

            for (int i = limit+1; i < doubleLimit; i++)
            {
                var val = await cache.TryGet<int?>("key" + i.ToString());
                Assert.AreEqual(i, val);
            }
        }

        [Test]
        public async Task FitTest()
        {
            var cache = new RamCache(recordsLimit: 5);
            for (int i = 0; i < 100; i++)
            {
                await cache.Set("key" + i.ToString(), i);
            }

            await cache.Set("mykey", "val");
            var val=await cache.TryGet<string>("mykey");
            Assert.AreEqual("val", val);
        }
    }
}
