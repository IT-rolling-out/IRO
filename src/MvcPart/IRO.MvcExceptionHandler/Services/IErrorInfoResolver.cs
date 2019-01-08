using System;
using IRO.MvcExceptionHandler.Models;

namespace IRO.MvcExceptionHandler.Services
{
    public interface IErrorInfoResolver
    {
        ErrorInfo GetByErrorKey(string errorKey);
        ErrorInfo GetByException(Type exceptionType);        
        ErrorInfo GetByHttCode(int httpCode);
    }
}