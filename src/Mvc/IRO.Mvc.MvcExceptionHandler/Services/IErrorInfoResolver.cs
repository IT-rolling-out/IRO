using System;
using IRO.Mvc.MvcExceptionHandler.Models;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public interface IErrorInfoResolver
    {
        ErrorInfo GetByErrorKey(string errorKey);
        ErrorInfo GetByException(Type exceptionType);        
        ErrorInfo GetByHttCode(int httpCode);
    }
}