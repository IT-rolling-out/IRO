using LiteDB;
using Newtonsoft.Json;
using IRO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using IRO.Common.Services;

namespace IRO.Storage.WithLiteDB
{
    /// <summary>
    /// Storage in json file.
    /// </summary>
    public class LiteDatabaseStorage : IKeyValueStorage
    {
        const string DefaultDbFileName = "localstorage.db";
        readonly object Locker = new object();
        string _collectionName;
        string _dbFilePath;

        public LiteDatabaseStorage() : this("def_storage_collection")
        {
        }

        public LiteDatabaseStorage(string collectionName)
            : this(collectionName, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="dbFilePath"></param>
        /// <param name="cacheStorage">Use cache storages for much performanse. Default is RamCacheStorage.</param>
        public LiteDatabaseStorage(string collectionName, string dbFilePath)
        {
            if (dbFilePath == null)
                dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultDbFileName);

            _collectionName = collectionName;
            _dbFilePath = dbFilePath;
            using (var _db = new LiteDatabase(_dbFilePath))
            {
                var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                _collection.EnsureIndex("_id");
            }
        }

        /// <summary>
        /// Automatically synchronized with Get. If 'null' - will remove value.
        /// If you not closing application, recommended not await task.
        /// </summary>
        /// <param name="lifetime">Ignored.</param>
        public async Task Set(string key, object value)
        {

            Action asyncAct=() =>
            {
                lock (Locker)
                {
                    if (value == null)
                    {
                        using (var _db = new LiteDatabase(_dbFilePath))
                        {
                            var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                            _collection.Delete(key);
                        }
                    }
                    else
                    {
                        //Use json for more simple convertation.
                        string jsonStr = JsonConvert.SerializeObject(value);
                        using (var _db = new LiteDatabase(_dbFilePath))
                        {
                            var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                            _collection.Upsert(
                                new BsonDocument
                                {
                                    ["_id"] = key,
                                    ["Value"] = jsonStr
                                }
                            );
                        }
                    }
                }
            };
            await Task.Run(asyncAct);
        }

        /// <summary>
        /// Automatically synchronized with Set. 
        /// If key not exists - will return null for reference type and default value for value types.
        /// </summary>
        public async Task<object> Get(Type type, string key)
        {
            lock (Locker)
            {
                string jsonStr;
                using (var _db = new LiteDatabase(_dbFilePath))
                {
                    var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                    var keyValPair = _collection.FindById(key);
                    if (keyValPair == null)
                    {
                        //return default value for structs or null for class
                        if (type.IsValueType)
                            return Activator.CreateInstance(type);
                        else
                            return null;
                    }
                    jsonStr = keyValPair["Value"];
                }

                //Use json for more simple convertation.
                var res = JsonConvert.DeserializeObject(jsonStr, type);
                return res;
            }
        }

        /// <summary>
        /// Remember, that null value is equals that it not exists. 
        /// So, if before you set 'key_name' value to null, then ContainsKey will return false;
        /// </summary>
        public async Task<bool> ContainsKey(string key)
        {
            lock (Locker)
            {
                using (var _db = new LiteDatabase(_dbFilePath))
                {
                    var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                    return _collection.Exists(r => r["_id"] == key);
                }
            }
        }

        public async Task Clear()
        {
            lock (Locker)
            {
                using (var _db = new LiteDatabase(_dbFilePath))
                {
                    _db.DropCollection(_collectionName);
                }
            }
        }
    }
}