using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRO.Mvc.MvcExceptionHandler.Models
{
    public class RequestInfoDTO
    {
        public string RequestPath { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }        

        public IDictionary<string, StringValues> Headers { get; set; }     

        public IDictionary<string, string> Cookies { get; set; }

        public IDictionary<string, StringValues> QueryParameters { get; set; }

        public IDictionary<string, StringValues> FormParameters { get; set; }

        public string BodyText { get; set; }
    }
}
