using System;
using System.Diagnostics;
using System.Threading.Tasks;
using IRO.LoggingExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace IRO.UnitTests.Common
{
    public class LoggerExtensionsTest
    {
        IMethodLogger _loggingExt;
        ILogger _logger;

        [SetUp]
        public void Init()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging((logBuilder) =>
            {
                logBuilder.AddDebug();
                logBuilder.AddConsole();
            });
            services.AddMethodLogging();
            var sp = services.BuildServiceProvider();
            var factory = sp.GetRequiredService<IMethodLoggerFactory>();
            _loggingExt = factory.CreateMethodLogger(GetType());
            _logger = sp.GetRequiredService<ILogger<LoggerExtensionsTest>>();
        }

        [Test]
        public async Task TestMethodLogging()
        {
            TestNormal(5, 6);
            await Task.Delay(1000);
            Console.WriteLine("\n==============\n");
            try
            {
                TestError(5, 6);
            }
            catch { }
            await Task.Delay(1000);
            Console.WriteLine("\n==============\n");
            await AsyncTestNormal(5, 6);
            await Task.Delay(1000);
            Console.WriteLine("\n==============\n");
            try
            {
                await AsyncTestError(5, 6);
            }
            catch { }

            Assert.Pass();
        }

        int TestNormal(int a, int b)
        {
            using var methodLogScope = _loggingExt
                .MethodLogScope()
                .WithArguments(a, b)
                .WithAdditionalValue("basketGuid", "af33rrr32-32r23-323232t");

            var sum = a + b;

            return methodLogScope.WithReturn(sum);
        }

        int TestError(int a, int b)
        {
            using var methodLogScope = _loggingExt
                .MethodLogScope()
                .WithArguments(a, b)
                .WithAdditionalValue("basketGuid", "af33rrr32-32r23-323232t");

            var sum = a + b;
            throw new Exception("AAAAAA");

            return methodLogScope.WithReturn(sum);
        }

        async Task<int> AsyncTestNormal(int a, int b)
        {
            using var methodLogScope = _loggingExt
                .MethodLogScope()
                .WithArguments(a, b, 100)
                .WithAdditionalValue("basketGuid", "af33rrr32-32r23-323232t");

            await Task.Run(() => { });
            var sum = a + b;

            return methodLogScope.WithReturn(sum);
        }

        async Task<int> AsyncTestError(int a, int b)
        {

            await Task.Run(() => { });

            using var methodLogScope = _loggingExt
                .MethodLogScope()
                .WithArguments(a, b)
                .WithAdditionalValue("basketGuid", "af33rrr32-32r23-323232t");

            await Task.Run(() => { });
            await Task.Run(() => { });
            await Task.Run(() => { });
            var sum = a + b;
            throw new Exception("AAAAAA");

            return methodLogScope.WithReturn(sum);
        }

        int TestNormal_Analog(int a, int b)
        {
            try
            {
                var sum = a + b;

                var msg = "Method {CallerNamespase}.{CallerClass}.{CalledMethod} called." +
                          "\n\nWith arguments: {Argument_a}, {Argument_b}." +
                          "\n\nWith additional values: {someAdditionalValueName}." +
                          "\n\nReturned: {Result}";
                _logger.LogInformation(
                    msg,
                    GetType().Namespace,
                    GetType().Name,
                    nameof(TestNormal_Analog),
                    a,
                    b,
                    "af33rrr32-32r23-323232t",
                    sum
                );
                return sum;
            }
            catch (Exception ex)
            {
                var msg = "Method {CallerNamespase}.{CallerClass}.{CalledMethod} called." +
                          "\n\nWith arguments: {Argument_a}, {Argument_b}." +
                          "\n\nWith additional values: {basketGuid}." +
                          "\n\nFinished with exception: {Exception}";
                _logger.LogInformation(
                    msg,
                    GetType().Namespace,
                    GetType().Name,
                    nameof(TestNormal_Analog),
                    a,
                    b,
                    "af33rrr32-32r23-323232t",
                    ex
                );
                throw;
            }
        }

    }
}