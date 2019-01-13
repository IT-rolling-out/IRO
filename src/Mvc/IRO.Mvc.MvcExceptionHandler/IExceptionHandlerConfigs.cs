using System;
using IRO.Mvc.MvcExceptionHandler.Services;

namespace IRO.Mvc.MvcExceptionHandler
{
    public interface IExceptionHandlerConfigs
    {
        bool CanBindByHttpCode { get; }

        int DefaultHttpCode { get; }

        IErrorDescriptionUrlHandler ErrorDescriptionUrlHandler { get; }

        Func<Exception, Exception> InnerExceptionsResolver { get; }

        bool IsDebug { get; }

        IErrorKeyGenerator KeyGenerator { get; }

        IErrorKeyValidator KeyValidator { get; }

        string Host { get; }
    }
}