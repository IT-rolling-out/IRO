using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IRO.LoggingExt
{
    class MethodLoggerFactory : IMethodLoggerFactory
    {
        readonly IServiceProvider _serviceProvider;
        readonly MethodLoggingSettings _settings;

        public MethodLoggerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _settings = serviceProvider.GetService<MethodLoggingSettings>() ?? new MethodLoggingSettings();
        }

        public IMethodLogger CreateMethodLogger(Type callerType)
        {
            if (callerType == null) 
                throw new ArgumentNullException(nameof(callerType));
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            return new MethodLogger(loggerFactory, callerType, _settings);
        }

        public IMethodLogger CreateMethodLogger<TCaller>()
        {
            return CreateMethodLogger(typeof(TCaller));
        }
    }
}