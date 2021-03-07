using System.Threading.Tasks;
using IRO.Storage;
using IRO.Storage.DefaultStorages;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public class FileDatabaseStorageTests
    {
        readonly IKeyValueStorage _storage;

        public FileDatabaseStorageTests()
        {
            _storage = new FileStorage();
            _storage.Clear();
        }

        [Test]
        public async Task TestScopes()
        {
            await StaticUnifiedTests.TestScopes(_storage);
        }

        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNull(_storage);
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(_storage);
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(_storage);
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(_storage);
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(_storage);
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(_storage);
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(_storage);
        }

        [Test]
        public async Task ReadTest()
        {
            await StaticUnifiedTests.ReadTest(_storage);
        }
    }
}