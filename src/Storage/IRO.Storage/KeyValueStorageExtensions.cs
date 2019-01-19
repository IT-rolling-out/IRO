using System;
using System.Threading.Tasks;

namespace IRO.Storage
{
    public static class KeyValueStorageExtensions
    {
        /// <summary>
        /// Return value or null.
        /// </summary>
        public static async Task<T> GetOrNull<T>(this IKeyValueStorage @this, string key)
            where T : class
        {
            try
            {
                return await @this.Get<T>(key);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Return value or default Type instance (null for class and default value for struct).
        /// </summary>
        public static async Task<T> GetOrDefault<T>(this IKeyValueStorage @this, string key)
        {
            try
            {
                return await @this.Get<T>(key);
            }
            catch
            {
                if (typeof(T).IsValueType)
                {
                    return Activator.CreateInstance<T>();
                }
                else
                {
                    object res = null;
                    return (T)res;
                }
            }
        }
    }
}