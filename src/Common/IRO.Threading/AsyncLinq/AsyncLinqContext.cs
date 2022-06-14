using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IRO.Threading
{
    public class AsyncLinqContext
    {
        public int MaxThreadsCount { get; private set; }

        internal CancellationToken CancellationToken { get; private set; }

        internal Queue<HashSet<Task>> RunningTasksHashSetsQueue { get; } = new Queue<HashSet<Task>>();

        private AsyncLinqContext() { }

        public static AsyncLinqContext Create(int? maxThreadsCount=null, CancellationToken cancellationToken = default)
        {
            var alc = new AsyncLinqContext();
            if (maxThreadsCount.HasValue)
            {
                if (maxThreadsCount < 1)
                {
                    throw new ArgumentException($"Value is '{maxThreadsCount}'. Must be bigger than 1.", nameof(maxThreadsCount));
                }
                alc.MaxThreadsCount = maxThreadsCount.Value;
            }
            else
            {
                alc.MaxThreadsCount = Environment.ProcessorCount;
            }
            alc.CancellationToken = cancellationToken;
            return alc;
        }
    }
}
