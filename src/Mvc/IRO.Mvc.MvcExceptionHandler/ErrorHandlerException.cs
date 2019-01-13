using System;
using System.Runtime.Serialization;

namespace IRO.Mvc.MvcExceptionHandler
{
    public class ErrorHandlerException : Exception
    {
        public ErrorHandlerException()
        {
        }

        public ErrorHandlerException(string message) : base(message)
        {
        }

        public ErrorHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ErrorHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
