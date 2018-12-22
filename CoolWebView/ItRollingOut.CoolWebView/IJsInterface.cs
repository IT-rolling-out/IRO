using System;
using System.Collections.Generic;
using System.Text;

namespace S2A.Plugins.WebViewSuite
{
    public interface IJsInterface
    {
        void OnLoaded(WebViewWrap webViewWrap);
    }
}
