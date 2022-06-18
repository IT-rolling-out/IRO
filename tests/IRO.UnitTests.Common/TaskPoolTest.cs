using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IRO.Threading;
using NUnit.Framework;

namespace IRO.UnitTests.Common
{
    public class TaskPoolTest
    {
        [Test]
        public async Task ExceptionTest()
        {
            try
            {
                await TaskPool.Global.Run(async () =>
                {
                    await TaskPool.Global.Run(async () =>
                    {
                        throw new Exception("Hi!");
                    });
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Hi!")
                {
                    Assert.Pass();
                }
            }
            Assert.Fail();
        }

        [Test]
        public async Task CancelTest()
        {
            try
            {
                var cancelTokenSource = new CancellationTokenSource();
                await TaskPool.Global.Run(async () =>
                {
                    await TaskPool.Global.Run(async () =>
                    {
                        cancelTokenSource.Cancel();
                    });
                }, cancelTokenSource.Token);
            }
            catch (TaskCanceledException ex)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}
