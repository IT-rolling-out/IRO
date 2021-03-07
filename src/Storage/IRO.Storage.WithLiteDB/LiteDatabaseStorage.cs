using LiteDB;
using System;
using System.Threading.Tasks;
using IRO.Cache;
using IRO.Common.Text;
using IRO.Storage.Exceptions;
using Newtonsoft.Json.Linq;

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
        readonly object Locker = new object();
        string _collectionName;
        readonly string _dbFilePath;
        readonly RamCache _cache;
        readonly IStringsSerializer _serializer;
        bool _useCache;

        public LiteDatabaseStorage(IStringsSerializer serializer = null, LiteDatabaseStorageInitOptions opt = null)
        {
            opt ??= new LiteDatabaseStorageInitOptions();
            _useCache = opt.UseCache;
            _serializer = serializer ?? new JsonSimpleSerializer();
            _cache = new RamCache(1000);

            _collectionName = opt.CollectionName;
            _dbFilePath = opt.DbFilePath;
            using (var _db = new LiteDatabase(_dbFilePath))
            {
                var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                _collection.EnsureIndex("_id");
            }
        }

        public Task<JToken> Get(string key)
        {
            throw new NotImplementedException();
        }

        public async Task Set(string key, object value)
        {
            Action asyncAct = () =>
              {
                  try
                  {
                      lock (Locker)
                      {
                          if (_useCache)
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

        public Task Remove(string key)
        {
            throw new NotImplementedException();
        }

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