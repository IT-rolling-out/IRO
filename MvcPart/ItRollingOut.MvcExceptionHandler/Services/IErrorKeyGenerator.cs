using System;

namespace ItRollingOut.MvcExceptionHandler.Services
{
    public interface IErrorKeyGenerator
    {
        string GenerateErrorKey(Type exceptionType);
    }
}
