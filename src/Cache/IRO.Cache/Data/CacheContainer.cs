using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.Cache
{
    internal struct RamCacheContainer
    {
        public DateTime? ExpiresIn { get; set; }

        public byte[] SerializedValue { get; set; }
    }
}
