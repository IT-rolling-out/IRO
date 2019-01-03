using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IRO.Reflection
{
    public static class TypeValuesSelector
    {
        /// <summary>
        /// Находит все значения (свойств или констант), которые являются потомками T.
        /// </summary>
        /// <param name="instance">Если null, то только статические свойства. Иначе - только свойства объекта.</param>
        /// <returns></returns>
        public static List<T> FindAllValuesThatImplementType<T>(Type searchInType, SelectorFlags selectorFlags, object instance = null)
        {
            var t = searchInType;
            var res = new List<T>();
            var searchStatic = instance == null;

            if (selectorFlags.HasFlag(SelectorFlags.Consts))
            {
                var constsInfo = GetConstantsInfo(t, searchStatic);
                foreach (var item in constsInfo)
                {
                    var val = item.GetValue(instance);
                    if (val is T variable)
                        res.Add(variable);
                }
            }

            if (selectorFlags.HasFlag(SelectorFlags.Properties))
            {
                var propsInfo = GetPropertiesInfo(t, searchStatic);

                foreach (var item in propsInfo)
                {
                    var val = item.GetValue(instance);
                    if (val is T variable)
                        res.Add(variable);
                }
            }
            return res;
        }

        static List<FieldInfo> GetConstantsInfo(Type type, bool searchStatic)
        {
            var fieldInfos = type.GetFields(
                GetBindingFlags(searchStatic)
                );

            return fieldInfos.Where(fi => FieldChecker(fi)).ToList();
        }

        static bool FieldChecker(FieldInfo fi)
        {
#if NETSTANDARD2_0
            return fi.IsLiteral && !fi.IsInitOnly;
#endif
            return !fi.IsInitOnly;
        }

        static PropertyInfo[] GetPropertiesInfo(Type type, bool searchStatic)
        {
            var propsInfo = type.GetProperties(GetBindingFlags(searchStatic));
            return propsInfo;
        }

        static BindingFlags GetBindingFlags(bool searchStatic)
        {
            var bindFlag = searchStatic ? BindingFlags.Static : BindingFlags.Instance;
            bindFlag = bindFlag | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            return bindFlag;
        }
    }
}
