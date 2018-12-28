using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ItRollingOut.Cross.ImprovedWebView.Plugins;

namespace ItRollingOut.Cross.ImprovedWebView
{
    public interface IWebViewWrap
    {
    
    }

    public abstract class WebViewWrap:IDisposable
    {
        public bool IsDisposed { get; private set; }

        public event Action Disposed;

        public void Dispose()
        {
            Url = null;
            SitesWithJsBridge = null;
            foreach(var plug in Plugins)
            {
                plug.OnFinish(this);
            }

            IsDisposed = true;
            Disposed?.Invoke();
        }

        /// <summary>
        /// Функция, которая будет вызвана после загрузки страницы (включая все плагины и встроенные скрипты).
        /// Используйте это, если хотите гарантировать полную загрузку плагинов.
        /// </summary>
        public const string OnPageTotallyLoadedFunctionName = "OnPageTotallyLoaded";

        public const string LowLevelJsObjectName = "NativeMessages";

        public WrapSettings Settings { get; private set; }

        public string Url { get; private set; }

        protected JsMessagingSystem JsMessaging { get;  private set;}

        public bool JsMessagingEnabledOnCurrentPage{ get;  private set;}

        /// <summary>
        /// Сайты на которых разрешено вызывать c# код из Js.
        /// Все сайты по-умолчанию.
        /// </summary>
        public IReadOnlyCollection<Regex> SitesWithJsBridge { get;private set; }

        /// <summary>
        /// Порядок плагинов в списке может влиять на результат, т.к. некоторые плагины могут конфликтовать.
        /// </summary>
        public IReadOnlyCollection<IWebViewWrapPlugin> Plugins { get; private set; }

        /// <summary>
        /// Получает объект, которым оперирует WebViewWrap. Это не обезательно WebView,
        /// может быть какой-то StackLayout с самим WebView, ProgressBar`ом и прочими компонентами.
        /// </summary>
        public object NativeView { get; private set; }

        /// <summary>
        /// Вернет именно WebView.
        /// </summary>
        public object NativeWebView { get; private set; }

        #region Events region
        /// <summary>
        /// Реагирует на редиректы.
        /// </summary>
        public event PageLoadStartedDelegate PageLoadStarted;

        public event PageLoadFinishedDelegate PageLoadFinished;

        public event BackButtonPressedDelegate BackButtonPressed;
        #endregion

        #region Private data
        readonly List<string> _includedJs = new List<string>();

        const string DebugStepStr = "WebViewWrap debug step -> '{0}'.";

        readonly object _pageFinishedSync_Locker = new object();

        bool _lastLoadWasHandled = false;

        PageLoadFinishedDelegate _pageFinishedSync_EventHandler;

        TaskCompletionSource<LoadFinishedEventArgs> _pageFinishedSync_TaskCompletionSource;
        #endregion

        /// <summary>
        /// Если null, используются настройки ядра.
        /// </summary>
        /// <param name="wrapInitConfigs"></param>
        public WebViewWrap(WrapInitConfigs wrapInitConfigs=null)
        {
            Init(wrapInitConfigs);
        }

        /// <summary>
        /// Добавляет скрипты, которые выполнятся сразу ПОСЛЕ загрузки страницы.
        /// </summary>
        public void AddIncludedJs(string jsStringProvider)
        {
            _includedJs.Add(jsStringProvider);
        }

        /// <summary>
        /// Дает возможность вызывать методы и свойства этого объекта из js.
        /// Все в соответствии с RollingOutTools.ReflectionVisit, читайте документацию.
        /// </summary>
        public void AddJsInterface(string objName, IJsInterface jsInterface)
        {
            AddJsInterface(objName, (object)jsInterface);
        }

        public void AddJsInterface(string objName, object jsInterface)
        {
            var newJsProxy=JsMessaging.AddJsMessagingProxy(objName, jsInterface);
            if(jsInterface is IJsInterface)
                ((IJsInterface)jsInterface).OnLoaded(this);
            AddIncludedJs(newJsProxy.GetJsProxyObject());

        }

        public Task<LoadFinishedEventArgs> Load(string url)
        {
            var resTask = CreatePageLoadFinishedTask(
                () =>
                {
                    StartLoading(url);
                }
                );
            return resTask;
        }

        public Task<LoadFinishedEventArgs> LoadData(string data, string baseUrl = "about:blank")
        {
            var resTask = CreatePageLoadFinishedTask(
                () =>
                {
                    StartLoadingData(data, baseUrl);
                }
                );
            return resTask;
        }

        protected abstract void StartLoadingData(string data, string baseUrl);

        protected abstract void StartLoading(string url);

        /// <summary>
        /// В большинстве случаев (если не произошла ошибка) вернет json строку.
        /// </summary>
        public abstract Task<object> ExJs(string script,int? timeoutMS=null);

        public abstract bool CanGoForward();

        public abstract void GoForward();

        public abstract bool CanGoBack();

        public abstract void GoBack();        

        #region Must be called in inherit class or it`s services.
        /// <summary>
        /// Должен быть вызван после получения WebView.
        /// </summary>
        public virtual void SetNativeViews(object webView, object rendererView)
        {
            NativeView = rendererView;
            NativeWebView = webView;
            InitPlugins();
        }

