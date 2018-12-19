using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ItRollingOut.MvcExceptionHandler.Services
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
