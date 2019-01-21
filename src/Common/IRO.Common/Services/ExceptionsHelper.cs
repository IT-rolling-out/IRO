using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.Common.Services
{

    public static class ExceptionsHelper
    {
        /// <summary>
        /// Find first inner exception (or current) that is assignable from soughtExceptionType.
        /// Or return null.
        /// <para></para>
        /// If AggregateException - will search in all InnerExceptions.
        /// </summary>
        public static Exception FindInnerException(Type soughtExceptionType, Exception ownerEx)
        {
            if (soughtExceptionType == null)
                throw new ArgumentNullException(nameof(soughtExceptionType));

            if (ownerEx == null)
                return null;

            if (soughtExceptionType.IsAssignableFrom(ownerEx.GetType()))
            {
                return ownerEx;
            }
            else if (ownerEx is AggregateException aggregateEx)
            {
                foreach (var innerEx in aggregateEx.InnerExceptions)
                {
                    var foundEx = FindInnerException(soughtExceptionType, innerEx);
                    if (foundEx != null)
                        return foundEx;
                }
            }
            else
            {
                var foundEx = FindInnerException(soughtExceptionType, ownerEx.InnerException);
                if (foundEx != null)
                    return foundEx;
            }
            return null;
        }

        /// <summary>
        /// Находит вложенное исключение по его типу.
        /// </summary>
        public static TException FindInnerException<TException>(Exception parentEx) where TException : Exception
        {
            return FindInnerException(typeof(TException), parentEx) as TException;
        }
    }
}
