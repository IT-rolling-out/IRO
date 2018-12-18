using System;
using ItRollingOut.Tools.MvcExceptionHandler.Models;

namespace ItRollingOut.Tools.MvcExceptionHandler.Services
{
    public interface IErrorInfoBuilder
    {
        void Register(ErrorInfo errorInfo);
        void RegisterAllAssignable(AssignableErrorsInfo assignableErrorsInfo);

        IErrorInfoResolver Build();
    }
}