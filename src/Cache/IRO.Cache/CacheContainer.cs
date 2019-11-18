using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.Cache
{
    internal struct CacheContainer
    {
        public DateTime? ExpiresIn { get; set; }

        public string SerializedValue { get; set; }
    }
}
