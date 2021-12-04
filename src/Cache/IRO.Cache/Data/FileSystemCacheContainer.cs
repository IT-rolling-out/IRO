using System;

namespace IRO.Cache.Data
{
    internal struct FileSystemCacheContainer
    {
        public DateTime? ExpiresIn { get; set; }

        public string FilePath { get; set; }
    }
}