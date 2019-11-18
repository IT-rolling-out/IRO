using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IRO.Cache.Exceptions;

namespace IRO.Cache
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Just call of Set(key, null).
        /// </summary>
        public static async Task Remove(this IKeyValueCache @this, string key)
        {
            await @this.Set(key, null);
        }

        /// <summary>
        /// Return value or null.
        /// </summary>
        public static async Task<T> GetOrNull<T>(this IKeyValueCache @this, string key)
            where T : class
        {
            object value = await @this.GetOrNull(typeof(T), key);
            if (value is T valueConverted)
            {
                return valueConverted;
            }
            return null;
        }

        /// <summary>
        /// Return value or default.
        /// Use nullable for value types.
        /// </summary>
        public static async Task<T> GetOrDefault<T>(this IKeyValueCache @this, string key)
        {
            object value = await @this.GetOrNull(typeof(T), key);
            if (value is T valueConverted)
            {
                return valueConverted;
            }
            return default(T);
        }

    }
}
