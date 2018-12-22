using S2A.Plugins.WebViewSuite.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace S2A.Plugins.WebViewSuite
{
    /// <summary>
    /// Изначально на этот класс возлагалась гораздо большая роль, но теперь он скорее просто набор стандартных настроек WebViewWrap.
    /// </summary>
    public static class WrapCore
    {
        public static WrapInitConfigs DefaultInitConfigs { get; set; } 
    }
}
