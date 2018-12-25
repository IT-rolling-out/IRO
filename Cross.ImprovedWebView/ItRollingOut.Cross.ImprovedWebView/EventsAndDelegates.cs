using System;

namespace ItRollingOut.Cross.ImprovedWebView
{
    public delegate void PageLoadStartedDelegate(WebViewWrap sender, LoadStartedEventArgs args);

    public delegate void PageLoadFinishedDelegate(WebViewWrap sender, LoadFinishedEventArgs args);

    public class LoadEventArgs:EventArgs
    {
        public string Url { get; set; }

        /// <summary>
        /// Показывает включен ли мост к нативным функциям на этой странице.
        /// </summary>
        public bool IsBridgeEnabled{ get; set; }
    }

    public class LoadFinishedEventArgs : LoadEventArgs
    {
        /// <summary>
        /// Только если ошибка подключения.
        /// </summary>
        public bool IsError { get; set; }

        public string ErrorDescription { get; set; }

        /// <summary>
        /// Если истина, то контент страницы не был автоматически загружен.
        /// </summary>
        public bool WasHandled { get; set; }
    }

    public class LoadStartedEventArgs : LoadEventArgs
    {
        /// <summary>
        /// Если истина, то контент страницы не будет загружен автоматически, но все события отработают как обычно.
        /// </summary>
        public bool Handled { get; set; }
    }

    public delegate void BackButtonPressedDelegate(WebViewWrap sender, BackButtonPressedEventArgs args);

    public class BackButtonPressedEventArgs : EventArgs
    {
        /// <summary>
        /// Установите значение true, чтобы отменить стандартный обработчик.
        /// Если в стеке уже не осталось страниц для возврата, то стандартной обработки не будет.
        /// </summary>
        public bool Cancel { get; set; }

        public bool CanGoBack { get; set; }
    }
}
