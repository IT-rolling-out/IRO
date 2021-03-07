using System;
using System.Threading.Tasks;
using IRO.Storage.Exceptions;

namespace IRO.Storage
{
    public static class StorageExtensions
    {
        public static async Task<T> Get<T>(this IKeyValueStorage @this, string key)
        {
            object value=null;
            try
            {
                value = await @this.Get(typeof(T), key);
                return (T)value;
            }
            catch (Exception ex)
            {
                throw new StorageException($"Can`t cast returned value '{value}' to type {typeof(T).Name}.", ex);
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
                return default(T);
            }
        }
    }
}