using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwashbuckleTest.Exceptions
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
