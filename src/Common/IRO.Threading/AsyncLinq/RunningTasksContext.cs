using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRO.Threading.AsyncLinq
{
    internal class RunningTasksContext
    {
        private readonly AsyncLinqContext _asyncLinqContext;
        private readonly HashSet<Task> _localTasksHashSet = new HashSet<Task>();

        public RunningTasksContext(AsyncLinqContext asyncLinqContext)
        {
            this._asyncLinqContext = asyncLinqContext;
        }

        public int GetGlobalCount()
        {
            return _asyncLinqContext.RunningTasksCount;
        }

        public void Remove(Task t)
        {
            lock (_localTasksHashSet)
            {
                _localTasksHashSet.Remove(t);
                lock (_asyncLinqContext)
                {
                    _asyncLinqContext.RunningTasksCount--;
                }
            }
        }

        public void Add(Task t)
        {
            lock (_localTasksHashSet)
            {
                _localTasksHashSet.Add(t);
                lock (_asyncLinqContext)
                {
                    _asyncLinqContext.RunningTasksCount++;
                }
            }
        }

        public Task FirstOrDefault()
        {
            lock (_localTasksHashSet)
            {
                return _localTasksHashSet.FirstOrDefault();
            }
        }
    }
}
