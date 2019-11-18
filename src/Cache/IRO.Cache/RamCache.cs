using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache.Exceptions;
using IRO.Common.Text;


namespace IRO.Cache
{
    /// <summary>
    /// Saves data in ram as strings.
    /// </summary>
    public class RamCache : IKeyValueCache
    {
        readonly IStringsSerializer _serializer;
        readonly ConcurrentDictionary<string, CacheContainer> _cacheDict =
            new ConcurrentDictionary<string, CacheContainer>();

        readonly object _keysPoolLock = new object();
        readonly List<string> _keysPool = new List<string>();

        readonly int _recordsLimit;

        /// <summary>
        /// </summary>
        /// <param name="recordsLimit">Defailt is int.Max.</param>
        public RamCache(int recordsLimit = int.MaxValue, IStringsSerializer serializer=null)
        {
            _serializer = serializer ?? new JsonSimpleSerializer();
            _recordsLimit = recordsLimit;
            if (recordsLimit < 1)
                throw new ArgumentException("Records limit can`t be smaller than 1.", nameof(recordsLimit));
        }

        /// <summary>
        /// </summary>
        /// <param name="expiresIn">Ignored.</param>
        /// <returns></returns>
        public async Task Set(string key, object value, DateTime? expiresIn = null)
        {
            SetSync(key, value, expiresIn);
        }

        public async Task<object> GetOrNull(Type type, string key)
        {
            return TryGetSync(type, key);
        }

        public void SetSync(string key, object value, DateTime? expiresIn = null)
        {
            try
            {
                Fit();

                if (value == null)
                {
                    _cacheDict.TryRemove(key, out var val);
                    return;
                }

                var serializedValue = _serializer.Serialize(value);
                var container = new CacheContainer()
                {
                    SerializedValue = serializedValue,
                    ExpiresIn = expiresIn
                };
                _cacheDict[key] = container;

                //Add to pool.
                _keysPool.Add(key);
            }
            catch (Exception ex)
            {
                throw new CacheException("", ex);
            }
        }

        public object TryGetSync(Type type, string key)
        {
            try
            {
                if (_cacheDict.TryGetValue(key, out var container))
                {
                    var value = _serializer.Deserialize(type, container.SerializedValue);
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new CacheException("", ex);
            }
        }

        public void Fit()
        {
            if (_cacheDict.Count < _recordsLimit)
            {
                return;
            }

            //Use half limit to disable Fit lock on each iteration.
            //var halfOfLimit = _recordsLimit / 2;
            //if (halfOfLimit < 1)
            //    halfOfLimit = 1;
            //else if (halfOfLimit > 100)
            //    halfOfLimit = 100;

            //!Not thread safe, but it is jsut simplest cache, come on.
            while (_keysPool.Count > _recordsLimit)
            {
                var dictKey = _keysPool.First();
                _keysPool.Remove(dictKey);
                _cacheDict.TryRemove(dictKey, out var value);
            }
        }

    }
}
