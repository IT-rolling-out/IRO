using System;
using System.Threading.Tasks;

namespace IRO.Storage
{
    public interface IKeyValueStorage
    {
        /// <summary>
        /// Return value if exists or throw exception.
        /// </summary>
        Task<T> Get<T>(string key);

        /// <summary>
        /// If value is 'null', then method will remove that value from the dictionary.
        /// Method is synchronized with Get.
        /// If you're not closing the application, it's not recommended to use await keyword.
        /// </summary>
        /// <param name="lifetime">If null - will not expire ever.</param>
        Task Set(string key, object value, TimeSpan? lifetime=null);

        Task<bool> ContainsKey(string key);

        Task Clear();
    }
}