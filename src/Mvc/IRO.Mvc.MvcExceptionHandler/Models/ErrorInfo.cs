using System;

namespace IRO.Mvc.MvcExceptionHandler.Models
{
    /// <summary>
    /// ������, ������� ��������� ����� ����� http �����, ����������� � ErrorKey.
    /// </summary>
    public struct ErrorInfo
    {
        public int? HttpCode { get; set; }

        public string ErrorKey { get; set; }

        public Type ExceptionType { get; set; }

        public override string ToString()
        {
            return $"({nameof(ErrorInfo)}: " +
                $"{nameof(HttpCode)} - {HttpCode}, " +
                $"{nameof(ErrorKey)} - {ErrorKey}, " +
                $"{nameof(ExceptionType)} - {ExceptionType?.Name})";
        }
    }



}
