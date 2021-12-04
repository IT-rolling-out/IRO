using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache.Data;
using IRO.Cache.Exceptions;
using IRO.Common.Text;


namespace IRO.Cache
{
    /// <summary>
    /// Saves data in ram as bytes.
    /// </summary>
    public class RamCache : IKeyValueCache
    {
        readonly ConcurrentDictionary<string, RamCacheContainer> _cacheDict =
            new ConcurrentDictionary<string, RamCacheContainer>();

        readonly List<string> _keysPool = new List<string>();

        readonly int _recordsLimit;

        /// <summary>
        /// </summary>
        /// <param name="recordsLimit">Defailt is int.Max.</param>
        public RamCache(int recordsLimit = int.MaxValue)
        {
            _recordsLimit = recordsLimit;
            if (recordsLimit < 1)
                throw new ArgumentException("Records limit can`t be smaller than 1.", nameof(recordsLimit));
        }


        public async Task<Stream> GetStream(string key)
        {
            var bytes = await GetBytes(key);
            var stream = new MemoryStream(bytes);
            return stream;
        }

        public async Task SetStream(string key, Stream stream, DateTime? expiresIn = null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(ms);
                var bytes = ms.ToArray();
                await SetBytes(key, bytes, expiresIn);
            }
        }
        
        public async Task<byte[]> GetBytes(string key)
        {
            try
            {
                if (_cacheDict.TryGetValue(key, out var container))
                {
                    return container.SerializedValue;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new CacheException("", ex);
            }
        }

        public async Task SetBytes(string key, byte[] bytes, DateTime? expiresIn = null)
        {
            try
            {
                await Fit();

                if (bytes == null)
                {
                    await Remove(key);
                    return;
                }
                var container = new RamCacheContainer()
                {
                    SerializedValue = bytes,
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

        public async Task Remove(string key)
        {
            _cacheDict.TryRemove(key, out var val);
        }

        public async Task Clear()
        {
            _cacheDict.Clear();
            _keysPool.Clear();
        }

        public async Task Fit()
        {
            if (_cacheDict.Count < _recordsLimit)
            {
                return;
            }

            while (_keysPool.Count > _recordsLimit)
            {
                var dictKey = _keysPool.First();
                _keysPool.RemoveAt(0);
                try
                {
                    await Remove(dictKey);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

    }
}
