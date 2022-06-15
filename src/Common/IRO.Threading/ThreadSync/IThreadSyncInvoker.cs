using System;

namespace IRO.Threading.ThreadSync
{
    public interface IThreadSyncInvoker
    {
        void Invoke(Action act);

        void InvokeAsync(Action act);
    }
}
