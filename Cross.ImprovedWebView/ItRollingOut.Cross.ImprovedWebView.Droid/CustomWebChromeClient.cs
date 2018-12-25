using System;
using Android.Content;
using Android.Webkit;

namespace ItRollingOut.Cross.ImprovedWebView.Droid
{
    public class CustomWebChromeClient:WebChromeClient
    {
        public const int RequestSelectFile = 3412;
        int fileCounter = 0;

        AndroidWebViewWrap _webViewWrap;

        public CustomWebChromeClient(AndroidWebViewWrap webViewWrap)
        {
            _webViewWrap = webViewWrap;
        }

        /// <summary>
        /// Обработчик отправки файлов в браузере.
        /// </summary>
        public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            if (_webViewWrap.Settings.UploadsEnabled)
                return false;

            try
            {
                Intent intent = fileChooserParams.CreateIntent();
                ActivityExtensions.StartActivityAndReturnResult(
                    intent
                    )
                .ContinueWith((t) =>
                {
                    try
                    {
                        var resultArgs = t.Result;
                        var uriArr = FileChooserParams.ParseResult(
                            Convert.ToInt32(resultArgs.ResultCode),
                            resultArgs.Data
                            );
                        filePathCallback?.OnReceiveValue(uriArr);
                    }
                    catch (Exception ex)
                    {
                        AllAnalytics.Inst.TryLogException(ex, "CallbackOfFileChooser");
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#endif
                AllAnalytics.Inst.TryLogException(ex, "FileChooser");
                Srv.Messages.ShowToast("Cannot open file chooser.");
                return false;
            }
        }
    }
}