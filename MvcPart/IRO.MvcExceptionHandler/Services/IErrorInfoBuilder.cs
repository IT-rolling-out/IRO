using System;
using IRO.MvcExceptionHandler.Models;

namespace IRO.MvcExceptionHandler.Services
{
    public interface IErrorInfoBuilder
    {
        void Register(ErrorInfo errorInfo);
        void RegisterAllAssignable(AssignableErrorsInfo assignableErrorsInfo);

        IErrorInfoResolver Build();
    }
}