using System.Threading.Tasks;
using IRO.Storage.WithMongoDB;
using MongoDB.Driver;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public class MongoStorageTests : BaseStorageTests
    {
        public MongoStorageTests()
        {
            var client =
                new MongoClient(
                    "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var db = client.GetDatabase("storage_test_db");
            Storage = new MongoDatabaseStorage(db);
        }
    }
}