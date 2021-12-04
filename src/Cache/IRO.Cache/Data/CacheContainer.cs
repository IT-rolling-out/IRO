using System;

namespace IRO.Cache.Data
{
    internal struct RamCacheContainer
    {
        public DateTime? ExpiresIn { get; set; }

        public byte[] SerializedValue { get; set; }
    }
}
