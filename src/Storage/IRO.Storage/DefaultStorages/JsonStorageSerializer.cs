using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace IRO.Storage.DefaultStorages
{
    public class JsonStorageSerializer : IStorageSerializer
    {
        readonly JsonSerializerSettings _serializerSettings;

        public JsonStorageSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public JsonStorageSerializer()
        {
            _serializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling=NullValueHandling.Ignore
            };
            if (Debugger.IsAttached)
            {
                _serializerSettings.Formatting = Formatting.Indented;
            }
        }

        public string Serialize(object val)
        {
            return JsonConvert.SerializeObject(val, _serializerSettings);
        }

        public object Deserialize(Type type, string val)
        {
            return JsonConvert.DeserializeObject(val, type, _serializerSettings);
        }
    }
}
