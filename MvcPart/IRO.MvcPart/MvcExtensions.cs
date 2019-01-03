using IRO.Common.Services;
using Microsoft.AspNetCore.Http;

namespace IRO.MvcPart
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
