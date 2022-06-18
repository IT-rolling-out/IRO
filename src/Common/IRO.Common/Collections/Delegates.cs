using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.Common.Collections
{
    public delegate void ForEachDelegate<T>(T item);
    public delegate void ForEachWithPosDelegate<T>(T item, int position);
}
