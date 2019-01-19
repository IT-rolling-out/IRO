using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Will serialize all values, like other storages.
    /// </summary>
    public class RamStorage:IKeyValueStorage
    {
        readonly IStorageSerializer _serializer;
        readonly object _locker = new object();
        readonly Dictionary<string, object> _storageDict = new Dictionary<string, object>();

        public RamStorage(IStorageSerializer serializer)
        {
            _serializer = serializer;
        }

        public RamStorage():this(new JsonStorageSerializer())
        {
        }

        public async Task<T> Get<T>(string key)
        {
            try
            { 
                lock (_locker)
                {
                    var t = typeof(T);
                    if (!_storageDict.ContainsKey(key))
                    {
                        //return default value for structs or null for class
                        throw new KeyNotFoundException();
                    }

                    var origValue = _storageDict[key];
                    if (origValue == null)
                    {
                        _storageDict.Remove(key);
                        throw new Exception();
                    }
                    var str = _serializer.Serialize(origValue);
                    var value = _serializer.Deserialize(t, str);
                    return (T)value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Can`t resolve value by key '{key}' in ram storage.", ex);
            }
        }

        public async Task Set(string key, object value, TimeSpan? lifetime = null)
        {
            lock (_locker)
            {
                if (value == null)
                {
                    _storageDict.Remove(key);
                }
                else
                {
                    _storageDict[key] = value;
                }
            }
        }

        public async Task<bool> ContainsKey(string key)
        {
            return _storageDict.ContainsKey(key);
        }

        public async Task Clear()
        {
            _storageDict.Clear();
        }
    }
}
