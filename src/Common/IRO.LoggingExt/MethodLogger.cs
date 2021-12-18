using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace IRO.LoggingExt
{
    class MethodLogger : IMethodLogger
    {
        readonly ILoggerFactory _loggerFactory;
        readonly Type _callerType;
        readonly MethodLoggingSettings _settings;

        public MethodLogger(ILoggerFactory loggerFactory, Type callerType, MethodLoggingSettings settings)
        {
            _loggerFactory = loggerFactory;
            _callerType = callerType;
            _settings = settings;
        }


        public IMethodLogScope MethodLogScope(
            ILogger logger = null,
            [CallerMemberName] string methodName = null
            )
        {
            if (_settings.LogLevel > LogLevel.Error)
            {
                return new EmptyDisposable();
            }

            logger ??= _loggerFactory.CreateLogger(_callerType);

            if (methodName == null)
            {
                logger.LogError("Caller member name is null.");
                return new EmptyDisposable();
            }
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
                        BindingFlags.CreateInstance | BindingFlags.Default | BindingFlags.GetProperty |
                        BindingFlags.SetProperty;
            var methodInfo = _callerType.GetMethod(methodName, flags);
            if (methodInfo == null)
            {
                logger.LogError($"Can't resolve methodInfo for {methodName}");
                return new EmptyDisposable();
            }

            return new MethodLogScope(logger, _settings, methodInfo);
        }
    }
}