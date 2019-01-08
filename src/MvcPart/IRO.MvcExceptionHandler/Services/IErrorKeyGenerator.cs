using System;

namespace IRO.MvcExceptionHandler.Services
{
    public interface IErrorKeyGenerator
    {
        string GenerateErrorKey(Type exceptionType);
    }
}
