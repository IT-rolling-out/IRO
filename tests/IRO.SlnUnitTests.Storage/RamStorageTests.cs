using System;
using System.IO;
using System.Threading.Tasks;
using IRO.Storage.DefaultStorages;
using NUnit.Framework;

namespace IRO.SlnUnitTests.Storage
{
    public class RamStorageTests
    {
        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNullThrows(new RamCacheStorage());
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(new RamCacheStorage());
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(new RamCacheStorage());
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(new RamCacheStorage());
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(new RamCacheStorage());
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(new RamCacheStorage());
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(new RamCacheStorage());
        }
    }
}