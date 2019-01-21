using IRO.Common.Services;
using Microsoft.AspNetCore.Http;

namespace IRO.Mvc.Core
{
    public static class MvcExtensions
    {
        /// <summary>
        /// Current name used as key to save request body text in HttpContext.Items.
        /// </summary>
        public const string RequestBodyTextItemName = "RequestBodyText";

        /// <summary>
        /// Read request contetn to string and then return cached value.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetRequestBodyText(this HttpContext httpContext)
        {
            if(httpContext.Items.TryGetValue(RequestBodyTextItemName, out var cachedText))
            {
                return (string)cachedText;
            }
            string text=StreamHelpers.ReadAllTextFromStream(httpContext.Request.Body);
            httpContext.Items[RequestBodyTextItemName] = text;
            return text;
        }
    }
}
