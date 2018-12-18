using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ItRollingOut.Tools.Reflection.ModelBinders
{
    public class BaseParamsBindings
    {
        protected readonly JsonSerializer JsonSerializer;

        public BaseParamsBindings()
        {
            JsonSerializer = JsonSerializer.Create();
        }

        protected object ThrowOrDefaultValue(Exception ex, bool ignoreExceptions, Type paramType)
        {
            ThrowIfAllowed(ex, ignoreExceptions);
            return paramType.CreateDefaultValue();
        }

        protected void ThrowIfAllowed(Exception ex, bool ignoreExceptions, string message = "")
        {
            if (ignoreExceptions)
            {
            }
            else
            {
                throw new ReflectionModelBinderException(message, ex);
            }
        }

        protected JToken GetJToken(string json, JsonSerializer jsonSerializer = null)
        {
            return Deserialize<JToken>(json, jsonSerializer);
        }

        protected T Deserialize<T>(string json, JsonSerializer jsonSerializer = null)
        {
            return (T)Deserialize(typeof(T), json, jsonSerializer);
        }

        protected object Deserialize(Type t, string json, JsonSerializer jsonSerializer = null)
        {
            if (jsonSerializer == null)
            {
                jsonSerializer = JsonSerializer;
            }
            object res = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(json)), t);
            return res;
        }

    }
}
