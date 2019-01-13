using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Mvc.GetToPostProxying
{
    public static class GetToPost
    {
        private static HttpClient _httpClient = new HttpClient();

        private static string _domainName;

        public static GetToPostRequestContentType RequestContentType { get; set; } = GetToPostRequestContentType.Json;

        /// <summary>
        /// Метод нужно добавить в конвеер обработки запроса. После этого, все get запросы начинающиеся с "/api-get" будут
        /// конвертированы в post запросы к соответствующим методам.
        /// <para></para>
        /// Пока есть поддержка контента form url encoded и json. Поддержка заголовков.
        /// <para></para>
        /// Метод предназначен для более простой отладки и не должен использоваться в продакшене.
        /// </summary>
        public static async Task GetToPostRequestHandler(HttpContext httpContext, Func<Task> next)
        {
            var req = httpContext.Request;
            var resp = httpContext.Response;
            var isGetToPostRequest = req.Path.HasValue && req.Path.Value.StartsWith("/api-get");

            if (isGetToPostRequest && req.Method == "GET")
            {
                if (_domainName == null)
                {
                    _domainName = req.Scheme
                        + "://"
                        + req.Host.Value;
                }

                var requestUrl = _domainName + req.Path.ToString().Replace("/api-get/", "/api/");

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                var queryDict = req.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                if (RequestContentType == GetToPostRequestContentType.FormData)
                {
                    httpRequestMessage.Content = new FormUrlEncodedContent(queryDict);
                }
                else
                {
                    var jsonStr = JsonConvert.SerializeObject(queryDict);
                    httpRequestMessage.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                }

                foreach (var header in req.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value.ToString());
                }

                var realResp = await _httpClient.SendAsync(httpRequestMessage);
                var respStr = await realResp.Content.ReadAsStringAsync();

                foreach (var header in realResp.Headers)
                {
                    resp.Headers.Add(header.Key, header.Value.ToString());
                }

                resp.StatusCode = (int)realResp.StatusCode;
                resp.ContentType = realResp.Content?.Headers?.ContentType?.MediaType;
                await httpContext.Response.WriteAsync(respStr);
            }
            else
            {
                await next();
            }
        }
    }
}
