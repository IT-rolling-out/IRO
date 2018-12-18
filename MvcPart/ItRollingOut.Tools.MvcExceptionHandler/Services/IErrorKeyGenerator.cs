using System;

namespace ItRollingOut.Tools.MvcExceptionHandler.Services
{
    public interface IErrorKeyGenerator
    {
        string GenerateErrorKey(Type exceptionType);
    }
}
