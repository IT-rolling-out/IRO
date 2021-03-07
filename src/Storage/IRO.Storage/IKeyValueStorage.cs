using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace IRO.Storage
{
    public interface IKeyValueStorage
    {
        Task<object> Get(Type type, string key);

        Task<JToken> Get(string key);

        Task Set(string key, object value);

        Task Remove(string key);

        Task<bool> ContainsKey(string key);

        Task Clear();
    }
}