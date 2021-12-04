using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using IRO.Common.Services;
using IRO.Common.Text;
using IRO.Storage.Data;
using IRO.Storage.Exceptions;
using Newtonsoft.Json;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Storage in file.
    /// <para></para>
    /// Use its own unlimited cache to store object, that will be serialized (store all data in memory and sync with hard drive).
    /// </summary>
    public class FileStorage : BaseStorage
    {
        const int TimeoutSeconds = 30;
        readonly string _storageFilePath;
        IDictionary<string, string> _storageDict;

        public FileStorage(FileStorageInitOptions opt = null)
        {
            opt ??= new FileStorageInitOptions();
            _storageFilePath = opt.StorageFilePath;
            if (!File.Exists(_storageFilePath))
            {
                File.CreateText(_storageFilePath).Close();
            }

        }

        protected override async Task InnerSet(string key, string value)
        {
            LoadStorageState();
            _storageDict[key] = value;
            SaveStorageState();
        }

        protected override async Task InnerRemove(string key)
        {
            LoadStorageState();
            _storageDict.Remove(key);
            SaveStorageState();
        }

        protected override async Task<string> InnerGet(string key)
        {
            LoadStorageState();
            if (_storageDict.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        protected override async Task InnerClear()
        {
            _storageDict?.Clear();
            SaveStorageState();
        }


        protected void LoadStorageState()
        {
            _storageDict = ReadStorage();
        }

        protected void SaveStorageState()
        {
            string serializedDict = JsonConvert.SerializeObject(_storageDict);
            WriteStorage(serializedDict);
        }

        Dictionary<string, string> ReadStorage()
        {
            Dictionary<string, string> res = null;
            try
            {
                FileHelpers.TryReadAllText(
                    _storageFilePath,
                    out string strFromFile,
                    TimeoutSeconds
                    );
                res = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    strFromFile
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return res ?? new Dictionary<string, string>();
        }

        void WriteStorage(string storage)
        {
            if (!File.Exists(_storageFilePath))
            {
                File.CreateText(_storageFilePath).Close();
            }
            FileHelpers.TryWriteAllText(_storageFilePath, storage, TimeoutSeconds);
        }
    }
}