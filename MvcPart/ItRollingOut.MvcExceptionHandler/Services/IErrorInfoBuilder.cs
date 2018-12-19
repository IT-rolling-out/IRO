using System;
using ItRollingOut.MvcExceptionHandler.Models;

namespace ItRollingOut.MvcExceptionHandler.Services
{
    public interface IErrorInfoBuilder
    {
        void Register(ErrorInfo errorInfo);
        void RegisterAllAssignable(AssignableErrorsInfo assignableErrorsInfo);

        IErrorInfoResolver Build();
    }
}