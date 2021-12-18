using System;

namespace IRO.LoggingExt
{
    public interface IMethodLoggerFactory
    {
        IMethodLogger CreateMethodLogger(Type callerType);

        IMethodLogger CreateMethodLogger<TCaller>();
    }
}