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
        public async Task Test1()
        {
            var cache = new RamCache(recordsLimit:5);
            cache.Set()
        }
    }
}
