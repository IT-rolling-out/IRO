using System;

namespace IRO.Threading
{
    public interface IThreadSyncInvoker
    {
        void Invoke(Action act);

        void InvokeAsync(Action act);
    }
}