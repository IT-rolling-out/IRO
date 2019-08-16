using System;

namespace IRO.Common.Abstractions
{
    public interface IInformativeDisposable:IDisposable
    {
        bool IsDisposed { get; }
    }
}
