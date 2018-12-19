using System;
using ItRollingOut.MvcExceptionHandler.Models;

namespace ItRollingOut.MvcExceptionHandler.Services
{
    public interface IErrorInfoResolver
    {
        ErrorInfo GetByErrorKey(string errorKey);
        ErrorInfo GetByException(Type exceptionType);        
        ErrorInfo GetByHttCode(int httpCode);
    }
}