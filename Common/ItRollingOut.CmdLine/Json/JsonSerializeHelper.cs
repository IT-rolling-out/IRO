using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.CmdLine.Json
{
    /// <summary>
    /// Из старой библиотеки ItRollingOut.Json, лучше не использовать.
    /// </summary>
    class JsonSerializeHelper
    {
        static JsonSerializeHelper()
        {
            Inst = new JsonSerializeHelper();
        }

        public static JsonSerializeHelper Inst { get;}

        static JsonSerializeOptions defOptions = new JsonSerializeOptions();
        static JsonSerializerSettings settingsIgnoreDefaults = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling= ReferenceLoopHandling.Ignore
        };
        static JsonSerializerSettings settingsIncludeDefaults = new JsonSerializerSettings( );
        static IContractResolver defCintractResolver=new CamelCasePropertyNamesContractResolver();

        public T FromJson<T>(string json)
        {
            return (T)FromJson(typeof(T), json);
        }

        public object FromJson(Type objType,string json)
        {
            return JsonConvert.DeserializeObject(json, objType);
        }

        /// <summary>
        /// Если тип IConvertible, то будет конвертирован через Convert.ToString,
        /// иначе - сериализирован в json.
        /// </summary>
        public string ToConvertibleOrJson(object obj, JsonSerializeOptions jsonSerializeOptions = null)
        {
            Type objType = obj?.GetType();
            if (obj!=null && typeof(IConvertible).IsAssignableFrom(objType))
            {
                return Convert.ToString(obj);
            }
            else
            {
                return ToJson(objType, obj, jsonSerializeOptions);
            }
        }

        public object FromConvertibleOrJson(Type objType,string objStr)
        {
            if (typeof(IConvertible).IsAssignableFrom(objType))
            {
                return Convert.ChangeType(objStr, objType);
            }
            else
            {
                return FromJson(objType, objStr);
            }
        }

        public T FromConvertibleOrJson<T>(string objStr)
        {
            return (T)FromConvertibleOrJson(typeof(T),objStr);
        }


        public string ToJson<T>(T obj, JsonSerializeOptions jsonSerializeOptions = null)
        {
            return ToJson(typeof(T), obj, jsonSerializeOptions);
        }

        public string ToJson(Type objType, object obj, JsonSerializeOptions jsonSerializeOptions = null)
        {
            jsonSerializeOptions = jsonSerializeOptions ?? defOptions;
            JsonSerializerSettings settings = jsonSerializeOptions.IgnoreDefaultValues ? settingsIgnoreDefaults : settingsIncludeDefaults;
            settings.ContractResolver = defCintractResolver;
            return JsonConvert.SerializeObject(
                obj,
                objType,
                jsonSerializeOptions.WithNormalFormating ? Formatting.Indented : new Formatting(),
                settings
                );
        }
    }
}
