﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Storage.Exceptions;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Will serialize all values, like other storages.
    /// </summary>
    public class RamStorage : IKeyValueStorage
    {
        const string ExceptionMsgTemplate = "Error with '{0}' in ram storage.";
        readonly IStorageSerializer _serializer;
        readonly object _locker = new object();
        readonly IDictionary<string, object> _storageDict = new ConcurrentDictionary<string, object>();

        public RamStorage(IStorageSerializer serializer)
        {
            _serializer = serializer;
        }

        public RamStorage() : this(new JsonStorageSerializer())
        {
        }

        public async Task<object> Get(Type type, string key)
        {
            try
            {
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
                var value = _serializer.Deserialize(type, str);
                return value;
            }
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
            }
        }

        public async Task Set(string key, object value, TimeSpan? lifetime = null)
        {
            try
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
            catch (Exception ex)
            {
                throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
            }
        }

        public async Task<bool> ContainsKey(string key)
        {
            try
            {
                return _storageDict.ContainsKey(key);
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
                _storageDict.Clear();
            }
            catch (Exception ex)
            {
                throw new StorageException("Exception while cleaning.", ex);
            }
        }
    }
}
