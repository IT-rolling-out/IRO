using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRO.Common.Collections
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable), "You can`t create hash set from null.");
            var res = new HashSet<T>();
            foreach(var item in enumerable)
            {
                res.Add(item);
            }
            return res;
        }

        public static Dictionary<TKey, TValue> PairToDictionary<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> enumerable)
        {
            var res = new Dictionary<TKey, TValue>();
            FillDictWithEnumerable(enumerable, res);
            return res;
        }

        public static Dictionary<TKey, TValue> PairToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            var res = new Dictionary<TKey, TValue>();
            FillDictWithEnumerable(enumerable, res);
            return res;
        }

        /// <summary>
        /// Fill dictionary from enumerable with pair element.
        /// </summary>
        public static void FillDictWithEnumerable<TKey, TValue>(
            IEnumerable<Tuple<TKey, TValue>> enumerable, 
            IDictionary<TKey, TValue> dictToFill
            )
        {
            foreach (var item in enumerable)
            {
                dictToFill.Add(item.Item1, item.Item2);
            }
        }

        /// <summary>
        /// Fill dictionary from enumerable with pair element.
        /// </summary>
        public static void FillDictWithEnumerable<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> enumerable, 
            IDictionary<TKey, TValue> dictToFill
            )
        {
            foreach (var item in enumerable)
            {
                dictToFill.Add(item.Key, item.Value);
            }
        }
    }
}
