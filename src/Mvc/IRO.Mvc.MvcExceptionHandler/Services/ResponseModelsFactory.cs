using Microsoft.AspNetCore.Http;
using IRO.Common.Collections;
using IRO.Mvc;
using IRO.Mvc.MvcExceptionHandler.Models;
using System;
using IRO.Mvc.Core;
using IRO.Mvc.MvcExceptionHandler.Controllers;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public class ResponseModelsFactory
    {
        public string CreateDebugUrl(ErrorContext errorContext)
        {
            var methodPath = "DevExceptionsPage/" + DevExceptionsPageController.AddException(
                errorContext.OriginalException,
                errorContext.HttpContext
                );
            var host=errorContext.Configs.Host ?? "";
            if (!host.EndsWith("/"))
                host += "/";
            return host + methodPath;
        }

        public ErrorDTO CreateErrorData(ErrorContext errorContext)
        {
            var errorInfo = errorContext.ErrorInfo;
            var err = new ErrorDTO();
            err.ErrorKey = errorInfo.ErrorKey;
            err.InfoUrl = errorContext.Configs.ErrorDescriptionUrlHandler?.GenerateUrl(errorInfo.ErrorKey);

            if (errorContext.Configs.IsDebug)
            {
                if (errorContext.OriginalException != null)
                {
                    err.Message = errorContext.OriginalException.Message;
                    err.StackTrace = errorContext.OriginalException.ToString();
                    try
                    {
                        err.DebugUrl = CreateDebugUrl(errorContext);
                    }
                    catch { }
                }
                err.RequestInfo = CreateRequestInfo(errorContext.HttpContext);                
            }
            return err;
        }

        public RequestInfoDTO CreateRequestInfo(HttpContext httpContext)
        {
            var requestInfo = new RequestInfoDTO();
            var req = httpContext.Request;

            try
            {
                requestInfo.QueryParameters = req.Query.PairToDictionary();
            }
            catch { }

            try
            {
                requestInfo.Headers = req.Headers.PairToDictionary();
            }
            catch { }

            try
            {
                requestInfo.Cookies = req.Cookies.PairToDictionary();
            }
            catch { }

            try
            {
                if (req.HasFormContentType)
                {
                    requestInfo.FormParameters = req.Form.PairToDictionary();
                }
            }
            catch { }

            try
            {
                requestInfo.ContentLength = req.ContentLength ?? 0;
                var lengthInKB = requestInfo.ContentLength / 1024;
                if (lengthInKB <= 500)
                {
                    requestInfo.BodyText = httpContext.GetRequestBodyText();
                }
            }
            catch { }

            requestInfo.RequestPath = req.Path;
            requestInfo.ContentType = req.ContentType;           
            return requestInfo;
        }
    }
}
