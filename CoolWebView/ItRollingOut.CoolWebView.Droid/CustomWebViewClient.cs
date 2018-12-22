using Android.Graphics;
using Android.Runtime;
using Android.Webkit;
using S2A.Plugins.EmbededSharedResources;
using System;

namespace S2A.Plugins.WebViewSuite.Droid
{
    /// <summary>
    /// Настройка браузера.
    /// </summary>
    public class CustomWebViewClient : WebViewClient
    {
        AndroidWebViewWrap _webViewWrap;
        LoadStartedEventArgs _lastLoadStartedArgs;
        LoadFinishedEventArgs _errorLoadArgs;
        bool _oldShouldOverrideUrlLoadingWorks = false;

        public CustomWebViewClient(AndroidWebViewWrap webViewWrap)
        {
            _webViewWrap = webViewWrap;
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);

            

            var args = _errorLoadArgs
                ?? new LoadFinishedEventArgs()
                {
                    Url = url,
                    IsError = false,
                };
            _errorLoadArgs = null;
            _webViewWrap.OnPageLoadFinished(args);

            _webViewWrap.ToogleProgressBar(false);
            _webViewWrap.WebView.Visibility = Android.Views.ViewStates.Visible;
        }

        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            //Нужно скрыть wv, иначе круговой програссбар будет отображаться над страницей.
            //Для полосы это не проблема.
            if (_webViewWrap.Settings.ProgressBarStyle == ProgressBarStyle.Circular)
                _webViewWrap.WebView.Visibility = Android.Views.ViewStates.Invisible;
            _webViewWrap.ToogleProgressBar(true);           
            
            OnPageStarted(url); 
            base.OnPageStarted(view, url, favicon);
        }

        [Obsolete]
        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            _oldShouldOverrideUrlLoadingWorks = true;
            if (_lastLoadStartedArgs.Handled)
            {
                OnShouldLoad(view);
                return true;
            }
            return base.ShouldOverrideUrlLoading(view, url);
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            if (!_oldShouldOverrideUrlLoadingWorks && _lastLoadStartedArgs.Handled)
            {
                OnShouldLoad(view);
                return true;
            }            
            return base.ShouldOverrideUrlLoading(view, request);
        }

        void OnPageStarted(string url)
        {
            _lastLoadStartedArgs = new LoadStartedEventArgs()
            {
                Url = url
            };
            _webViewWrap.OnPageLoadStarted(_lastLoadStartedArgs);
        }

        void OnShouldLoad(WebView view)
        {
            //Исключительный случай внедрения js.
            view.EvaluateJavascript(
                EmbededSharedResourcesLoader.LoadPolifillJs(),
                null
                );
        }

        [Obsolete]
        public override void OnReceivedError(WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            //Вроде как этот метод работает до апи 23, а в последующих работает второй OnReceivedError.
            //Не уверен что он срабатывает только для страниц, нужно тестирование.
            if (errorCode==ClientError.Connect && !failingUrl.Contains("favicon"))
            {
                _errorLoadArgs = new LoadFinishedEventArgs()
                {
                    Url = failingUrl,
                    IsError = true,
                    ErrorDescription = description
                };
            }
            else
                base.OnReceivedError(view, errorCode, description,failingUrl);
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (error.ErrorCode==ClientError.Connect && request.IsForMainFrame)
            {
                _errorLoadArgs = new LoadFinishedEventArgs()
                {
                    Url = request.Url.ToString(),
                    IsError = true,
                    ErrorDescription = error.Description
                };
            }
            else
                base.OnReceivedError(view, request, error);
        }

        public override void OnLoadResource(WebView view, string url)
        {
            base.OnLoadResource(view, url);
        }

        //Will remade this metod later.
        //Method for opening speecial app for urls.
        //public override bool ShouldOverrideUrlLoading(WebView view, string url)
        //{
        //    Messages.ShowToast("shouldOverrideUrlLoading: " + url);
        //    Intent intent;
        //    intent = new Intent(Intent.ActionView);
        //    intent.SetData(Android.Net.Uri.Parse(url));
        //    var context = Application.Context;
        //    var ra = intent.ResolveActivityInfo(context.PackageManager, PackageInfoFlags.Providers|PackageInfoFlags.Receivers);
        //    if (ra != null)
        //    {
        //        Messages.ShowToast(ra.ToString());
        //        Messages.ShowToast(ra.ApplicationInfo.ToString());
        //        Messages.ShowToast(ra.LaunchMode.ToString());
        //        Messages.ShowToast(ra.ProcessName);
        //        Messages.ShowToast(ra.Name);
        //        //Messages.ShowToast();
        //        context.StartActivity(intent);
        //        return true;
        //    }
        //    return false;
        //}
    }
}