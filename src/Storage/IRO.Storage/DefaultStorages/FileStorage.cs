﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using IRO.Common.Services;
using IRO.Storage.Exceptions;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Storage in file.
    /// </summary>
    public class FileStorage : IKeyValueStorage
    {
        const string ExceptionMsgTemplate = "Error with '{0}' in file storage.";
        const int TimeoutSeconds = 30;
        readonly object _locker = new object();
        readonly string _storageFilePath;
        readonly string _syncFilePath;
        readonly IStorageSerializer _serializer;

        long _currentSyncIteration;
        Dictionary<string, object> _storageDict;


        /// <summary>
        /// Storage name will be "localstorage".
        /// </summary>
        public FileStorage() : this("local_storage")
        {

        }

        /// <summary>
        /// </summary>
        /// <param name="storageName">Storage name</param>
        public FileStorage(string storageName)
            : this(storageName, AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        public FileStorage(string storageName, string path)
            : this(storageName, path, new JsonStorageSerializer())
        {

        }

        public FileStorage(string storageName, string path, IStorageSerializer serializer)
        {
            _serializer = serializer;
            _storageFilePath = Path.Combine(path, storageName);
            _syncFilePath = Path.Combine(path, storageName + "_sync.txt");
            CommonHelpers.TryCreateFileIfNotExists(_storageFilePath);
            CommonHelpers.TryCreateFileIfNotExists(_syncFilePath);

        }

        /// <summary>
        /// Method is synchronized with Get.
        /// If you're not closing the application, it's not recommended to use await keyword.
        /// </summary>
        /// <param name="lifetime">Ignored.</param>
        /// <returns></returns>
        public async Task Set(string key, object value, TimeSpan? lifetime = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        LoadStorageState();

                        if (value == null)
                        {
                            _storageDict.Remove(key);
                        }
                        else
                        {
                            _storageDict[key] = value;
                        }

                        string serializedDict = _serializer.Serialize(
                            _storageDict
                        );

                        SaveStorageState(serializedDict);
                    }
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Method is synchronized with Set. 
        /// If key doesn't exist, then method will return null for reference type or default value for value types.
        /// </summary>
        public async Task<object> Get(Type type, string key)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return PrivateGet(type, key);
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
                }
            }).ConfigureAwait(false);
        }

        public async Task<bool> ContainsKey(string key)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        LoadStorageState();
                        return _storageDict.ContainsKey(key);
                    }
                }
                catch (Exception ex)
                {
                    throw new StorageException(string.Format(ExceptionMsgTemplate, key), ex);
                }
            }).ConfigureAwait(false);
        }

        public Task Clear()
        {
            lock (_locker)
            {
                try
                {
                    _storageDict?.Clear();
                    SaveStorageState("{}");
                }
                catch (Exception ex)
                {
                    throw new StorageException("Cleaning exception.", ex);
                }
            }
            return Task.FromResult<object>(null);
        }

        object PrivateGet(Type type, string key)
        {
            lock (_locker)
            {
                LoadStorageState();

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
        }

        void LoadStorageState()
        {
            long fromFileSyncIteration = ReadSyncIteration();
            if (_storageDict == null || _currentSyncIteration < fromFileSyncIteration)
            {
                _storageDict = ReadStorage();
                _currentSyncIteration = fromFileSyncIteration;
            }
        }

        void SaveStorageState(string storage)
        {
            WriteStorage(storage);
            WriteSyncIteration(++_currentSyncIteration);
        }

        Dictionary<string, object> ReadStorage()
        {
            Dictionary<string, object> res = null;
            try
            {
                CommonHelpers.TryReadAllText(_storageFilePath, out string strFromFile, TimeoutSeconds);
                res = (Dictionary<string, object>)_serializer.Deserialize(
                    typeof(Dictionary<string, object>),
                    strFromFile
                    );
            }
            catch { }
            if (res == null)
                res = new Dictionary<string, object>();
            return res;
        }

        void WriteStorage(string storage)
        {
            CommonHelpers.TryCreateFileIfNotExists(_storageFilePath);
            CommonHelpers.TryWriteAllText(_storageFilePath, storage, TimeoutSeconds);
        }

        long ReadSyncIteration()
        {
            try
            {
                bool success = CommonHelpers.TryReadAllText(_syncFilePath, out string str, TimeoutSeconds);
                long res = success ? Convert.ToInt64(str) : 0;
                return res;
            }
            catch
            {
                return 0;
            }

        }

        void WriteSyncIteration(long newIteration)
        {
            CommonHelpers.TryCreateFileIfNotExists(_syncFilePath);
            CommonHelpers.TryWriteAllText(_syncFilePath, newIteration.ToString(), TimeoutSeconds);
        }

        void ThrowWrappedError(Exception ex, string key)
        {

        }
    }
}