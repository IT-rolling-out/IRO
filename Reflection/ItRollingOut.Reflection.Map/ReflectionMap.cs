using ItRollingOut.Common.Services;
using ItRollingOut.Reflection.Map.Metadata;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ItRollingOut.Reflection.Map
{

    public class ReflectionMap
    {
        /// <summary>
        /// Создает ReflectionMap для типа inspectedType. 
        /// Не забудьте, что свойства и методы должны быть отмечены одним из атрибутов.
        /// <param name="inspectedType">Тип, методы экземпляра которого будут вызваны через ReflectionMap. 
        /// Это может быть как класс так и его интерфейс</param>
        /// <param name="prefixName">Строка, которая будет добавлена вначале всех ключей в словаре методов.</param>
        /// <param name="allowIncludedOnjects">Если истина, то свойства, 
        /// помеченные через IncludedObjReflectionMapAttribute будут исследованы.</param>
        /// </summary>
        public ReflectionMap(Type inspectedType,string splitter= ".",
            bool allowIncludedObjects=false,string prefixName=null,bool underscoreNotation=false)
        {
            _allowIncludedOnjects = allowIncludedObjects;
            _splitter = splitter;
            _underscoreNotationEnabled = underscoreNotation;
            _underscoreNotationEnabled = underscoreNotation;


            InspectedType = inspectedType;
            Func<object, object> first_funcToGetLocalInstanceFromGlobal = (globalInst) =>
            {
                return globalInst;
            };
            string prefix = (prefixName == null) ? "" : (prefixName + splitter);

            //Ищем методы
            InspectMetods(
                prefix, "",
                inspectedType,
                first_funcToGetLocalInstanceFromGlobal
                );

            //Ищем простые свойства
            InspectSimpleProps(
                prefix, "",
                inspectedType,
                first_funcToGetLocalInstanceFromGlobal
                );

            //Ищем свойства-категории
            InspectIncludedObjProps(
                prefix, "",
                inspectedType,
                first_funcToGetLocalInstanceFromGlobal
                );
        }

        #region Private fields
        bool _allowIncludedOnjects;

        string _splitter;

        bool _underscoreNotationEnabled;
        
        Dictionary<string, IReflectionMapMethod> _longNameAndMethod = new Dictionary<string, IReflectionMapMethod>();
        #endregion

        public IReadOnlyDictionary<string, IReflectionMapMethod> LongNameAndMethod =>_longNameAndMethod;

        /// <summary>
        /// Тип по которому был построен ReflectionMap.
        /// </summary>
        public Type InspectedType { get; private set; }

        /// <summary>
        /// Кроме как для удобного просмотра карты всех апи этот метод ни для чего не служит.
        /// </summary>
        public string AsString()
        {
            StringBuilder res = new StringBuilder();
            foreach (var item in _longNameAndMethod)
            {
                string newStr = item.Key;
                var dictVal = item.Value;
                if (_longNameAndMethod.ContainsKey(item.Key))
                {
                    newStr += "(";
                    bool isFirst = true;
                    foreach (var parameter in dictVal.Parameters)
                    {
                        if (!isFirst)
                        {
                            newStr += ", ";

                        }
                        isFirst = false;
                        newStr += parameter.Info.ParameterType.Name + "  "
                            + ToCurrentNotation(parameter.ParamName);
                    }
                    newStr += ")";
                }
                newStr += ";";
                if (item.Value.Description != null)
                {
                    newStr += $"  /*{item.Value.Description}*/";
                }
                res.AppendLine(newStr);
            }
            return res.ToString();
        }

        /// <summary>
        /// Ищем методы.
        /// </summary>
        /// <param name="t">Исследуемый тип, не путать с InspectedType, ведь с погружением в его поля это будет их тип, соответственно.</param>
        /// <param name="funcToGetLocalInstanceFromGlobal">Делегат для получения объекта с типом t из InspectedType.</param>
        void InspectMetods(string prefix, string realNamePrefix, Type t, Func<object, object> funcToGetLocalInstanceFromGlobal)
        {
            var methodInfoList = GetAllMethods(t);

            foreach (var item in methodInfoList)
            {
                var attr = item.GetCustomAttribute(typeof(MethodReflectionMapAttribute)) as MethodReflectionMapAttribute;
                if (attr != null)
                {
                    var newMethod = new ReflectionMapMethod()
                    {
                        DisplayName = prefix + (attr.DisplayName ?? ToCurrentNotation(item.Name)),
                        RealName = realNamePrefix + item.Name,
                        Description = attr.Description,
                        Parameters = ParameterInfoArrayToParamsArray(item.GetParameters()),
                        ReturnType=item.ReturnType
                    };
                    newMethod.InvokeAction = (globalInst, parameters) =>
                    {
                        var locInst = funcToGetLocalInstanceFromGlobal(globalInst);
                        return item.Invoke(locInst, parameters);
                    };

                    _longNameAndMethod.Add(
                        newMethod.DisplayName,
                        newMethod
                        );
                }
            }

        }

        Parameter[] ParameterInfoArrayToParamsArray(ParameterInfo[] arr)
        {
            var resArr = new Parameter[arr.Length];
            for(int i = 0; i < arr.Length; i++)
            {
                resArr[i] = new Parameter()
                {
                    ParamName = arr[i].Name,
                    Info = arr[i]
                };
            }
            return resArr;
        }

        /// <summary>
        /// Default reflection method don`t return methods of interfaces, that type implement. This method can do it.
        /// </summary>
        List<MethodInfo> GetAllMethods(Type t)
        {

            List<MethodInfo> methodInfoList = new List<MethodInfo>();
            methodInfoList.AddRange(t.GetMethods());

            foreach (var interfaceType in t.GetInterfaces())
            {
                methodInfoList.AddRange(interfaceType.GetMethods());
                
            }
            return methodInfoList;
        }

        /// <summary>
        /// Ищем простые свойства (которые будут сконвертированы в методы с приставкой get и set.
        /// </summary>
        /// <param name="t">Исследуемый тип, не путать с InspectedType, ведь с погружением в его поля это будет их тип, соответственно.</param>
        /// <param name="funcToGetLocalInstanceFromGlobal">Делегат для получения объекта с типом t из InspectedType.</param>
        void InspectSimpleProps(string prefix, string realNamePrefix, Type t, Func<object, object> funcToGetLocalInstanceFromGlobal)
        {
            foreach (var item in t.GetProperties())
            {
                var attr = item.GetCustomAttribute(typeof(SimplePropReflectionMapAttribute)) as SimplePropReflectionMapAttribute;
                if (attr != null)
                {
                    if (attr.CanGet && item.CanRead)
                    {
                        var newMethod = new ReflectionMapMethod()
                        {
                            DisplayName = prefix + (NotationGetPrefix() + (attr.DisplayName ?? ToCurrentNotation(item.Name))),
                            RealName = realNamePrefix + item.Name,
                            Description = attr.Description,
                            Parameters = new Parameter[] { },
                            ReturnType = item.PropertyType,
                            Kind = MethodKind.PropertyGetter
                        };

                        var getter = item.GetMethod;
                        newMethod.InvokeAction = (globalInst, parameters) =>
                        {
                            var locInst = funcToGetLocalInstanceFromGlobal(globalInst);
                            return getter.Invoke(locInst, parameters);
                        };

                        _longNameAndMethod.Add(
                            newMethod.DisplayName,
                            newMethod
                            );
                    }

                    if (attr.CanSet && item.CanWrite)
                    {
                        var param = new Parameter
                        {
                            ParamName = "val"
                        };

                        var newMethod = new ReflectionMapMethod()
                        {
                            DisplayName = prefix + (NotationSetPrefix() + (attr.DisplayName ?? ToCurrentNotation(item.Name))),
                            RealName = realNamePrefix + item.Name,
                            Description = attr.Description,
                            Parameters = new Parameter[] { param },
                            ReturnType = typeof(void),
                            Kind = MethodKind.PropertySetter
                        };


                        var setter = item.SetMethod;
                        newMethod.InvokeAction = (globalInst, parameters) =>
                        {
                            var locInst = funcToGetLocalInstanceFromGlobal(globalInst);
                            return setter.Invoke(locInst, parameters);
                        };

                        _longNameAndMethod.Add(
                            newMethod.DisplayName,
                            newMethod
                            );
                    }
                }
            }

        }

        /// <summary>
        /// Ищем свойства-категории. Все методы из типа свойства будут добавленны в reflection map с префиксом в виде имени свойства (учитывая погружение).
        /// </summary>
        /// <param name="t">Исследуемый тип, не путать с InspectedType, ведь с погружением в его поля это будет их тип, соответственно.</param>
        /// <param name="funcToGetLocalInstanceFromGlobal">Делегат для получения объекта с типом t из InspectedType.</param>
        void InspectIncludedObjProps(string prefix, string realNamePrefix, Type t, Func<object, object> funcToGetLocalInstanceFromGlobal)
        {
            if (!_allowIncludedOnjects)
                return;
            foreach (var item in t.GetProperties())
            {
                var attr = item.GetCustomAttribute(typeof(IncludedObjReflectionMapAttribute)) as IncludedObjReflectionMapAttribute;
                if (attr != null)
                {
                    var displayName = prefix + (attr.DisplayName ?? ToCurrentNotation(item.Name));
                    var realName = realNamePrefix + item.Name;
                    //_longNameAndInfo.Add(
                    //    prefix + newInfo.DisplayName,
                    //    newInfo
                    //    );

                    var categoryGetter=item.GetMethod;
                    Func<object, object> new_funcToGetLocalInstanceFromGlobal = (globalInst) =>
                    {
                        var parentInst = funcToGetLocalInstanceFromGlobal(globalInst);
                        var res=categoryGetter.Invoke(parentInst, new object[0]);
                        return res;
                    };

                    var newPrefix =  displayName + _splitter;
                    var newRealNamePrefix = realNamePrefix +realName+".";
                    InspectMetods(
                        newPrefix,
                        newRealNamePrefix,
                        item.PropertyType,
                        new_funcToGetLocalInstanceFromGlobal
                        );

                    InspectSimpleProps(
                        newPrefix,
                        newRealNamePrefix,
                        item.PropertyType,
                        new_funcToGetLocalInstanceFromGlobal
                        );

                    InspectIncludedObjProps(
                        newPrefix,
                        newRealNamePrefix,
                        item.PropertyType,
                        new_funcToGetLocalInstanceFromGlobal
                        );
                }
            }
        }

        string NotationGetPrefix()
        {
            if (_underscoreNotationEnabled)
                return "get_";
            return "Get";
        }

        string NotationSetPrefix()
        {
            if (_underscoreNotationEnabled)
                return "set_";
            return "Set";
        }

        string ToCurrentNotation(string inputStr)
        {
            if (_underscoreNotationEnabled)
                return TextExtensions.ToUnderscoreCase(inputStr);
            return inputStr;
        }
    }
}
