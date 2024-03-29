﻿using System;
using System.Threading;
using System.Threading.Tasks;
using IRO.UnitTests.Common;

namespace IRO.UnitTests.Common.Helpers
{
    static class HardWait
    {
        public static Task Delay(int ms)
        {
            return Delay(TimeSpan.FromMilliseconds(ms));
        }

        public static async Task Delay(TimeSpan timeSpan)
        {
            var startDT = DateTime.Now;
            while ((DateTime.Now - startDT) < timeSpan)
            {
                Thread.Sleep(1);
            }
            //await Task.Delay(timeSpan);
        }
    }
}
