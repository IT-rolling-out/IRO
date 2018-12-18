using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.Tools.Services
{
    
    public static class ExceptionsHelper
    {
        /// <summary>
        /// Находит вложенное исключение по его типу.
        /// </summary>
        public static T FindInnerExceptionInAggregateException<T>(AggregateException parentEx) where T : Exception
        {
            if (parentEx == null)
                return null;

            foreach (var innerEx in parentEx.InnerExceptions)
            {
                if (innerEx is AggregateException)
                    return FindInnerExceptionInAggregateException<T>((AggregateException)innerEx);
                else
                    return FindInnerExceptionInAggregateException<T>(innerEx);
            }

            return null;
        }

        /// <summary>
        /// Находит вложенное исключение по его типу.
        /// </summary>
        public static T FindInnerExceptionInAggregateException<T>(Exception parentEx) where T : Exception
        {
            if (parentEx == null)
                return null;
            if (parentEx is T)
                return (T)parentEx;
            if (parentEx is AggregateException)
                return FindInnerExceptionInAggregateException<T>(((AggregateException)parentEx));
            else
            {
                var innerEx = parentEx.InnerException;
                return FindInnerExceptionInAggregateException<T>(innerEx);
            }

        }

        
    }
}
