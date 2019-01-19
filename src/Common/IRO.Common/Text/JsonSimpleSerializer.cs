using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace IRO.Common.Text
{
    public class JsonSimpleSerializer : IStringsSerializer
    {
        readonly JsonSerializerSettings _serializerSettings;

        public JsonSimpleSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public JsonSimpleSerializer()
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
