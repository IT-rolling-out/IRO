using System;

namespace IRO.Mvc.MvcExceptionHandler.Models
{
    /// <summary>
    /// Модель, которая используется для биндинга всех исключений по базовому типу. 
    /// </summary>
    public struct AssignableErrorsInfo
    {
        public int? HttpCode { get; set; }

        public string ErrorKeyPrefix { get; set; }

        public Type BaseExceptionType { get; set; }

        public override string ToString()
        {
            return $"({nameof(ErrorInfo)}: " +
                $"{nameof(HttpCode)} - {HttpCode}, " +
                $"{nameof(ErrorKeyPrefix)} - {ErrorKeyPrefix}, " +
                $"{nameof(BaseExceptionType)} - {BaseExceptionType?.Name})";
        }
    }



}
