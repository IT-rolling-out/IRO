using IRO.Storage.DefaultStorages;

namespace IRO.UnitTests.Storage
{
    public class FileStorageTests : BaseStorageTests
    {
        public FileStorageTests()
        {
            Storage = new FileStorage();
        }
    }
}