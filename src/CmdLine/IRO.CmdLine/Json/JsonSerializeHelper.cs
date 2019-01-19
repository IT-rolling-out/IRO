using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.CmdLine.Json
{
    /// <summary>
    /// Из старой библиотеки IRO.Json, лучше не использовать.
    /// </summary>
    class JsonSerializeHelper
    {
        static JsonSerializeHelper()
        {
            Inst = new JsonSerializeHelper();
        }

        public static JsonSerializeHelper Inst { get;}

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
        public string ToConvertibleOrJson(object obj)
        {
            Type objType = obj?.GetType();
            if (obj!=null && typeof(IConvertible).IsAssignableFrom(objType))
            {
                return Convert.ToString(obj);
            }
            else
            {
                return ToJson(objType, obj);
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


        public string ToJson<T>(T obj)
        {
            return ToJson(typeof(T), obj);
        }

        public string ToJson(Type objType, object obj)
        {
            var settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver= new CamelCasePropertyNamesContractResolver(),
                Formatting=Formatting.Indented
            };
            return JsonConvert.SerializeObject(
                obj,
                objType,
                settings
                );
        }
    }
}
