using System;
using System.IO;

namespace IRO.Storage.WithLiteDB
{
    public class LiteDatabaseStorageInitOptions
    {
        /// <summary>
        /// Default is 'key_value_collection'.
        /// </summary>
        public string CollectionName { get; set; } = "key_value_collection";

        /// <summary>
        /// Default is '{BaseDirectory}/localstorage.litedb'.
        /// </summary>
        public string DbFilePath { get; set; }
            = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localstorage.litedb");

        /// <summary>
        /// Default is true.
        /// <para></para>
        /// Use cache for much performanse. Default is RamCacheStorage.
        /// <para></para>
        /// Disable it for crossprocess database, because cache threadsafe only in process.
        /// </summary>
        public bool UseCache { get; set; } = true;
    }
}