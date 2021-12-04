using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IRO.Storage;
using IRO.UnitTests.Storage.Data;
using NUnit.Framework;

namespace IRO.UnitTests.Storage
{
    public static class StaticUnifiedTests
    {
        public static async Task TestScopes(IKeyValueStorage storage)
        {
            await storage.Set("ComplexType.Val1", 100);
            await storage.Set("ComplexType.Val2", "First scope string");
            await storage.Set("ComplexType.Val3.Val1", 200);
            await storage.Set("ComplexType.Val3.Val2", "Second scope string");
            await storage.Set("ComplexType.Val3.Val3.Val1", 300);
            await storage.Set("ComplexType.Val3.Val3.Val2", "Third scope string");

            var num = await storage.Get<int>("ComplexType.Val1");
            Assert.AreEqual(num, 100);
            num = await storage.Get<int>("ComplexType.Val3.Val1");
            Assert.AreEqual(num, 200);

            var str = await storage.Get<string>("ComplexType.Val3.Val2");
            Assert.AreEqual(str, "Second scope string");

            var complexType = await storage.Get<ComplexType>("ComplexType");
            Assert.AreEqual(complexType.Val3.Val3.Val1, 300);
            complexType = await storage.Get<ComplexType>("ComplexType.Val3");
            Assert.AreEqual(complexType.Val3.Val1, 300);

            await storage.Remove("ComplexType.Val3.Val3.Val1");
            var isContains = await storage.ContainsKey("ComplexType.Val3.Val3.Val1");
            Assert.IsFalse(isContains);
            isContains = await storage.ContainsKey("ComplexType.Val3.Val3");
            Assert.IsTrue(isContains);

            await storage.Remove("ComplexType.Val3.Val3.Val2");
            isContains = await storage.ContainsKey("ComplexType.Val3.Val3");
            Assert.IsFalse(isContains);
            isContains = await storage.ContainsKey("ComplexType");
            Assert.IsTrue(isContains);

            await storage.Set("ComplexType", 999);
            num = await storage.Get<int>("ComplexType");
            Assert.AreEqual(num, 999);
            isContains = await storage.ContainsKey("ComplexType.Val3.Val3");
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val1"));
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val2"));
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val3"));
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val3.Val1"));
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val3.Val3"));
            Assert.IsFalse(isContains);
            isContains = (await storage.ContainsKey("ComplexType.Val3.Val3.Val1"));
            Assert.IsFalse(isContains);
        }

        /// <summary>
        /// Must throw exception.
        /// </summary>
        /// <param name="storage"></param>
        public static async Task TestGetNull(IKeyValueStorage storage)
        {
            var key = "value_TestGetNull";
            await storage.Set(key, null);
            var value = await storage.Get<string>(key);
            Assert.IsNull(value);
        }

        public static async Task TestGetOrDefaultForValueType(IKeyValueStorage storage)
        {
            var key = "testValue1_c34a33";
            await storage.Set(key, null);
            var num = await storage.GetOrDefault<int>(key);
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
            await Task.Delay(2000);
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
            var val = await storage.Get<int>(key);
            Assert.AreEqual(val, 100);
        }

        public static async Task ContainsTest(IKeyValueStorage storage)
        {
            var key = "value_ContainsTest";
            await storage.Set(key, "asda");
            var isContains = await storage.ContainsKey(key);
            Assert.IsTrue(isContains);
            await storage.Set(key, null);
            isContains = await storage.ContainsKey(key);
            Assert.IsTrue(isContains);
            await storage.Remove(key);
            isContains = await storage.ContainsKey(key);
            Assert.IsFalse(isContains);
        }

        public static async Task SynchronizationTest(IKeyValueStorage storage)
        {
            await storage.Clear();

            var rd = new Random();
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    await storage.Set("key" + rd.Next(10000, 99999).ToString(), "qwwwwwwww");
                }


                string key = "key" + i.ToString();
                string prevKey = "key" + (i - 1).ToString();

                //await storage.Clear();
                await storage.Set(prevKey, null);
                var prevVal = await storage.GetOrDefault<string>(prevKey);
                if (prevVal != null)
                {
                    Assert.Fail($"Not null value after cleaning. Iteration {i}.");
                }

                await storage.Set(key, "val");
                var val = await storage.Get<string>(key);
                Assert.AreEqual(val, "val");
                var isContains = await storage.ContainsKey(key);
                Assert.IsTrue(isContains);
            }

            await storage.Clear();
        }

        public static async Task ReadTest(IKeyValueStorage storage)
        {
            await storage.Clear();

            var rd = new Random();
            for (int i = 0; i < 50; i++)
            {
                await storage.Set("somekey" + i, "qwwwwwwww");
            }

            await storage.Set("mykey", "val");
            for (int i = 0; i < 500; i++)
            {
                var val = await storage.Get<string>("mykey");
                Assert.AreEqual("val", val);
            }
            await storage.Clear();
        }


    }
}