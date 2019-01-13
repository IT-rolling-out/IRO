using IRO.Mvc.MvcExceptionHandler.Models;
using IRO.Mvc.MvcExceptionHandler.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IRO.Mvc.MvcExceptionHandler
{
    /// <summary>
    /// Реализую здесь же IExceptionHandlerConfigs от лени, чтоб не заморачиваться над копированием.
    /// </summary>
    public class MvcExceptionHandlerSetup : IExceptionHandlerConfigs
    {
        internal Action<IErrorInfoBuilder> ActionToRegisterExceptions { get; private set; }

        /// <summary>
        /// Default is 500.
        /// </summary>
        public int DefaultHttpCode { get; set; } = 500;

        /// <summary>
        /// Default settings allow serializer to ignore default values when serializing.
        /// For ErrorDTO it is more comfortable way.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        /// <summary>
        /// Current host.
        /// Example http://localhost:80
        /// </summary>
        public string Host { get; set; } 

        /// <summary>
        /// Can use default.
        /// </summary>
        public IErrorKeyGenerator KeyGenerator { get; set; } = new ErrorKeyGenerator();

        /// <summary>
        /// Can use default.
        /// </summary>
        public IErrorKeyValidator KeyValidator { get; set; } = new ErrorKeyValidator();

        /// <summary>
        /// FormattedErrorDescriptionUrlHandler works fine, you must set it here or it will not generate error urls.
        /// </summary>
        public IErrorDescriptionUrlHandler ErrorDescriptionUrlHandler { get; set; }

        /// <summary>
        /// Default used for AggregateException (find first inner exception in it).
        /// </summary>
        public Func<Exception, Exception> InnerExceptionsResolver { get; set; } = InnerExceptionsResolvers.InspectAggregateException;

        /// <summary>
        /// If true - will handle success results too if can find their httpcode in errors dictionary and if ResponseBody empty.
        /// Default value is true.
        /// </summary>
        public bool CanBindByHttpCode { get; set; } = true;

        /// <summary>
        /// Executed after ErrorDTO generated, here you can add your own data before it will be returned.
        /// </summary>
        public ResponsesFilterBeforeDelegate FilterBeforeDTO { get; set; }

        /// <summary>
        /// Executed before default mapping to allow user write custom methods to handle exceptions.
        /// </summary>
        public ResponsesFilterAfterDelegate FilterAfterDTO { get; set; }

        public bool IsDebug { get; set; }

        /// <summary>
        /// Handler for exceptions in current middleware.
        /// </summary>
        public Action<Exception> OwnExceptionsHandler { get; set; }

        public void Mapping(Action<IErrorInfoBuilder> actionToRegisterExceptions)
        {
            if (ActionToRegisterExceptions != null)
                throw new ErrorHandlerException("Can`t use mapping more than one time.");
            ActionToRegisterExceptions = actionToRegisterExceptions;
        }

        public IExceptionHandlerConfigs CreateConfigs()
        {
            return this;
        }
    }
}
