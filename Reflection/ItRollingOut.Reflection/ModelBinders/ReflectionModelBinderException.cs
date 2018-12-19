using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ItRollingOut.Reflection.ModelBinders
{
    public class ReflectionModelBinderException : Exception
    {
        public ReflectionModelBinderException()
        {
        }

        public ReflectionModelBinderException(string message) : base(message)
        {
        }

        public ReflectionModelBinderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReflectionModelBinderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
