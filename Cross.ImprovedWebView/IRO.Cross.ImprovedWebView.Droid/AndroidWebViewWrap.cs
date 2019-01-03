using System;
using System.IO;
using System.Threading.Tasks;
using Android.Views;
using Android.Webkit;

namespace IRO.Cross.ImprovedWebView.Droid
{
    public class AndroidWebViewWrap : WebViewWrap
    {
        public WebViewRenderer WebViewRenderer { get => (NativeView as WebViewRenderer); }

        public WebView WebView { get => (NativeWebView as WebView); }

        public AndroidWebViewWrap(WrapInitConfigs wrapInitConfigs=null) 
            : base(wrapInitConfigs)
        {            
        }

        public AndroidWebViewWrap()
        {
        }

        public override void SetNativeViews(object webView, object rendererView)
        {
            base.SetNativeViews(webView, rendererView);
            ApplyWrapSettings();
        }

        public override async Task<object> ExJs(string script, int? timeoutMS)
        {
            var scriptUpd = "JSON.stringify((function(){" + script + "})());";
            var jsResult = await WebViewRenderer.CurrentWebView.ExJsWithResult(script,timeoutMS);
            return jsResult;
        }

        protected override void StartLoading(string url)
        {
            WebViewRenderer.StartLoading(url);
        }

        protected override void StartLoadingData(string data, string baseUrl)
        {
            WebViewRenderer.StartLoadingData(data,baseUrl);
        }

        public override bool CanGoForward()
        {
            return WebViewRenderer.CurrentWebView.CanGoForward();
        }

        public override void GoForward()
        {
            WebViewRenderer.CurrentWebView.GoForward();
        }

        public override bool CanGoBack()
        {
            return WebViewRenderer.CurrentWebView.CanGoBack();
        }

        public override void GoBack()
        {
            WebViewRenderer.CurrentWebView.GoBack();
        }

        #region WebView part
        void AddJsInterface(WebView wv)
        {
            var obj = new LowLevelJsBridge(this,JsMessaging);
            wv.AddJavascriptInterface(obj, LowLevelJsObjectName);

        }

        /// <summary>
        /// Настройка WebView, которые требуют данных от WebViewWrap
        /// </summary>
        void ApplyWrapSettings()
        {
            var wv = WebView;
            var webViewRenderer = WebViewRenderer;

            AddJsInterface(wv);

            InitWebViewCaching(wv, Settings.CacheFolder);

            wv.Settings.BuiltInZoomControls = Settings.ZoomEnabled;

            wv.SetWebChromeClient(new CustomWebChromeClient(this));
            wv.SetWebViewClient(new CustomWebViewClient(this));

            if (Settings.PermissionsMode == PermissionsMode.AllowedAll)
            {
                wv.Settings.AllowFileAccess = true;
                wv.Settings.AllowFileAccessFromFileURLs = true;
                wv.Settings.AllowUniversalAccessFromFileURLs = true;

            }

            // Добавляет настройки, которые необходимы только для отображаемых WebView.
            if (Settings.AttachVisualElements)
            {
                webViewRenderer.ToogleProgressBar(Settings.ProgressBarStyle);
                UseBackButtonCrunch(wv);
                wv.SetDownloadListener(new CustomDownloadListener(this));
            }
        }

        /// <summary>
        /// Костыль для возврата на предыдущую страницу. 
        /// Если невозможно вернуться, то двойно так воспринимается как команда к выходу их приложения.
        /// </summary>
        void UseBackButtonCrunch(WebView wv)
        {
            int backTaps = 0;
            int wantToQuitApp = 0;
            var ev = new EventHandler<View.KeyEventArgs>((s, e) =>
            {
                if (e.KeyCode == Keycode.Back)
                {
                    e.Handled = true;
                    if (backTaps > 0)
                    {
                        //wantToQuitApp используется для двух попыток нажать назад перед оконсчательной установкой, что нельзя идти назад.
                        //Просто баг в WebView.
                        var canGoBack =  wv.CanGoBack() && wantToQuitApp > 0;
                        var args = new BackButtonPressedEventArgs();
                        args.CanGoBack = canGoBack;
                        OnBackButtonPressed(args);
                        if (args.Cancel)
                            return;

                        if (canGoBack)
                        {
                            wantToQuitApp = 0;
                            backTaps = 0;
                            wv.GoBack();
                        }
                        else
                        {
                            wantToQuitApp++;
                        }
                    }
                    else
                    {
                        backTaps++;
                    }
                }

            });
            wv.KeyPress += ev;
        }

        void InitWebViewCaching(WebView wv, string cacheDirectory)
        {
            wv.Settings.CacheMode = CacheModes.Normal;
            wv.Settings.SetAppCacheMaxSize(100 * 1024 * 1024);
            wv.Settings.SetAppCacheEnabled(true);
            try
            {
                //string cacheDirectory = System.IO.Path.Combine(
                //    Wrap.Settings.CacheFolder,
                //    "br_cache"
                //    );
                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }
                wv.Settings.SetAppCachePath(cacheDirectory);
            }
            catch { }
        }

        public void ToogleProgressBar(bool isVisible)
        {
            
            if (isVisible)
            {
                WebViewRenderer.ToogleProgressBar(Settings.ProgressBarStyle);
            }
            else
            {
                WebViewRenderer.ToogleProgressBar(ProgressBarStyle.None);
            }
        }
        #endregion
    }
}