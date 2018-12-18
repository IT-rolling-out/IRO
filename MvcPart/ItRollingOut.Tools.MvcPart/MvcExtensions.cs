using ItRollingOut.Tools.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.Tools.MvcPart
{
    public static class MvcExtensions
    {
        /// <summary>
        /// Current name used as key to save request body text in HttpContext.Items.
        /// </summary>
        public const string RequestBodyTextItemName = "RequestBodyText";


        public static string GetRequestBodyText(this HttpContext httpContext)
        {
            if(httpContext.Items.TryGetValue(RequestBodyTextItemName, out var cachedText))
            {
                return (string)cachedText;
            }
            string text=CommonHelpers.ReadAllTextFromStream(httpContext.Request.Body);
            httpContext.Items[RequestBodyTextItemName] = text;
            return text;
        }
    }
}
