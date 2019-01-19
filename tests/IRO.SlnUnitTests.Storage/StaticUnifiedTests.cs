using System.Collections.Generic;
using System.Threading.Tasks;
using IRO.SlnUnitTests.MyStorage;
using IRO.Storage;
using NUnit.Framework;

namespace IRO.SlnUnitTests.Storage
{
    public static class StaticUnifiedTests
    {
        /// <summary>
        /// Must throw exception.
        /// </summary>
        /// <param name="storage"></param>
        public static async Task TestGetNullThrows(IKeyValueStorage storage)
        {
            var key = "testValue_32453v4ar";
            await storage.Set(key, "val");
            await storage.Set(key, null);
            try
            {
                await storage.Get<string>(key);
                Assert.Fail("Storage.Get must throw exception for values, that null or doesn`t exists.");
            }
            catch
            {
                Assert.Pass();
            }
        }

        public static async Task TestGetOrDefaultForValueType(IKeyValueStorage storage)
        {
            var key = "testValue1_c34a33";
            await storage.Set(key, null);
            var num=await storage.GetOrDefault<int>(key);
            Assert.AreEqual(num, default(int));
        }

        public static async Task ComplexObjectTest(IKeyValueStorage storage)
        {
            var dictLikeComplexObj = new Dictionary<string, object>()
                {
                    { "Val1","10"},
                    { "Val2","aerertavt"},
                    {
                        "Val3",
                        new Dictionary<string, object>()
                        {
                            { "Val1",11},
                            { "Val2","fhg hgh fhxghhg"},
                        }
                    }
                };
            await storage.Set("complex", dictLikeComplexObj);
            var obj = await storage.Get<ComplexType>("complex");
            Assert.AreEqual(obj.Val1, 10);
            Assert.AreEqual(obj.Val3.Val1, 11);
        }

        public static void TaskWaitDefaultCall(IKeyValueStorage storage)
        {
            var key = "D991231h323";
            storage.Set(key, 100).Wait();
            var val = storage.Get<int>(key).Result;
            Assert.AreEqual(val, 100);
        }

        public static async Task DefaultCall(IKeyValueStorage storage)
        {
            var key = "D99h323";
            await storage.Set(key, 100);
            var val=await storage.Get<int>(key);
            Assert.AreEqual(val, 100);
        }

        public static async Task ContainsTest(IKeyValueStorage storage)
        {
            var key = "wad323";
            await storage.Set(key, "asda");
            var isContains = await storage.ContainsKey(key);
            Assert.IsTrue(isContains);
            await storage.Set(key, null);
            isContains = await storage.ContainsKey(key);
            Assert.IsFalse(isContains);

        }

        public static async Task SynchronizationTest(IKeyValueStorage storage)
        {
            for (int i = 0; i < 50; i++)
            {
                string key = "key" + i.ToString();
                string prevKey = "key" + (i-1).ToString();

                await storage.Clear();
                var prevVal = await storage.GetOrNull<string>(prevKey);
                if (prevVal != null)
                {
                    Assert.Fail($"Not null value after cleaning. Iteration {i}.");
                }

                await storage.Set(key,"val");
                var val = await storage.Get<string>(key);
                Assert.AreEqual(val, "val");
                var isContains=await storage.ContainsKey(key);
                Assert.IsTrue(isContains);
            }
        }

        
    }
}