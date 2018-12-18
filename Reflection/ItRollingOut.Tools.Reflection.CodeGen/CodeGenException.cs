using System;
using System.Runtime.Serialization;

namespace ItRollingOut.Tools.Reflection.CodeGen
{
    public class CodeGenException : Exception
    {
        public CodeGenException()
        {
        }

        public CodeGenException(string message) : base(message)
        {
        }

        public CodeGenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CodeGenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
