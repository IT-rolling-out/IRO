using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IRO.Common.Collections
{
    /// <summary>
    /// Just downgrated ConcurrentDictionary.
    /// HasSet concurrent analog.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashCollection<T>
        : ICollection<T>, IReadOnlyCollection<T>
    {
        ConcurrentDictionary<T, T> _concurrentDict = new ConcurrentDictionary<T, T>();

        public int Count => _concurrentDict.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Return true if removed.
        /// </summary>
        public bool Remove(T item)
        {
           return  _concurrentDict.TryRemove(item, out T value);
        }

        public void Add(T item)
        {
            _concurrentDict[item] = item;
        }

        public bool TryAdd(T item)
        {
            return _concurrentDict.TryAdd(item, item);
        }

        public void Clear()=> _concurrentDict.Clear();

        public bool Contains(T item)=> _concurrentDict.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            //Is someone use this?
            this.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ConcurrentHashCollectionEnumerator(
                _concurrentDict.GetEnumerator()
                );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class ConcurrentHashCollectionEnumerator : IEnumerator<T>
        {
            readonly IEnumerator<KeyValuePair<T, T>> _dictEnumerator;

            public ConcurrentHashCollectionEnumerator(IEnumerator<KeyValuePair<T, T>> dictEnumerator)
            {
                _dictEnumerator = dictEnumerator;
            }

            public bool MoveNext()=>_dictEnumerator.MoveNext();

            public void Reset() => _dictEnumerator.Reset();

            public T Current => _dictEnumerator.Current.Key;

            object IEnumerator.Current => Current;

            public void Dispose() => _dictEnumerator.Dispose();
        }
    }
}
