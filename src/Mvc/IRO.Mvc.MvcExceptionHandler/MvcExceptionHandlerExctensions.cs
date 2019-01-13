using System;
using Microsoft.AspNetCore.Builder;

namespace IRO.Mvc.MvcExceptionHandler
{
    public static class MvcExceptionHandlerExctensions
    {
        /// <summary>
        /// Must be registered after UseDeveloperExceptionPage, but before anything else.
        /// </summary>
        public static IApplicationBuilder UseMvcExceptionHandler(this IApplicationBuilder app, Action<MvcExceptionHandlerSetup> setupAction)
        {
            var middleware = new ExHandlerMiddleware(setupAction);
            app.Use(middleware.RequestProcessing);
            return app;
        }
    }
}
