namespace S2A.Plugins.WebViewSuite
{
    public struct WrapSettings
    {
        public PermissionsMode PermissionsMode { get; set; }   

        public bool DownloadsEnabled { get; set; }

        public bool UploadsEnabled { get; set; }

        public bool ZoomEnabled { get; set; } 

        public string CacheFolder { get; set; }

        /// <summary>
        /// Если истина, то будут добавлены визуальные элементы.
        /// </summary>
        public bool AttachVisualElements { get; set; }

        public ProgressBarStyle ProgressBarStyle { get; set; } 
    }
}
