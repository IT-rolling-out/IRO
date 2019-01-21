using System;
using System.IO;
using System.Threading.Tasks;
using IRO.Storage.DefaultStorages;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public class FileStorageTests
    {
        [Test]
        public void TestStorageFileExists()
        {
            string name = "storage_dasdadf3r2fd.json";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(filePath))
                File.Delete(filePath);
            var storage = new FileStorage(name);
            if (!File.Exists(filePath))
                Assert.Fail("Storage file doesn`t created.");
        }

        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNullThrows(new FileStorage("storage1.json"));
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(new FileStorage("storage2.json"));
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(new FileStorage("storage3.json"));
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(new FileStorage("storage4.json"));
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(new FileStorage("storage5.json"));
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(new FileStorage("storage6.json"));
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(new FileStorage("storage7.json"));
        }

        [Test]
        public async Task ReadTest()
        {
            await StaticUnifiedTests.ReadTest(new FileStorage("storage8.json"));
        }
    }
}