using System;
using ItRollingOut.Tools.MvcExceptionHandler.Models;

namespace ItRollingOut.Tools.MvcExceptionHandler.Services
{
    public interface IErrorInfoResolver
    {
        ErrorInfo GetByErrorKey(string errorKey);
        ErrorInfo GetByException(Type exceptionType);        
        ErrorInfo GetByHttCode(int httpCode);
    }
}