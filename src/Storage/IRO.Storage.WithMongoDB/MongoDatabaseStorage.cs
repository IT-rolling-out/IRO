using System;
using System.Threading.Tasks;
using IRO.Cache;
using IRO.Common.Text;
using IRO.Storage.DefaultStorages;
using IRO.Storage.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace IRO.Storage.WithMongoDB
{
    public class MongoDatabaseStorage : BaseStorage
    {
        readonly string _collectionName;
        readonly RamCache _cache;
        readonly bool _useCache;
        readonly IMongoDatabase _db;
        IMongoCollection<BsonDocument> _collection;


        public MongoDatabaseStorage(IMongoDatabase db, MongoDatabaseStorageInitOptions opt = null)
        {
            opt ??= new MongoDatabaseStorageInitOptions();
            _useCache = opt.UseCache;
            _cache = new RamCache(1000);
            _db = db;
            _collectionName = opt.CollectionName;
            _collection = _db.GetCollection<BsonDocument>(_collectionName);
            _collection.EnsureIndex("key").Wait();
        }

        protected override async Task InnerSet(string key, string value)
        {
            if (_useCache)
                await _cache.Set(key, value);
            if (value == null)
            {
                await _collection.DeleteOneAsync(d => d["key"] == key);
            }
            else
            {
                await _collection.UpsertAsync(
                    d => d["key"] == key,
                    new BsonDocument
                    {
                        ["key"] = key,
                        ["Value"] = value
                    }
                );
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
                    return (string)cachedValue;
            }

            var keyValPair = await _collection.FindOneOrDefaultAsync(d => d["key"] == key);
            if (keyValPair != null)
            {
                return keyValPair["Value"]?.AsString;
            }
            return null;

        }

        protected override async Task InnerClear()
        {
            await _db.DropCollectionAsync(_collectionName);
            _cache.Clear();
        }
    }
}