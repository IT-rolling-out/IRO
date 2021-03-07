using LiteDB;
using System;
using System.Threading.Tasks;
using IRO.Cache;
using IRO.Common.Text;
using IRO.Storage.DefaultStorages;
using IRO.Storage.Exceptions;
using NeoSmart.AsyncLock;
using Newtonsoft.Json.Linq;

namespace IRO.Storage.WithLiteDB
{
    /// <summary>
    /// Storage in litedb file.
    /// Recommended to use it for storages upper than 1000 records.
    /// For another use Storage.FileStorage.
    /// </summary>
    public class LiteDatabaseStorage : BaseStorage
    {
        readonly string _collectionName;
        readonly string _dbFilePath;
        readonly RamCache _cache;
        readonly bool _useCache;

        public LiteDatabaseStorage(LiteDatabaseStorageInitOptions opt = null)
        {
            opt ??= new LiteDatabaseStorageInitOptions();
            _useCache = opt.UseCache;
            _cache = new RamCache(1000);

            _collectionName = opt.CollectionName;
            _dbFilePath = opt.DbFilePath;
            using (var _db = new LiteDatabase(_dbFilePath))
            {
                var _collection = _db.GetCollection<BsonDocument>(_collectionName);
                _collection.EnsureIndex("_id");
            }
        }

        protected override async Task InnerSet(string key, string value)
        {
            if (_useCache)
                await _cache.Set(key, value);
            if (value == null)
            {
                using (var db = new LiteDatabase(_dbFilePath))
                {
                    var collection = db.GetCollection<BsonDocument>(_collectionName);
                    collection.Delete(key);
                }
            }
            else
            {
                using (var db = new LiteDatabase(_dbFilePath))
                {
                    var collection = db.GetCollection<BsonDocument>(_collectionName);
                    collection.Upsert(
                        new BsonDocument
                        {
                            ["_id"] = key,
                            ["Value"] = value
                        }
                    );
                }
            }
        }

        protected override async Task InnerRemove(string key)
        {
            await InnerSet(key, null);
        }

        protected override async Task<string> InnerGet(string key)
        {
            if (_useCache)
            {
                var cachedValue = await _cache.GetOrNull(typeof(string), key);
                if (cachedValue != null)
                    return (string) cachedValue;
            }

            using (var db = new LiteDatabase(_dbFilePath))
            {
                var collection = db.GetCollection<BsonDocument>(_collectionName);
                var keyValPair = collection.FindById(key);
                if (keyValPair == null)
                    return null;
                return keyValPair["Value"];
            }
        }

        protected override async Task InnerClear()
        {
            using (var _db = new LiteDatabase(_dbFilePath))
            {
                _db.DropCollection(_collectionName);
            }
            _cache.Clear();
        }
    }
}