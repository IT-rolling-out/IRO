using Microsoft.AspNetCore.Http;
using System;

namespace IRO.Mvc.MvcExceptionHandler.Models
{
    /// <summary>
    /// Контекст ошибки, используемый в мидлвере.
    /// Все значения могут быть null в начале обработки.
    /// </summary>
    public class ErrorContext
    {
        public Exception OriginalException { get; set; }

        public Exception InnerException { get; set; }

        public ErrorInfo ErrorInfo { get; set; }

        public ErrorDTO ResponseDTO { get; set; }

        public IExceptionHandlerConfigs Configs { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}
