using Newtonsoft.Json;

namespace IRO.Common.Serialization
{
    /// <summary>
    /// Serialize object on first ToString call.
    /// </summary>
    public struct LazySerializer
    {
        #region Static.
        static JsonSerializerSettings JsonSerializerSetting { get; } = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// GetType used, not typeof(TValue).
        /// </summary>
        public static string DefaultSerializer<TValue>(TValue value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSetting);
        }
        #endregion
    }
}
