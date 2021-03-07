using System.Threading.Tasks;
using IRO.Storage;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public abstract class BaseStorageTests
    {
        protected IKeyValueStorage Storage { get; set; }

        [Test]
        public async Task TestScopes()
        {
            await StaticUnifiedTests.TestScopes(Storage);
        }

        [Test]
        public async Task TestGetNullThrows()
        {
            await StaticUnifiedTests.TestGetNull(Storage);
        }

        [Test]
        public async Task TestGetOrDefaultForValueType()
        {
            await StaticUnifiedTests.TestGetOrDefaultForValueType(Storage);
        }

        [Test]
        public async Task ComplexObjectTest()
        {
            await StaticUnifiedTests.ComplexObjectTest(Storage);
        }

        [Test]
        public void TaskWaitDefaultCall()
        {
            StaticUnifiedTests.TaskWaitDefaultCall(Storage);
        }

        [Test]
        public async Task DefaultCall()
        {
            await StaticUnifiedTests.DefaultCall(Storage);
        }

        [Test]
        public async Task ContainsTest()
        {
            await StaticUnifiedTests.ContainsTest(Storage);
        }

        [Test]
        public async Task SynchronizationTest()
        {
            await StaticUnifiedTests.SynchronizationTest(Storage);
        }

        [Test]
        public async Task ReadTest()
        {
            await StaticUnifiedTests.ReadTest(Storage);
        }
    }
}