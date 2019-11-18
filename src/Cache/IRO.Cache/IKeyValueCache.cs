using System;
using System.Threading.Tasks;

namespace IRO.Cache
{
    public interface IKeyValueCache
    {
        /// <summary>
        /// Return value or null.
        /// </summary>
        /// <param name="type">Type of returned object.</param>
        Task<object> GetOrNull(Type type, string key);

        /// <summary>
        /// If value is 'null', then method will remove that value from the cache.
        /// </summary>
        Task Set(string key, object value, DateTime? expiresIn=null);

        /// <summary>
        /// Clears some records if limited.
        /// More often called by cahce service.
        /// </summary>
        void Fit();

    }
}