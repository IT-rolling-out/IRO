using System;

namespace IRO.Threading
{
    public class ThreadSyncInvoker:IThreadSyncInvoker
    {
        readonly Action<Action> _syncInvoker;
        readonly Action<Action> _asyncInvoker;

        public ThreadSyncInvoker(Action<Action> syncInvoker, Action<Action> asyncInvoker)
        {
            _syncInvoker = syncInvoker ?? throw new ArgumentNullException(nameof(syncInvoker));
            _asyncInvoker = asyncInvoker ?? throw new ArgumentNullException(nameof(asyncInvoker));
        }

        public void Invoke(Action act)
        {
            _syncInvoker.Invoke(act);
        }

        public void InvokeAsync(Action act)
        {
            _asyncInvoker.Invoke(act);
        }
    }
}