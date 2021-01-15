using System.Threading.Tasks;
using IRO.Storage.WithLiteDB;
using IRO.Storage.WithMongoDB;
using MongoDB.Driver;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public class MongoDatabaseStorageTests
    {
        readonly MongoDatabaseStorage _storage;

        public MongoDatabaseStorageTests()
        {
            var client =
                new MongoClient(
                    "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var db = client.GetDatabase("storage_test_db");
            _storage=new MongoDatabaseStorage(db);
        }

        [Test]
        public async Task TestGetNullThrows()
        {
            
            await StaticUnifiedTests.TestGetNullThrows(_storage);
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