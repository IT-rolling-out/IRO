using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Util.Jar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S2A.Plugins.WebViewSuite.Droid
{
    public class WebViewRenderer : RelativeLayout
    {       
        public WebView CurrentWebView { get; private set; }
        string _url;

        //ViewStub webViewStub;
        ProgressBar linearProgressBar;
        ProgressBar circularProgressBar;
        bool webViewInflated = false;

        public WebViewRenderer(Context context) : base(context)
        {
            Init();
        }

        public WebViewRenderer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public WebViewRenderer(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,defStyleAttr)
        {
            Init();
        }

        void Init()
        {
            View rootView = Inflate(Context, Resource.Layout.webview_suite, this);

            CurrentWebView = (WebView)rootView.FindViewById(Resource.Id.just_web_view);
            //webViewStub = (ViewStub)rootView.FindViewById(Resource.Id.webview_stub);
            linearProgressBar = (ProgressBar)rootView.FindViewById(Resource.Id.linear_progressbar);
            circularProgressBar = (ProgressBar)rootView.FindViewById(Resource.Id.circular_progressbar);

            ToogleProgressBar(ProgressBarStyle.None);
            
            WebViewInflated();
        }

        /// <summary>
        /// Выполнится после инициализации WebView.
        /// </summary>
        void WebViewInflated()
        {
            webViewInflated = true;
            ApplyDefaultSettings(CurrentWebView);

            if (CurrentWebView == null)
                return;
            if (_url != null && !string.IsNullOrEmpty(_url))
            {
                CurrentWebView.LoadUrl(_url);
            }
        }

        /// <summary>
        /// Нужно использовать этот метод для корректной работы навигации.
        /// </summary>
        internal void StartLoading(string url)
        {
            _url = url;
            if (!webViewInflated || CurrentWebView == null)
                return;
            CurrentWebView.LoadUrl(_url);
        }

        /// <summary>
        /// Нужно использовать этот метод для корректной работы навигации.
        /// </summary>
        internal void StartLoadingData(string data, string baseUrl)
        {
            _url = baseUrl;
            if (!webViewInflated || CurrentWebView == null)
                return;
            CurrentWebView.LoadDataWithBaseURL(
                baseUrl,
                data, 
                "text/html",
                "UTF-8",
                CurrentWebView.Url
                );
        }

        /// <summary>
        /// Настройка WebView, которые не требуют WrapSettings.
        /// Будут применены сразу после инициализации WebView.
        /// </summary>
        void ApplyDefaultSettings(WebView wv)
        {
            wv.Settings.JavaScriptEnabled = true;

#if DEBUG
            WebView.SetWebContentsDebuggingEnabled(true);
#endif
     
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                CookieManager.Instance.SetAcceptThirdPartyCookies(wv, true);
            }
            else
            {
                CookieManager.Instance.SetAcceptCookie(true);
            }

            wv.Settings.SetPluginState(WebSettings.PluginState.On);
            try
            {
                wv.Settings.PluginsEnabled = true;                
            }
            catch { }
            wv.Settings.LoadWithOverviewMode = true;
            //wv.Settings.UseWideViewPort = true;
            wv.Settings.DefaultZoom = WebSettings.ZoomDensity.Far;            
            wv.Settings.DisplayZoomControls = false;
            wv.Settings.AllowContentAccess = true;
            wv.Settings.DomStorageEnabled = true;
            wv.Settings.JavaScriptCanOpenWindowsAutomatically = true;
            wv.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            wv.Settings.SavePassword = true;
            wv.Settings.MediaPlaybackRequiresUserGesture = false;
            try
            {
                //Обычно не работает, нужно задать в манифесте.
                wv.Settings.SafeBrowsingEnabled = false;
            }
            catch { }
            wv.Settings.DatabaseEnabled = true;
            

            //Подмена user-agent нужна чтоб избежать ограничений от некоторых сайтов.
            string androidVersion = Build.VERSION.Release;
            wv.Settings.UserAgentString = $"Mozilla/5.0 (Linux; Android {androidVersion}; m2 Build/LMY47D) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/66.0.3359.158 Mobile Safari/537.36";
        }

        internal void InitWebViewCaching(WebView wv, string cacheDirectory)
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

        internal void ToogleProgressBar(ProgressBarStyle progressBarStyle)
        {
            switch (progressBarStyle)
            {
                case ProgressBarStyle.Circular:
                    linearProgressBar.Visibility = ViewStates.Gone;
                    circularProgressBar.Visibility = ViewStates.Visible;
                    break;
                case ProgressBarStyle.None:
                    linearProgressBar.Visibility = ViewStates.Gone;
                    circularProgressBar.Visibility = ViewStates.Gone;
                    break;
                case ProgressBarStyle.Linear:
                    linearProgressBar.Visibility = ViewStates.Visible;
                    circularProgressBar.Visibility = ViewStates.Gone;
                    break;
            }
        }
        
        
    }
}
