using System;

namespace IRO.Cache
{
    internal struct FileSystemCacheContainer
    {
        public DateTime? ExpiresIn { get; set; }

        public string FilePath { get; set; }
    }
}