        public void OnPageLoadStarted(LoadStartedEventArgs args)
        {
            JsMessagingEnabledOnCurrentPage = CheckJsBridgeAllowed(args.Url);
            args.IsBridgeEnabled = JsMessagingEnabledOnCurrentPage;
            Url = args.Url;
            //Вызов событий перед первым await, т.к. они должны выполниться синхронно.
            LogStepIfDebug("Page load started");
            foreach(var plugin in Plugins)
            {
                plugin.OnPageLoadStarted(this,args);
            }
            LogStepIfDebug("Executed all plugins OnPageLoadStarted");
            PageLoadStarted?.Invoke(this, args);
            LogStepIfDebug("OnPageLoadStarted events invoked");

            //Отмена загрузки страницы производится на более низком уровне.
            //Для андроид это ShouldOverrideUrlLoading.
            //А эта переменная сохраняет значение для OnPageLoadFinished.
            _lastLoadWasHandled = args.Handled;
        }

        public async void OnPageLoadFinished(LoadFinishedEventArgs args)
        {
            args.IsBridgeEnabled = JsMessagingEnabledOnCurrentPage;
            args.WasHandled = _lastLoadWasHandled;
            await LoadIncludedJs();
            foreach (var plugin in Plugins)
            {
                await plugin.OnPageLoadFinished(this, args);
            }
            LogStepIfDebug("Executed all plugins OnPageLoadFinished");

            OnPageTotallyLoaded(args);
        }

        /// <summary>
        /// Вернет истину, если стандартную обработку стоит отменить.
        /// </summary>
        public void OnBackButtonPressed(BackButtonPressedEventArgs args)
        {
            BackButtonPressed?.Invoke(this,args);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Этот код гарантированно выполнится после загрузки страницы и всех плагинов.
        /// </summary>
        async void OnPageTotallyLoaded(LoadFinishedEventArgs args)
        {
            await ExJs(OnPageTotallyLoadedFunctionName+"();");
            PageLoadFinished?.Invoke(this,args);
            LogStepIfDebug("Page totally finished");            
        }

        void Init(WrapInitConfigs wrapInitConfigs)
        {
            wrapInitConfigs = wrapInitConfigs ?? WrapCore.DefaultInitConfigs ?? new WrapInitConfigs();

            SitesWithJsBridge=wrapInitConfigs.CloneSitesWithJsBridge();           
            Settings = wrapInitConfigs.Settings;
            Plugins = wrapInitConfigs.ClonePlugins() ?? new List<IWebViewWrapPlugin>();

            //Скрипт для поддержки колбеков при вызове нативных методов.
            AddIncludedJs(
                JsMessagingProxy.GetImprovedSendFunc()
                );

            JsMessaging = new JsMessagingSystem(this);
            var jsInterfaces = wrapInitConfigs.CloneJsInterfaces();
            foreach (var item in jsInterfaces)
            {
                AddJsInterface(
                    item.Key,
                    item.Value
                );
            }

            if (wrapInitConfigs.IncludedJs != null)
            {
                foreach (var item in wrapInitConfigs.IncludedJs)
                {
                    AddIncludedJs(item);
                }
            }
                    
        }

        void InitPlugins()
        {
            var plugins = Plugins;
            if (plugins == null)
                return;
            foreach (var plug in plugins)
            {
                plug.OnCreate(this);
            }
        }

        async Task LoadIncludedJs()
        {
            LogStepIfDebug("Load included js started");
            foreach (var str in _includedJs)
            {
                string jsScript = str;
                await ExJs(jsScript);
            }
            LogStepIfDebug("Load included js finished");
        }

        bool CheckJsBridgeAllowed(string url)
        {
            if (SitesWithJsBridge?.Any() != true)
                return false;
            foreach (var regex in SitesWithJsBridge)
            {
                if (regex.IsMatch(url))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///Вызывается для удаления ссылок на обработчик события и промиса.
        ///Если к моменту вызова не была завершена загрузка предыдущей страницы, то таск будет отменен.
        /// </summary>
        void TryCancelPageFinishedTask()
        {
            if (_pageFinishedSync_EventHandler != null)
                PageLoadFinished -= _pageFinishedSync_EventHandler;
            _pageFinishedSync_TaskCompletionSource?.TrySetCanceled();
            _pageFinishedSync_EventHandler = null;
            _pageFinishedSync_TaskCompletionSource = null;
        }

        Task<LoadFinishedEventArgs> CreatePageLoadFinishedTask(Action loadStarter)
        {
            //Локкер нужен для того, чтоб обязательно вернуть нужный таск из метода, даже если он сразу будет отменен.
            lock (_pageFinishedSync_Locker)
            {
                TryCancelPageFinishedTask();
                _pageFinishedSync_TaskCompletionSource = new TaskCompletionSource<LoadFinishedEventArgs>();
                _pageFinishedSync_EventHandler = async (s, a) =>
                {
                    _pageFinishedSync_TaskCompletionSource.TrySetResult(a);
                };
                PageLoadFinished += _pageFinishedSync_EventHandler;
                loadStarter();
                return _pageFinishedSync_TaskCompletionSource.Task;
            }
        }

        internal static void LogStepIfDebug(string text)
        {
#if DEBUG
            Debug.WriteLine(string.Format(DebugStepStr.ToUpper(), text));
#endif
        }        
        #endregion
    }
}
