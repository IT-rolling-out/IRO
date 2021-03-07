using System;
using System.IO;

namespace IRO.Storage.Data
{
    public class FileStorageInitOptions
    {
        /// <summary>
        /// Default is '{BaseDirectory}/localstorage.json'.
        /// </summary>
        public string StorageFilePath { get; set; }
            = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localstorage.json");
    }
}