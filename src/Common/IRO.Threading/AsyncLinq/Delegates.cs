using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Threading.AsyncLinq
{
    public delegate Task ForEachAsyncDelegate<T>(T item, int position);

    public delegate Task<R> SelectAsyncDelegate<T, R>(T item, int position);

    public delegate Task<bool> WhereAsyncDelegate<T>(T item, int position);
}
