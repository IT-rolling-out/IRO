using System;
using System.Runtime.Serialization;

namespace IRO.Cache.Exceptions
{
    public class CacheException : Exception
    {
        public CacheException()
        {
        }

        public CacheException(string message) : base(message)
        {
        }

        public CacheException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CacheException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
