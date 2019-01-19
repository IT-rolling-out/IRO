using IRO.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using IRO.Common.Text;

namespace IRO.Mvc.MvcExceptionHandler.Controllers
{
    [Route("DevExceptionsPage/")]
    [ApiController]
    public class DevExceptionsPageController:ControllerBase
    {
        readonly static IDictionary<string, Tuple<Exception, HttpContext>> _exceptionsDict
            = new ConcurrentDictionary<string, Tuple<Exception, HttpContext>>();
        
        IHostingEnvironment _hostingEnvironment;

        public DevExceptionsPageController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("{id}")]
        [HttpGet]
        public void Open(string id)
        {
            if (_exceptionsDict.TryGetValue(id, out var data))
            {                
                RequestDelegate next = async (ctx) =>
                {
                    var exceptionDispatchInfo=ExceptionDispatchInfo.Capture( data.Item1);
                    exceptionDispatchInfo.Throw();
                };
                var loggerFactory = new LoggerFactory();
                var opt = new DeveloperExceptionPageOptions();
                DiagnosticSource diagnosticSource = new DiagnosticListener("");
                var developerExceptionPageMiddleware = new DeveloperExceptionPageMiddleware(
                    next,
                    Options.Create(opt),
                    loggerFactory,
                    _hostingEnvironment,
                    diagnosticSource
                    );
                developerExceptionPageMiddleware.Invoke(data.Item2);
            }
            else
            {
                Response.WriteAsync("Can`t find exception object in cache. Maybe it removed or save on another server node.");
            }
        }

        /// <summary>
        /// Return id of page where you can open exception.
        /// Example: 'https://yourhost.com/DevExceptionsPage/jr2DkM230d
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string AddException(Exception exception, HttpContext httpContext)
        {
            if (_exceptionsDict.Count > 200)
            {
                _exceptionsDict.Clear();
            }
            var id = TextExtensions.Generate(10);
            var data = new Tuple<Exception, HttpContext>(
                exception,
                httpContext
                );
            _exceptionsDict.Add(id, data);
            return id;
        }
    }
}
