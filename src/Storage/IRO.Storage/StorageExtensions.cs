﻿using System;
using System.Threading.Tasks;
using IRO.Storage.Exceptions;

namespace IRO.Storage
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Just call of Set(key, null).
        /// </summary>
        public static async Task Remove(this IKeyValueStorage @this, string key)
        {
            await @this.Set(key, null);
        }

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