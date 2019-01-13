using System;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public static class InnerExceptionsResolvers
    {
        public static Exception InspectAggregateException(Exception originalException)
        {
            if (originalException is AggregateException aggregateException)
            {
                foreach (var innerEx in aggregateException.InnerExceptions)
                {
                    if (innerEx is AggregateException)
                    {
                        var recusionRes = InspectAggregateException(innerEx);
                        if (!(recusionRes is AggregateException))
                            return recusionRes;
                    }
                    else
                        return innerEx;
                }
            }
            return originalException;
        }

        public static Exception NotInspect(Exception originalException)
        {
            return originalException;
        }
    }
}
