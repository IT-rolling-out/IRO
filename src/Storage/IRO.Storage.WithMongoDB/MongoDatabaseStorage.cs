using System;
using System.Threading.Tasks;
using IRO.Common.Text;
using IRO.Storage.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IRO.Storage.WithMongoDB
{
    public class MongoDatabaseStorage : IKeyValueStorage
    {
        const string ExceptionMsgTemplate = "Error with '{0}' in MongoDB storage.";
        readonly object Locker = new object();
        readonly IMongoDatabase _db;
        string _collectionName;
        readonly IStringsSerializer _serializer;
        IMongoCollection<BsonDocument> _collection;

        public MongoDatabaseStorage(IMongoDatabase db, string collectionName = "key_value_storage")
            : this(db, collectionName, null)
        {
        }

        public MongoDatabaseStorage(IMongoDatabase db, string collectionName, IStringsSerializer serializer)
        {
            _serializer = serializer ?? new JsonSimpleSerializer();
            _db = db;
            _collectionName = collectionName;

            _collection = _db.GetCollection<BsonDocument>(_collectionName);
            _collection.EnsureIndex("key").Wait();
        }

        /// <summary>
        /// Automatically synchronized with Get. If 'null' - will remove value.
        /// </summary>
        public async Task Set(string key, object value)
        {
            try
            {
                Task taskToAwait;
                lock (Locker)
                {
                    if (value == null)
                    {
                        taskToAwait = _collection.DeleteOneAsync(d => d["key"] == key);
                    }
                    else
                    {
                        //Use json for more simple convertation.
                        string serializedStr = _serializer.Serialize(value);
                        taskToAwait = _collection.UpsertAsync(
                            d => d["key"] == key,
                            new BsonDocument
                            {
                                ["key"] = key,
                                ["Value"] = serializedStr
                            }
                        );

                    }
                }
                await taskToAwait;
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
            }

        }

        /// <summary>
        /// Automatically synchronized with Set. 
        /// </summary>
        public async Task<object> Get(Type type, string key)
        {
            try
            {
                string serializedStr = null;
                var keyValPair = await _collection.FindOneOrDefaultAsync(d => d["key"] == key);
                if (keyValPair != null)
                {
                    serializedStr = keyValPair["Value"].AsString;
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
                var value = await _collection.FindOneOrDefaultAsync(r => r["key"] == key);
                return value != null;
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
                await _db.DropCollectionAsync(_collectionName);
            }
            catch (Exception ex)
            {
                throw new StorageException("Can't drop collection.", ex);
            }
        }
    }
}