using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Common.Text;
using IRO.Storage.Exceptions;
using Newtonsoft.Json.Linq;

namespace IRO.Storage.DefaultStorages
{
    /// <summary>
    /// Will serialize all values, like other storages and save it in ram.
    /// Jsut like unlimited cache.
    /// </summary>
    public class RamStorage : BaseStorage
    {
        readonly IDictionary<string, string> _storageDict = new Dictionary<string, string>();

        protected override async Task InnerSet(string key, string value)
        {
            _storageDict[key] = value;
        }

        protected override async Task InnerRemove(string key)
        {
            _storageDict.Remove(key);
        }

        protected override async Task<string> InnerGet(string key)
        {
            if (_storageDict.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        protected override async Task InnerClear()
        {
            _storageDict.Clear();
        }
    }
}
