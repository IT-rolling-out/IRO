using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache.Exceptions;
using IRO.Common.Text;


namespace IRO.Cache
{
    /// <summary>
    /// Saves data in temp files. Each record is file.
    /// Useful for files, not for small objects.
    /// <para></para>
    /// Not thread safe.
    /// </summary>
    public class FileSystemCache : IKeyValueCache
    {
        readonly ConcurrentDictionary<string, FileSystemCacheContainer> _cacheDict =
            new ConcurrentDictionary<string, FileSystemCacheContainer>();

        readonly List<string> _keysPool = new List<string>();

        readonly int _recordsLimit;
        readonly string TempDirPath;

        /// <summary>
        /// </summary>
        /// <param name="recordsLimit">Defailt is int.Max.</param>
        public FileSystemCache(int recordsLimit = int.MaxValue)
        {
            TempDirPath = Path.Combine(Path.GetTempPath(), "file_sys_cache");
            if (Directory.Exists(TempDirPath))
            {
                Directory.Delete(TempDirPath, true);
            }
            _recordsLimit = recordsLimit;
            if (recordsLimit < 1)
                throw new ArgumentException("Records limit can`t be smaller than 1.", nameof(recordsLimit));
        }


        public async Task<Stream> GetStream(string key)
        {
            try
            {
                string filePath;
                if (_cacheDict.TryGetValue(key, out var container))
                {
                    filePath = container.FilePath;
                }
                else
                {
                    return null;
                }
                return File.OpenRead(filePath);
            }
            catch (Exception ex)
            {
                throw new CacheException("", ex);
            }
        }

        public async Task SetStream(string key, Stream stream, DateTime? expiresIn = null)
        {
            try
            {
                await Fit();

                if (stream == null)
                {
                    await Remove(key);
                    return;
                }
                var filePath = SaveFileToTempDirectory(stream);
                var container = new FileSystemCacheContainer()
                {
                    ExpiresIn = expiresIn,
                    FilePath = filePath
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

        public async Task<byte[]> GetBytes(string key)
        {
            using (var stream = await GetStream(key))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task SetBytes(string key, byte[] bytes, DateTime? expiresIn = null)
        {
            await SetStream(key, new MemoryStream(bytes), expiresIn);
        }

        public async Task Remove(string key)
        {
            _cacheDict.TryRemove(key, out var val);
            if(File.Exists(val.FilePath))
                File.Delete(val.FilePath);
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

        string SaveFileToTempDirectory(Stream stream)
        {
            if (!Directory.Exists(TempDirPath))
            {
                Directory.CreateDirectory(TempDirPath);
            }
            var resName = TextExtensions.Generate(10);
            var filePath = Path.Combine(TempDirPath, resName);
            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            return filePath;
        }
    }
}
