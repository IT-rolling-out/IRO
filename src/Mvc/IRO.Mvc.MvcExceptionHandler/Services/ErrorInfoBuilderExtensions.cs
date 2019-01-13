using System;
using IRO.Mvc.MvcExceptionHandler.Models;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public static class ErrorInfoBuilderExtensions
    {
        /// <summary>
        /// Регистрирует всех наслдников класс TException автоматически.
        /// Точнее, использует "ленивую регистрацию" проверяя тип исключения по запросу. 
        /// Из-за этого GetByErrorKey может не сработать для исключения при первом вызове.
        /// </summary>
        public static void RegisterAllAssignable<TException>(this IErrorInfoBuilder @this, string errorKeyPrefix=null, int? httpCode=null) 
            where TException : Exception
        {
            var assignableErrInfo = new AssignableErrorsInfo()
            {
                BaseExceptionType = typeof(TException),
                ErrorKeyPrefix = errorKeyPrefix,
                HttpCode = httpCode
            };
            @this.RegisterAllAssignable(assignableErrInfo);
        }

        /// <summary>
        /// Регистрирует errorKey для http кода. В этом методе нельзя автоматически сгенерировать errorKey, потому 
        /// его обязательно передать явно.
        /// </summary>
        /// <param name="httpCode"></param>
        /// <param name="errorKey"></param>
        public static void Register(this IErrorInfoBuilder @this, int? httpCode, string errorKey)
        {
            var errorInfo = new ErrorInfo();
            errorInfo.HttpCode = httpCode;
            errorInfo.ErrorKey = errorKey;
            @this.Register(errorInfo);
        }

        /// <summary>
        /// Регистрирует исключение. 
        /// </summary>
        /// <param name="httpCode">Можно также задать для него соответствующий httpCode или будет использован код по-умолчанию.</param>
        /// <param name="errorKey">Генерируется автоматически, если null или пустая строка (по имени исключения).</param>
        public static void Register<TException>(this IErrorInfoBuilder @this, int? httpCode = null, string errorKey = null) 
            where TException : Exception
        {
            var errorInfo = new ErrorInfo();
            errorInfo.ExceptionType = typeof(TException);
            errorInfo.HttpCode = httpCode;
            errorInfo.ErrorKey = errorKey;
            @this.Register(errorInfo);
        }
    }
}