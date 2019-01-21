using System.Threading.Tasks;
using IRO.Storage.DefaultStorages;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public class RamStorageTests
    {
        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNullThrows(new RamStorage());
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(new RamStorage());
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(new RamStorage());
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(new RamStorage());
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(new RamStorage());
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(new RamStorage());
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(new RamStorage());
        }

        [Test]
        public async Task ReadTest()
        {
            await StaticUnifiedTests.ReadTest(new RamStorage());
        }
    }
}