using System;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public interface IErrorKeyGenerator
    {
        string GenerateErrorKey(Type exceptionType);
    }
}
