using LiteDB;
using System;
using System.IO;
using System.Threading.Tasks;
using IRO.Cache;
using IRO.Common.Text;
using IRO.Storage.Exceptions;

namespace IRO.Storage.WithLiteDB
{
    /// <summary>
    /// Storage in litedb file.
    /// Recommended to use it for storages upper than 1000 records.
    /// For another use Storage.FileStorage.
    /// </summary>
    public class LiteDatabaseStorage : IKeyValueStorage
    {
        const string ExceptionMsgTemplate = "Error with '{0}' in litedb storage.";
        const string DefaultDbFileName = "localstorage.db";
        readonly object Locker = new object();
        string _collectionName;
        readonly string _dbFilePath;
        readonly RamCache _cache;
        readonly IStringsSerializer _serializer;
        bool _useCache;

        public LiteDatabaseStorage(string collectionName="def_storage_collection")
            : this(collectionName, null, null)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="useCache">Use cache for much performanse. Default is RamCacheStorage.
        /// <para></para>
        /// Disable it for crossprocess database, because cache threadsafe only in process.
        /// </param>
        public LiteDatabaseStorage(string collectionName, string dbFilePath, IStringsSerializer serializer, bool useCache=true)
        {
            _useCache = useCache;
            _serializer = serializer ?? new JsonSimpleSerializer();
            _cache = new RamCache(1000);
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
                try
                {
                    lock (Locker)
                    {
                        if(_useCache)
                            _cache.SetSync(key, value);
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
                            string serializedStr = _serializer.Serialize(value);
                            using (var _db = new LiteDatabase(_dbFilePath))
                            {
                                var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                                _collection.Upsert(
                                    new BsonDocument
                                    {
                                        ["_id"] = key,
                                        ["Value"] = serializedStr
                                    }
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
                }
            };
            asyncAct();
            //await Task.Run(asyncAct);
        }

        /// <summary>
        /// Automatically synchronized with Set. 
        /// If key not exists - will return null for reference type and default value for value types.
        /// </summary>
        public async Task<object> Get(Type type, string key)
        {
            try
            {
                string serializedStr = null;
                lock (Locker)
                {
                    if (_useCache)
                    {
                        var cachedValue = _cache.TryGetSync(type, key);
                        if (cachedValue != null)
                            return cachedValue;
                    }

                    using (var _db = new LiteDatabase(_dbFilePath))
                    {
                        var _collection = _db.GetCollection<BsonDocument>(_collectionName);

                        var keyValPair = _collection.FindById(key);
                        if (keyValPair != null)
                        {
                            serializedStr = keyValPair["Value"];
                        }

                    }
                }

                object value = null;
                if (serializedStr != null)
                {
                    value = _serializer.Deserialize(type, serializedStr);
                }

                if (value == null)
                {
                    throw new Exception("Can`t find value.");
                }
                return value;
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
            }
        }

        /// <summary>
        /// Remember, that null value is equals that it not exists. 
        /// So, if before you set 'key_name' value to null, then ContainsKey will return false;
        /// </summary>
        public async Task<bool> ContainsKey(string key)
        {
            try
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
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
            }
        }

        public async Task Clear()
        {
            try
            {
                lock (Locker)
                {
                    using (var _db = new LiteDatabase(_dbFilePath))
                    {
                        _db.DropCollection(_collectionName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate), ex);
            }
        }
    }
}