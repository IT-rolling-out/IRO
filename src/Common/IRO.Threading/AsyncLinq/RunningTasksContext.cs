using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRO.Threading.AsyncLinq
{
    internal class RunningTasksContext
    {
        public HashSet<Task> LocalTasksHashSet { get; } = new HashSet<Task>();

        public HashSet<Task> GlobalTasksHashSet { get; }

        public RunningTasksContext(HashSet<Task> globalTasksHashSet)
        {
            GlobalTasksHashSet = globalTasksHashSet;
        }

        public int GetGlobalCount()
        {
            lock (this)
            {
                return GlobalTasksHashSet.Count;
            }
        }

        public int GetLocalCount()
        {
            lock (this)
            {
                return LocalTasksHashSet.Count;
            }
        }

        public void RemoveTaskFromLocalAndGlobal( Task t)
        {
            lock (this)
            {
                LocalTasksHashSet.Remove(t);
                GlobalTasksHashSet.Remove(t);
            }
        }

        public void AddTaskToLocalAndGlobal(Task t, bool andStart)
        {
            lock (this)
            {
                LocalTasksHashSet.Add(t);
                GlobalTasksHashSet.Add(t);
                if (andStart)
                    t.Start();
            }
        }

        public Task FirstOrDefaultLocal()
        {
            lock (this)
            {
                return LocalTasksHashSet.FirstOrDefault();
            }
        }
    }
}
