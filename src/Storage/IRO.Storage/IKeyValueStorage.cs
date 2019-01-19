using System;
using System.Threading.Tasks;

namespace IRO.Storage
{
    public interface IKeyValueStorage
    {
        /// <summary>
        /// Return value if exists or throw exception.
        /// </summary>
        /// <param name="type">Type of returned object.</param>
        Task<object> Get(Type type, string key);

        /// <summary>
        /// If value is 'null', then method will remove that value from the storage.
        /// Method is synchronized with Get.
        /// If you're not closing the application, it's not recommended to use await keyword.
        /// </summary>
        Task Set(string key, object value);

        Task<bool> ContainsKey(string key);

        Task Clear();
    }
}