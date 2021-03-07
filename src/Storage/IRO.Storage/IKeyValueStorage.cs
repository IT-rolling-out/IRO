using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IRO.Storage
{
    /// <summary>
    /// Now can use scopes (using dot character).
    /// All operations works for scopes just like for objects, but you can access scope inner properties too.
    /// Example:
    /// <para></para>
    /// Set("Scope.FirstValue", 1);<para></para>
    /// Set("Scope.FirstValue", 2);<para></para>
    /// Get&lt;MyScopeClass&gt;("Scope");<para></para>
    /// </summary>
    public interface IKeyValueStorage
    {
        /// <summary>
        /// Return object casted to type.
        /// <para></para>
        /// If key was not defined or was removed - throw exception.
        /// <para></para>
        /// If object was set to null - return null.
        /// </summary>
        Task<object> Get(Type type, string key);

        /// <summary>
        /// Return JToken of object or scope.
        /// <para></para>
        /// If key was not defined or was removed - throw exception.
        /// <para></para>
        /// If object was set to null - return null.
        /// </summary>
        Task<JToken> Get(string key);

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">Can be null.</param>
        /// <returns></returns>
        Task Set(string key, object value);

        Task Remove(string key);

        /// <summary>
        /// True even if value set to null. False if not exists or removed.
        /// </summary>
        Task<bool> ContainsKey(string key);

        /// <summary>
        /// Delete all storage data.
        /// </summary>
        Task Clear();
    }
}