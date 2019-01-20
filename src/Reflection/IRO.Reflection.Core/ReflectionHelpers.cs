using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRO.Reflection.Core
{

    public static class ReflectionHelpers
    {
        #region Normal type names
        static Dictionary<string, string> _normalTypeNamesCahce = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeSettingsResolver">Вызывается каждый раз для получения имени какого-нибудь типа, 
        /// включая внутренние generic типы.</param>
        /// <returns></returns>
        public static string GetNormalTypeName(this Type t, Action<Type, TypeNamingSettings> typeSettingsResolver)
        {
            var settings = new TypeNamingSettings();
            typeSettingsResolver?.Invoke(t, settings);
            bool withNamespace = settings.WithNamespace;

            var key = t.FullName + "_" + withNamespace.ToString();
            if (_normalTypeNamesCahce.ContainsKey(key))
            {
                return _normalTypeNamesCahce[key];
            }

            var name = withNamespace ? t.Namespace + "." : "";

            if (t.IsGenericType)
            {
                //Remove generic declaration part
                name += t.Name.Remove(t.Name.IndexOf("`", StringComparison.Ordinal)) + "<";
                var genericArgs = t.GetGenericArguments();
                var firstArg = genericArgs[0];
                name += GetNormalTypeName(firstArg, typeSettingsResolver);
                for (var i = 1; i < genericArgs.Length; i++)
                {
                    name += ", " + GetNormalTypeName(genericArgs[i], typeSettingsResolver);
                }

                name += ">";
                return name;
            }

            var res = name + t.Name;
            _normalTypeNamesCahce[key] = res;
            return res;
        }

        /// <summary>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="settings">Используем одни настройки для всех типов.</param>
        /// <returns></returns>
        public static string GetNormalTypeName(this Type t, TypeNamingSettings settings)
        {
            return GetNormalTypeName(
                t,
                (type, s) =>
                {
                    s.WithNamespace = settings.WithNamespace;
                });
        }

        public static string GetNormalTypeName(this Type t, bool withNamespace)
        {
            return GetNormalTypeName(
                t,
                (type, s) =>
                {
                    s.WithNamespace = withNamespace;
                });
        }
        #endregion

        #region Copied from IocGlob.
        public static Type FindAssignable(Type baseType, IEnumerable<string> assembliesNames)
        {
            List<Exception> innerExceptions = null;
            List<Assembly> asms;
            if (assembliesNames == null)
            {
                asms = AppDomain.CurrentDomain.GetAssemblies().ToList();
            }
            else
            {
                asms = new List<Assembly>();
                foreach (var assemblyName in assembliesNames)
                {
                    try
                    {
                        var asm = Assembly.Load(assemblyName);

                        asms.Add(asm);
                    }
                    catch (Exception ex)
                    {
                        if (innerExceptions == null)
                            innerExceptions = new List<Exception>();
                        innerExceptions.Add(ex);
                    }
                }
            }
            return FindAssignable(baseType, asms);
            //if (innerExceptions == null)
            //{
            //    throw new Exception(msg);
            //}
            //throw new AggregateException(msg, innerExceptions);
        }

        /// <summary>
        /// Search assingnable types in app domain  and return first finded type or throw exception.
        /// </summary>
        /// <param name="assemblyesNames">If null, it will search in current app domain assemblies.</param>
        /// <returns></returns>
        public static Type FindAssignable(Type baseType, IEnumerable<Assembly> assemblies)
        {
            var types = assemblies.SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass)
                .ToList();

            types.Remove(baseType);
            if (types.Any())
            {
                return types.First();
            }

            var msg = $"Can`t find type that implement '{baseType.Name}'. It seems you have forgot to include implementing dll to main project.";
            throw new AggregateException(msg);
        }

        /// <summary>
        /// Search assignable types in app domain  and return first finded type or throw exception.
        /// </summary>
        /// <param name="assemblyesNames">If null, it will search in current app domain assemblies.</param>
        /// <returns></returns>
        public static Type FindAssignable<TBase>(IEnumerable<Assembly> assemblies)
        {
            return FindAssignable(typeof(TBase), assemblies);
        }

        public static IEnumerable<Type> FindAllWithAttribute(Type attrType, bool creatableOnly = false)
        {
            var typesWithMyAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = TryGetAttributes(t, attrType, true)
                where attributes != null && attributes.Length > 0
                    && (t.IsClass || t.IsValueType) && (!creatableOnly || !t.IsAbstract)
                select t;
            return typesWithMyAttribute;
        }

        static object[] TryGetAttributes(Type ownerType, Type attrType, bool inherit)
        {
            try
            {
                return ownerType.GetCustomAttributes(attrType, true);
            }
            catch
            {
                return new object[0];
            }
        }
        #endregion

        public static IEnumerable<ParameterInfo> ToParamInfo(this IEnumerable<Parameter> parameters)
        {
            return parameters.Select(x => x.Info);
        }

        public static IEnumerable<Parameter> ToParam(this IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Select(x => new Parameter
            {
                ParamName = x.Name,
                Info = x
            });
        }

        public static object CreateDefaultValue(this Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        public static bool IsNumericType(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// You can create type examle (not default null for classes),
        /// creates instanse. If enum or dict - fill it with one record.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object CreateTypeExample(Type t)
        {
            return CreateTypeExample(t, new HashSet<Type>());
        }

        /// <summary>
        /// </summary>
        /// <param name="difficultConstructorsTypes">Types with parameters in constructor. Used to avoid endless loops.</param>
        static object CreateTypeExample(Type t, HashSet<Type> difficultConstructorsTypes)
        {
            try
            {
                //Create dict.
                if (typeof(IDictionary).IsAssignableFrom(t) && t.IsGenericType)
                {
                    var keyType = t.GetGenericArguments()[0];
                    var valType = t.GetGenericArguments()[1];
                    var resDict = TryCreate(t) as IDictionary
                        ?? CreateDictNonGeneric(keyType, valType);

                    object key=null;
                    if (keyType.IsAssignableFrom(typeof(string)))
                    {
                        key = "key";
                    }
                    else
                    {
                        key = CreateTypeExample(keyType, difficultConstructorsTypes);
                    }
                    resDict.Add(
                        key,
                        CreateTypeExample(valType,difficultConstructorsTypes)
                        );
                    return resDict;
                }
                
                //Create list.
                if (typeof(IEnumerable).IsAssignableFrom(t) && t.IsGenericType)
                {
                    var elType = t.GetGenericArguments()[0];
                    var resList = TryCreate(t) as IList
                        ?? CreateListNonGeneric(elType);
                    resList.Add(
                        CreateTypeExample(elType,difficultConstructorsTypes)
                        );
                    return resList;
                }

                //Example of object is string/
                if (t.IsAssignableFrom(typeof(object)))
                {
                    return "";
                }

                //Constructor without params.
                var obj=TryCreate(t);

                //Constructor with params.
                if (obj == null)
                {
                    difficultConstructorsTypes.Add(t);
                    var constructors = t.GetConstructors();
                    foreach (var ctor in constructors)
                    {
                        try
                        {
                            //Create parameters.
                            var parametersTypes = ctor.GetParameters();
                            var parameters = new object[parametersTypes.Length];
                            for (int i = 0; i < parametersTypes.Length; i++)
                            {
                                var paramType = parametersTypes[i];
                                object paramValue = null;
                                //Ignore constructor loops.
                                if (!difficultConstructorsTypes.Contains(t))
                                {
                                    paramValue = CreateTypeExample(paramType.ParameterType, difficultConstructorsTypes);
                                }

                                parameters[i] = paramValue;
                            }

                            obj=ctor.Invoke(parameters);
                            break;
                        }
                        catch { }
                    }
                }
                return obj;
            }
            catch
            {
                return null;
            }
        }

        public static IList CreateListNonGeneric(Type elType)
        {
#if !NETSTANDARD2_0
            return (IList)new List<object>();
#endif
#if NETSTANDARD2_0
            var mi = typeof(DefaultTypesFactory).GetMethod(
                "CreateList",
                BindingFlags.Static | BindingFlags.Public
                );
            var miGen = mi.MakeGenericMethod(elType);
            return (IList)miGen.Invoke(null, new object[0]);
#endif
        }

        public static IDictionary CreateDictNonGeneric(Type keyType, Type valType)
        {
#if !NETSTANDARD2_0
            return (IDictionary)new Dictionary<object, object>();
#endif
#if NETSTANDARD2_0
            var mi = typeof(DefaultTypesFactory).GetMethod(
                "CreateDict",
                BindingFlags.Static | BindingFlags.Public
                );
            var miGen = mi.MakeGenericMethod(keyType, valType);
            return (IDictionary)miGen.Invoke(null, new object[0]);
#endif
        }

        static object TryCreate(Type t)
        {
            try
            {
                return Activator.CreateInstance(t);
            }
            catch
            {
                return null;
            }
        }                
    }
}
