using System;
using System.Threading.Tasks;
using IRO.Storage.Exceptions;

namespace IRO.Storage
{
    public static class KeyValueStorageExtensions
    {
        public static async Task<T> Get<T>(this IKeyValueStorage @this, string key)
        {
            object value= await @this.Get(typeof(T), key);
            if (value is T)
            {
                return (T)value;
            }
            else
            {
                throw new StorageException($"Can`t cast returned value '{value}' to type {typeof(T).Name}.");
            }
        }

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