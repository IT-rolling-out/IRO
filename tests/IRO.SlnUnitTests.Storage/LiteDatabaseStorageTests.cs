using System.Threading.Tasks;
using IRO.Storage.DefaultStorages;
using IRO.Storage.WithLiteDB;
using NUnit.Framework;

namespace IRO.SlnUnitTests.Storage
{
    public class LiteDatabaseStorageTests
    {
        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNullThrows(new LiteDatabaseStorage("collection1"));
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(new LiteDatabaseStorage("collection2"));
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(new LiteDatabaseStorage("collection3"));
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(new LiteDatabaseStorage("collection4"));
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(new LiteDatabaseStorage("collection5"));
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(new LiteDatabaseStorage("collection6"));
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(new LiteDatabaseStorage("collection7"));
        }
    }
}