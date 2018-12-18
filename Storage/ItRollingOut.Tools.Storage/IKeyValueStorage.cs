using System.Threading.Tasks;

namespace ItRollingOut.Tools.Storage
{
    public interface IKeyValueStorage
    {
        Task<T> Get<T>(string key);
        Task Set(string key, object value);
        Task<bool> ContainsKey(string key);
        Task ClearAll();
    }
}