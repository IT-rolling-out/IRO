namespace IRO.CmdLine
{
    /// <summary>
    /// Опции для получения ресурсов из thiss.
    /// </summary>
    public class ReadResourseOptions
    {
        /// <summary>
        /// Указывает нужно ли сохранят ресурс в кеш для последующего автозаполнения.
        /// </summary>
        public bool SaveToCache { get; set; } = true;
    }
}
