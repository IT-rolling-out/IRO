using System;
using System.Collections.Generic;
using System.Text;
using Amib.Threading;

namespace IRO.Threading
{
    public static class ThreadPool
    {
        public static SmartThreadPool Global { get; } = new SmartThreadPool();
    }
}
