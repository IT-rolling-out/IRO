using LiteDB;
using Newtonsoft.Json;
using ItRollingOut;
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
using ItRollingOut.Common.Diagnostics;

namespace ItRollingOut.Storage.WithLiteDB
{
    /// <summary>
    /// Storage in json file.
    /// </summary>
    public class LiteDatabaseLocalStorage : IKeyValueStorage
    {
        const string DefaultDbFileName = "localstorage.db";

        readonly object Locker = new object();
        string _collectionName;
        string _dbFilePath;
        //LiteDatabase _db;
        //LiteCollection<KeyValuePair<string, string>> _collection;

        public LiteDatabaseLocalStorage() : this("def_storage_collection")
        {
        }

        public LiteDatabaseLocalStorage(string collectionName) : this(
            collectionName,
            CommonHelpers.GetExecutableAssemblyDir() + DefaultDbFileName
            )
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuesNamespace">File name or same.</param>
        public LiteDatabaseLocalStorage(string collectionName, string dbFilePath)
        {
            Initialize(
               collectionName,
               dbFilePath
               );

        }

        void Initialize(string collectionName, string dbFilePath)
        {
            _collectionName = collectionName;
            _dbFilePath = dbFilePath;

            //_db = new LiteDatabase(_dbFilePath);
            //_collection = _db.GetCollection<KeyValuePair<string, string>>(_collectionName);
        }

        /// <summary>
        /// Automatically synchronized with Get. If 'null' - will remove value.
        /// If you not closing application, recommended not await task.
        /// </summary>
        public Task Set(string key, object value)
        {
            var are = new AutoResetEvent(false);
            var resTask = Task.Run(() =>
             {
                 lock (Locker)
                 {
                     are.Set();
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
             });
            are.WaitOne();
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Automatically synchronized with Set. 
        /// If key not exists - will return null for reference type and default value for value types.
        /// </summary>
        public Task<T> Get<T>(string key)
        {
            var res = (T)_Get(key, typeof(T));
            return Task.FromResult(res);
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

        object _Get(string key, Type t)
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
                        if (t.IsValueType)
                            return Activator.CreateInstance(t);
                        else
                            return null;
                    }
                    jsonStr = keyValPair["Value"];
                }

                //Use json for more simple convertation.
                var res = JsonConvert.DeserializeObject(jsonStr, t);
                return res;
            }
        }

        public Task ClearAll()
        {
            lock (Locker)
            {
                using (var _db = new LiteDatabase(_dbFilePath))
                {
                    _db.DropCollection(_collectionName);
                }
            }
            return Task.FromResult<object>(null);
        }
    }
}