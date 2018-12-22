using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace S2A.Plugins.WebViewSuite
{
    public class JsMessagingSystem:IDisposable
    {
        #region Exceptions
        /// <summary>
        /// Лог ошибок всегда доступен в окне отладки.
        /// ВНИМАНИЕ! Используйте только в режиме отладки, иначе любой сайт сможет сломать ваше приложение.
        /// </summary>
        public static bool ThrowExceptions { get; set; } = false;

        public static event Action<Exception> ExceptionRised;

        internal static void OnException(Exception ex)
        {
            ExceptionRised?.Invoke(ex);
#if DEBUG
            Debug.WriteLine($"JS MESSAGING SYSTEM EXCEPTION\n{ex.ToString()}");
#endif
            if (JsMessagingSystem.ThrowExceptions)
                throw ex;            
        }
        #endregion

        WebViewWrap _wvw;

        Dictionary<string, JsMessagingProxy> _objNameAndJsInterface = new Dictionary<string, JsMessagingProxy>();

        public IReadOnlyDictionary<string, JsMessagingProxy> ObjNameAndJsInterface =>_objNameAndJsInterface;        

        public JsMessagingSystem(WebViewWrap wvw)
        {
            _wvw = wvw;
        }

        public JsMessagingProxy AddJsMessagingProxy(string objName, object jsInterface)
        {
            var newJsProxy = new JsMessagingProxy(_wvw, objName, jsInterface);
            _objNameAndJsInterface.Add(
                objName,
                newJsProxy
                );
            return newJsProxy;
        }

        public async Task SendJsMessageAsync(string messageName, string sendedObjectJson, string resolveFunctionName, string rejectFunctionName)
        {
            var objName = messageName.Split('.')[0];
            var jsProxy = _objNameAndJsInterface[objName];
            await jsProxy.SendJsMessageAsync(messageName, sendedObjectJson, resolveFunctionName, rejectFunctionName);
        }

        /// <summary>
        /// Вернет результат выполнения в виде json.
        /// </summary>
        public string SendJsMessageSync(string messageName, string sendedObjectJson)
        {
            var objName = messageName.Split('.')[0];
            var jsProxy = _objNameAndJsInterface[objName];
            return jsProxy.SendJsMessageSync(messageName, sendedObjectJson);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
