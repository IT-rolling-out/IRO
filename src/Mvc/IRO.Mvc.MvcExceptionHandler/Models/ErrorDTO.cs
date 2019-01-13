using Newtonsoft.Json;
using System.Collections.Generic;

namespace IRO.Mvc.MvcExceptionHandler.Models
{
    public class ErrorDTO
    {
        /// <summary>
        /// Just json property.
        /// </summary>
        [JsonProperty(PropertyName = "__IsError")]
        public bool IsError => true;

        /// <summary>
        /// Строка, которая кратко описывает исключение. Аналогично enum, но именно строка для универсальности.
        /// Мидлвера валидирует эту строку в режиме дебага. Стандартный валидатор выкинет исключение, если строка содержит 
        /// любые символы кроме латинцы и нижнего подчеркивания.
        /// <para></para>
        /// Обычно это имя исключения, при стандартном маппинге. Например, для ArgumentNullException это ArgumentNull.
        /// </summary>
        public string ErrorKey { get; set; }

        public string InfoUrl { get; set; }

        /// <summary>
        /// Словарь для доп. данных. 
        /// Значение по-умолчанию null, нужно создать словарь.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        #region Debug info
        public string DebugUrl { get; set; }

        /// <summary>
        /// Сообщение ошибки, используется при дебаге.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Сообщение ошибки, используется при дебаге.
        /// </summary>
        public string StackTrace { get; set; }

        public RequestInfoDTO RequestInfo { get; set; }
        #endregion
    }
}
