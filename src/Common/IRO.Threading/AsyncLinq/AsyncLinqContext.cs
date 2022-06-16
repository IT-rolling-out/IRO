using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;

namespace IRO.Threading.AsyncLinq
{
    public class AsyncLinqContext
    {
        public TaskPool TaskPool { get; private set; }

        internal CancellationToken CancellationToken { get; private set; }

        private AsyncLinqContext() { }

        public static AsyncLinqContext Create(int maxThreadsCount, CancellationToken cancellationToken = default)
        {
            return Create(new TaskPool(maxThreadsCount), cancellationToken);
        }

        public static AsyncLinqContext Create(TaskPool taskPool = null, CancellationToken cancellationToken = default)
        {
            var alc = new AsyncLinqContext();
            alc.TaskPool = taskPool ?? new TaskPool();
            alc.CancellationToken = cancellationToken;
            return alc;
        }
    }
}
