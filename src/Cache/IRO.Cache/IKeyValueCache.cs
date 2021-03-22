using System;
using System.IO;
using System.Threading.Tasks;

namespace IRO.Cache
{
    public interface IKeyValueCache
    {
        Task<Stream> GetStream(string key);

        Task SetStream(string key, Stream stream, DateTime? expiresIn = null);

        Task<byte[]> GetBytes(string key);

        Task SetBytes(string key, byte[] bytes, DateTime? expiresIn = null);

        Task Remove(string key);

        Task Clear();

        Task Fit();

    }
}