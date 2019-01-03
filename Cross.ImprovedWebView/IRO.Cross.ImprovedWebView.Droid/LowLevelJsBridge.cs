using Android.Webkit;
using Java.Interop;

namespace IRO.Cross.ImprovedWebView.Droid
{
    class LowLevelJsBridge: Java.Lang.Object
    {
        AndroidWebViewWrap _wvw;
        JsMessagingSystem _jsMessagingSystem;

        public LowLevelJsBridge(AndroidWebViewWrap wvw,JsMessagingSystem jsMessagingSystem)
        {         
            _wvw = wvw;
            _jsMessagingSystem = jsMessagingSystem;
        }

        [Export]
        [JavascriptInterface]
        public async void SendAsync(string messageName, string sendedObjectJson, string resolveFunctionName, string rejectFunctionName)
        {
            if (_wvw.JsMessagingEnabledOnCurrentPage)
                await _jsMessagingSystem.SendJsMessageAsync(messageName, sendedObjectJson, resolveFunctionName, rejectFunctionName);
        }

        [Export]
        [JavascriptInterface]
        public string SendSync(string messageName, string sendedObjectJson)
        {
            if (_wvw.JsMessagingEnabledOnCurrentPage)
                return _jsMessagingSystem.SendJsMessageSync(messageName, sendedObjectJson);
            return "null";
        }
    }
}