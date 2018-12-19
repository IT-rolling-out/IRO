using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ItRollingOut.Common.Diagnostics;

namespace ItRollingOut.Storage.JsonFileStorage
{
    /// <summary>
    /// Storage in json file.
    /// </summary>
    public class JsonLocalStorage : IKeyValueStorage
    {
        const int TimeoutSeconds = 30;
        readonly object Locker = new object();
        readonly string _storageFilePath;
        readonly string _syncFilePath;
        long _currentSyncIteration;
        Dictionary<string, object> _storageDict;
        JsonSerializerSettings _serializeOpt;
        static object writingLock = 1;

        /// <summary>
        /// Storage name will be "localstorage"
        /// </summary>
        public JsonLocalStorage() : this("localstorage")
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageName">Storage name</param>
        public JsonLocalStorage(string storageName) : this(storageName, CommonHelpers.GetExecutableAssemblyDir())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageName">Storage name</param>
        /// <param name="path">Path to storage</param>
        public JsonLocalStorage(string storageName, string path)
        {
            _serializeOpt = new JsonSerializerSettings();
            if (Debugger.IsAttached)
            {
                _serializeOpt.Formatting = Formatting.Indented;
            }

            _storageFilePath = Path.Combine(path, storageName + ".json");
            _syncFilePath= Path.Combine(path, storageName + "_sync.txt");
            CommonHelpers.TryCreateFileIfNotExists(_storageFilePath);
            CommonHelpers.TryCreateFileIfNotExists(_syncFilePath);

        }

        /// <summary>
        /// Method is synchronized with Get. If value is 'null', then method will remove that value from the dictionary.
        /// If you're not closing the application, it's not recommended to use await keyword.
        /// </summary>
        public async Task Set(string key, object value)
        {
            await Task.Run(() =>
            {
                string serializedDict;
                lock (Locker)
                {
                    _LoadStorageState();     
                    
                    if (value == null)
                    {
                        _storageDict.Remove(key);
                    }
                    else
                    {
                        _storageDict[key] = value;
                    }

                    serializedDict = JsonConvert.SerializeObject(
                       _storageDict,
                       _serializeOpt
                       );

                    _SaveStorageState(serializedDict);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Method is synchronized with Set. 
        /// If key doesn't exist, then method will return null for reference type or default value for value types.
        /// </summary>
        public async Task<T> Get<T>(string key)
        {
            return await Task.Run(() =>
            {
                return (T)_Get(key, typeof(T));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// If the key doesn't exist, throws a null exception.
        /// If 'key_name' is equals to null, method will return false
        /// </summary>
        public async Task<bool> ContainsKey(string key)
        {
            lock (Locker)
            {
                _LoadStorageState();
                return _storageDict.ContainsKey(key);
            }
        }

        public Task ClearAll()
        {
            lock (Locker)
            {
                _SaveStorageState("{}");
            }
            return Task.FromResult<object>(null);
        }

        object _Get(string key, Type t)
        {
            lock (Locker)
            {
                _LoadStorageState();

                if (!_storageDict.ContainsKey(key))
                {
                    //return default value for structs or null for class
                    if (t.IsValueType)
                        return Activator.CreateInstance(t);
                    else
                        return null;
                }

                string serializedStr = JsonConvert.SerializeObject(_storageDict[key]);
                object res = JsonConvert.DeserializeObject(serializedStr, t);              
                return res;
            }

        }

        void _LoadStorageState()
        {
            long fromFileSyncIteration = _ReadSyncIteration();
            if (_storageDict == null || _currentSyncIteration < fromFileSyncIteration)
            {
                _storageDict = _ReadStorage();
                _currentSyncIteration = fromFileSyncIteration;
            }
        }

        void _SaveStorageState(string storage)
        {
            _WriteStorage(storage);
            _WriteSyncIteration(++_currentSyncIteration);
        }

        Dictionary<string, object> _ReadStorage()
        {
            Dictionary<string, object> res = null;
            try
            {
                CommonHelpers.TryReadAllText(_storageFilePath, out string strFromFile, TimeoutSeconds);
                res = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    strFromFile
                    );
            }
            catch { }
            if (res == null)
                res = new Dictionary<string, object>();
            return res;
        }

        void _WriteStorage(string storage)
        {
            CommonHelpers.TryCreateFileIfNotExists(_storageFilePath);
            CommonHelpers.TryWriteAllText(_storageFilePath, storage, TimeoutSeconds);
        }

        long _ReadSyncIteration()
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

        void _WriteSyncIteration(long newIteration)
        {
            CommonHelpers.TryCreateFileIfNotExists(_syncFilePath);
            CommonHelpers.TryWriteAllText(_syncFilePath, newIteration.ToString(), TimeoutSeconds);
        }
    }
}