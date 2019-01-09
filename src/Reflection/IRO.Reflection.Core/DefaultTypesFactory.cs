using System.Collections.Generic;

namespace IRO.Reflection.Core
{
    internal class DefaultTypesFactory
    {
        public static Dictionary<TKey, TValue> CreateDict<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>();
        }

        public static List<T> CreateList<T>()
        {
            return new List<T>();
        }
    }
}
