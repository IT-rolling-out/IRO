using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ItRollingOut.Tools.Collections
{
    public class Pool<T>
    {
        public int MaxLimit { get; private set; }

        ConcurrentBag<T> _objects;
        Func<T> _objectGenerator;
        int autocreated = 0;
        AutoResetEvent are = new AutoResetEvent(true);

        public Pool(Func<T> objectGenerator = null, int maxLimit = 1000)
        {
            MaxLimit = maxLimit;
            if (objectGenerator == null)
            {
                var constructor = typeof(T).GetConstructor(new Type[0]);
                if (constructor == null)
                    throw new Exception("For objects with non-default constructors you must send 'objectGenerator'.");
                _objectGenerator = () =>
                {
                    return (T)constructor.Invoke(new object[0]);
                };
            }
            else
            {
                _objectGenerator = objectGenerator;
            }
            _objects = new ConcurrentBag<T>();

        }


        public T GetObject()
        {
            if (autocreated > MaxLimit)
            {
                are.WaitOne();
            }
            autocreated++;
            T item;
            if (_objects.TryTake(out item))
                return item;

            return _objectGenerator();
        }

        public void PutObject(T item)
        {
            _objects.Add(item);
            autocreated--;
            are.Set();
        }

        public void ClearPool()
        {
            autocreated = 0;
            bool canBeDisposed = typeof(IDisposable).IsAssignableFrom(typeof(T));
            lock (_objects)
            {
                foreach (var item in _objects)
                {
                    if (canBeDisposed)
                    {
                        (item as IDisposable)?.Dispose();
                    }
                }
                while (!_objects.IsEmpty)
                {
                    _objects.TryTake(out T item);
                }
            }
        }
    }
}
