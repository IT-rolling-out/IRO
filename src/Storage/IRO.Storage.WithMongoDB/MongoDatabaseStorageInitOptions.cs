namespace IRO.Storage.WithMongoDB
{
    public class MongoDatabaseStorageInitOptions
    {
        /// <summary>
        /// Default is 'key_value_collection'.
        /// </summary>
        public string CollectionName { get; set; } = "key_value_collection";

        /// <summary>
        /// Default is true.
        /// <para></para>
        /// Use cache for much performanse. Default is RamCacheStorage.
        /// <para></para>
        /// Disable it for crossprocess database, because cache threadsafe only in process.
        /// </summary>
        public bool UseCache { get; set; } = true;

        /// <summary>
        /// Default is 10000.
        /// </summary>
        public int CacheRecordsLimit { get; set; } = 10000;
    }
}