using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ItRollingOut.Common.Services
{
    public class ServiceLocator<TKey, TResolvedBase>
    {
        public IDictionary<TKey, Func<TResolvedBase>> Factories { get; }
            = new ConcurrentDictionary<TKey, Func<TResolvedBase>>();

        public void RegisterService(TKey key, Func<TResolvedBase> factory)
        {
            Factories.Add(key, factory);
        }

        public TResolvedBase ResolveService(TKey key)
        {
            return Factories[key]();
        }
    }
}
