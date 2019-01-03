using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IRO.CmdLine.Json
{
    /// <summary>
    /// Из старой библиотеки IRO.Json, лучше не использовать.
    /// </summary>
    class JsonSerializeOptions
    {
        public bool WithNormalFormating { get; set; }
        public bool IgnoreDefaultValues { get; set; }
    }
}
