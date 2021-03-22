using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache.Exceptions;

namespace IRO.Cache
{
    public static class CacheExtensions
    {
        public static async Task<string> GetString(this IKeyValueCache cache, string key)
        {
            var bytes = await cache.GetBytes(key);
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        public static async Task<byte[]> TryGetBytes(this IKeyValueCache cache, string key)
        {
            try
            {
                var bytes = await cache.GetBytes(key);
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> TryGetString(this IKeyValueCache cache, string key)
        {
            try
            {
                var str = await cache.GetString(key);
                return str;
            }
            catch
            {
                return null;
            }
        }

        public static async Task SetString(this IKeyValueCache cache, string key, string str, DateTime? expiresIn = null)
        {
            if (str == null)
            {
                await cache.Remove(key);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                await cache.SetBytes(key, bytes, expiresIn);
            }

        }

    }
}
