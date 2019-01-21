using System;
using System.Runtime.Serialization;

namespace IRO.Tests.SwashbuckleTest.Exceptions
{
    public class ClientException : Exception
    {
        public ClientException()
        {
        }

        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
