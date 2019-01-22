using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRO.Reflection.Core.ModelBinders
{

    public class CmdStringToParamsBindings : BaseParamsBindings
    {
        public static CmdStringToParamsBindings Inst { get; } = new CmdStringToParamsBindings();

        /// <summary>
        /// Получает набор параметров из комплексного json объекта.
        /// Т.к. каждый парметр подбирается в соответствии с именем, то этот метод медленнее, но более точный.
        /// </summary>
        /// <param name="ignoreErrors">Если истина, то при ошибках десереализации параметрам будет 
        /// задано значение по-умолчанию в соответствии с их типом.</param>
        public List<object> ResolveFromCmd(string paramsStr, IEnumerable<Parameter> parameters,
            bool ignoreErrors = true, char splitter = '/')
        {
            if (string.IsNullOrWhiteSpace(splitter.ToString()))
            {
                throw new ArgumentException("Splitter in cmd params string cant be null or white space.");
            }
            List<object> res = new List<object>();
            var paramsDict = SplitParams(paramsStr, ignoreErrors, splitter);
            foreach (var parameter in parameters)
            {
                var paramType = parameter.ParamType;
                var paramName = parameter.ParamName;
                object currentValue = null;
                try
                {
                    string currentValueStr = PrepareStringForJsonSerializer(
                        paramsDict[paramName],
                        paramType
                        );
                    //Console.WriteLine("--> " + currentValueStr);
                    currentValue = Deserialize(paramType, currentValueStr);
                }
                catch (Exception ex)
                {
                    currentValue = ThrowOrDefaultValue(ex, ignoreErrors, paramType);
                }

                res.Add(currentValue);
            }
            return res;
        }

        string PrepareStringForJsonSerializer(string valueStr, Type paramType)
        {
            var trimmed = valueStr.Trim();
            if (paramType.IsNumericType())
            {
                return trimmed;
            }
            else if (paramType == typeof(bool))
            {
                return trimmed;
            }
            var firstChar = valueStr.Trim().First();
            var lastChar = valueStr.Trim().Last();
            if ((firstChar == '{' && lastChar == '}') || (firstChar == '[' && lastChar == ']'))
            {
                //If json object. Yeah, it`s crunch to allow user work without "" for strings (cmd limitations).
                return trimmed;
            }
            else if (firstChar == '\"' && lastChar == '\"')
            {
                //If escaped string.
                return trimmed;
            }

            //If string without "".
            return "\"" + valueStr + "\"";
        }

        Dictionary<string, string> SplitParams(string parameters, bool ignoreErrors, char splitter)
        {
            var res = new Dictionary<string, string>();
            //parameters = parameters.Trim();
            //if (parameters[0] == splitter)
            //{

            //}
            //parameters = parameters.Trim().Substring(1);
            var paramAndNameArray = parameters.Split('/');
            foreach (var paramAndName in paramAndNameArray)
            {
                string paramName = null;
                string value = null;
                try
                {
                    int splitIndex = paramAndName.IndexOf(":");
                    paramName = paramAndName.Remove(splitIndex).Trim();
                    value = paramAndName.Substring(splitIndex + 1);
                    if (string.IsNullOrWhiteSpace(paramName) || string.IsNullOrWhiteSpace(value))
                        throw new Exception("Wrong params str.");
                    res.Add(paramName, value);
                }
                catch (Exception ex)
                {
                    ThrowIfAllowed(ex, ignoreErrors, $"Error when split param '{paramName}' and its value '{value}'.");
                }
            }
            return res;
        }

    }
}
