using System;
using IRO.Mvc.MvcExceptionHandler.Models;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public static class ErrorInfoResolverExtensions 
    {
        public static ErrorInfo GetByException<TException>(this IErrorInfoResolver @this) where TException : Exception
        {
            return @this.GetByException(typeof(TException));
        }

        public static ErrorInfo? TryGetByErrorKey(this IErrorInfoResolver @this, string errorKey)
        {
            try
            {
                return @this.GetByErrorKey(errorKey);
            }
            catch
            {
                return null;
            }
        }

        public static ErrorInfo? TryGetByException(this IErrorInfoResolver @this, Type exceptionType)
        {
            try
            {
                return @this.GetByException(exceptionType);
            }
            catch 
            {
                return null;
            }
        }

        public static ErrorInfo? TryGetByException<TException>(this IErrorInfoResolver @this) where TException : Exception
        {
            try
            {
                return @this.GetByException<TException>();
            }
            catch 
            {
                return null;
            }
        }

        public static ErrorInfo? TryGetByHttCode(this IErrorInfoResolver @this, int httpCode)
        {
            try
            {
                return @this.GetByHttCode(httpCode);
            }
            catch 
            {
                return null;
            }
        }
    }
}