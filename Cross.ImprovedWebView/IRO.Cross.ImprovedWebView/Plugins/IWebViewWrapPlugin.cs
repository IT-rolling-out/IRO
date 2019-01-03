using System.Threading.Tasks;

namespace IRO.Cross.ImprovedWebView.Plugins
{
    public interface IWebViewWrapPlugin
    {
        void OnCreate(WebViewWrap webViewWrap);

        void OnFinish(WebViewWrap webViewWrap);

        /// <summary>
        /// Этот метод ничем не отличается от события, но здесь проще с управлением памятью.
        /// Просто он обязан быть синхронным.
        ///<para></para> 
        /// Обратите внимание на параметр Handled. Плагин сам должен решать как на него реагировать (чаще всего отмена, если истина).
        /// </summary>
        void OnPageLoadStarted(WebViewWrap webViewWrap, LoadStartedEventArgs loadStartedEventArgs);

        /// <summary>
        /// Лучше использовать этот метод, чем события, т.к. он возвращает таск и можно удобно
        /// организовать асинхронный код.
        /// <para></para> 
        /// Обратите внимание на параметр WasHandled. Плагин сам должен решать как на него реагировать (чаще всего отмена, если истина).
        /// </summary>
        Task OnPageLoadFinished(WebViewWrap webViewWrap, LoadFinishedEventArgs loadFinishedEventArgs);
    }
}
