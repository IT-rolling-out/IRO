using Android.App;
using Android.Webkit;
using S2A.Plugins.Analytics;
using ItRollingOut.Xamarin;
using System;

namespace S2A.Plugins.WebViewSuite.Droid
{
    public class CustomDownloadListener :Java.Lang.Object, IDownloadListener
    {
        AndroidWebViewWrap _webViewWrap;

        public CustomDownloadListener(AndroidWebViewWrap webViewWrap)
        {
            _webViewWrap = webViewWrap;
        }

        public void OnDownloadStart(string url, string userAgent, string contentDisposition, string mimetype, long contentLength)
        {
            if (_webViewWrap.Settings.DownloadsEnabled)
                return;

            try
            {

                DownloadManager.Request request = new DownloadManager.Request(
                    Android.Net.Uri.Parse(url)
                    );

                request.AllowScanningByMediaScanner();
                request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                string fileName = new ContentDisposition(contentDisposition).FileName.Trim('\"');
                request.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, fileName);
                DownloadManager dm = (DownloadManager)Application.Context.GetSystemService(Application.DownloadService);
                dm.Enqueue(request);
                Srv.Messages.ShowToast("Downloading File");
            }
            catch(Exception ex)
            {
                AllAnalytics.Inst.TryLogException(ex, "CustomDownloadListener");

            }
        }
    }
}