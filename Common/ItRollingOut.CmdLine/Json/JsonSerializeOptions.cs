using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.CmdLine.Json
{
    /// <summary>
    /// Из старой библиотеки ItRollingOut.Json, лучше не использовать.
    /// </summary>
    class JsonSerializeOptions
    {
        public bool WithNormalFormating { get; set; }
        public bool IgnoreDefaultValues { get; set; }
    }
}
