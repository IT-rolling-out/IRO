using System;

namespace IRO.UnitTests.Common
{
    public class ThreadsCounter
    {
        int _threadsCount;
        int ThreadsCount
        {
            get
            {
                return _threadsCount;
            }
            set
            {
                _threadsCount = value;
                if (MaxThreadsCount < _threadsCount)
                    MaxThreadsCount = _threadsCount;
                //Console.WriteLine($"Threads count: {_threadsCount}.");

            }
        }

        public int MaxThreadsCount { get; set; }

        public void ThreadStart()
        {
            lock (this)
                ThreadsCount++;
        }

        public void ThreadEnd()
        {
            lock (this)
                ThreadsCount--;
        }

        public void PrintMsg()
        {
            Console.WriteLine($"Max threads count: {MaxThreadsCount}.");
        }
    }
}
