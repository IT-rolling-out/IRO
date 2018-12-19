using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ItRollingOut.Reflection.ModelBinders
{
    public class JsonToParamsBindings:BaseParamsBindings
    {
        public static JsonToParamsBindings Inst { get; } = new JsonToParamsBindings();        

        /// <summary>
        /// Получает набор параметров из json массива. При этом соответствие устанавливается по порядковому номеру.
        /// </summary>
        /// <param name="ignoreErrors">Если истина, то при ошибках десереализации параметрам будет 
        /// задано значение по-умолчанию в соответствии с их типом.</param>
        public List<object> ResolveFromArray(string jsonArrayStr, IEnumerable<Type> paramTypes,
            bool ignoreErrors = true, JsonSerializer jsonSerializer = null)
        {
            List<object> res = new List<object>();
            var jToken = GetJToken(jsonArrayStr, jsonSerializer);

            int i = 0;
            foreach (var paramType in paramTypes)
            {
                object currentValue = null;
                try
                {
                    currentValue = jToken[i].ToObject(paramType);
                }
                catch (Exception ex)
                {
                    currentValue = ThrowOrDefaultValue(ex, ignoreErrors, paramType);
                }
                res.Add(currentValue);
                i++;
            }
            return res;
        }

        /// <summary>
        /// Получает набор параметров из json массива. При этом соответствие устанавливается по порядковому номеру.
        /// </summary>
        /// <param name="ignoreErrors">Если истина, то при ошибках десереализации параметрам будет 
        /// задано значение по-умолчанию в соответствии с их типом.</param>
        public List<object> ResolveFromArray(string jsonArrayStr, IEnumerable<Parameter> parameters,
            bool ignoreErrors = true, JsonSerializer jsonSerializer = null)
        {
            var paramTypes = new List<Type>();
            foreach(var parameter in parameters)
            {
                paramTypes.Add(parameter.Info.ParameterType);
            }
            return ResolveFromArray(jsonArrayStr,paramTypes, ignoreErrors, jsonSerializer);
        }

        /// <summary>
        /// Получает набор параметров из комплексного json объекта.
        /// Т.к. каждый парметр подбирается в соответствии с именем, то этот метод медленнее, но более точный.
        /// </summary>
        /// <param name="ignoreErrors">Если истина, то при ошибках десереализации параметрам будет 
        /// задано значение по-умолчанию в соответствии с их типом.</param>
        public List<object> ResolveFromComplexObj(string jsonComplexObjStr, IEnumerable<Parameter> parameters,
            bool ignoreErrors = true, JsonSerializer jsonSerializer = null)
        {
            List<object> res = new List<object>();
            var jToken = GetJToken(jsonComplexObjStr, jsonSerializer);
            int i = 0;
            foreach (var parameter in parameters)
            {
                var paramType = parameter.Info.ParameterType;
                object currentValue = null;
                try
                {
                    currentValue = jToken[parameter.ParamName].ToObject(paramType);
                }
                catch(Exception ex)
                {
                    currentValue = ThrowOrDefaultValue(ex, ignoreErrors, paramType);
                }
                res.Add(currentValue);
                i++;
            }
            return res;
        }

        
    }
}
