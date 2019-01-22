using System;

namespace IRO.Reflection.SummarySearch.Exceptions
{
    /// <summary>
    /// An exception thrown by the DocsByReflection library
    /// </summary>
   	[Serializable]
    class DocsParserException : Exception
    {
        /// <summary>
        /// Initializes a new exception instance with the specified
        /// error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or null if none.</param>
        public DocsParserException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
