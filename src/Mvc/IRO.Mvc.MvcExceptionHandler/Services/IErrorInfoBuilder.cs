using System;
using IRO.Mvc.MvcExceptionHandler.Models;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public interface IErrorInfoBuilder
    {
        void Register(ErrorInfo errorInfo);
        void RegisterAllAssignable(AssignableErrorsInfo assignableErrorsInfo);

        IErrorInfoResolver Build();
    }
}