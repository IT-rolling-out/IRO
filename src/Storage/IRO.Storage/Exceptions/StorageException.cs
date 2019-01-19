using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace IRO.Storage.Exceptions
{
    public class StorageException : Exception
    {
        public StorageException()
        {
        }

        public StorageException(string message) : base(message)
        {
        }

        public StorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
