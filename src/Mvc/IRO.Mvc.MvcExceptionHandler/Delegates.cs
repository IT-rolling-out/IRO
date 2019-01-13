using IRO.Mvc.MvcExceptionHandler.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Mvc.MvcExceptionHandler
{
    /// <summary>
    /// Возвращает true, если исключение обработано вручную и не нужно создавать dto результата.
    /// </summary>
    public delegate Task<bool> ResponsesFilterBeforeDelegate(        
        ErrorContext errorContext
        );

    /// <summary>
    /// Возвращает true, если исключение обработано вручную и нужно прекратить дальнейшую обработку.
    /// Позволяет декорировать стандартный ответ изменя ErrorDTO.
    /// </summary>
    public delegate Task<bool> ResponsesFilterAfterDelegate(
        ErrorContext errorContext      
        );
}
