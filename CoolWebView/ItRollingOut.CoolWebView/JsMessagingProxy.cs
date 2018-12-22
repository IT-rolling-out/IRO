using Newtonsoft.Json;
using RollingOutTools.ReflectionVisit;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace S2A.Plugins.WebViewSuite
{
    public class JsMessagingProxy:IDisposable
    {
        #region Improved send funcs
        const string ImprovedSend_AsyncFuncName = "sa";

        static string _improvedSend_AsyncFunc;

        const string ImprovedSend_SyncFuncName = "ss";

        static string _improvedSend_SyncFunc;

        static string _improvedSend;

        public static string GetImprovedSendFunc()
        {
            if (_improvedSend == null)
            {
                _improvedSend = GetImprovedSend_AsyncFunc() + GetImprovedSend_SyncFunc();
            }
            return _improvedSend;
        }

        static string GetImprovedSend_AsyncFunc()
        {
            if (_improvedSend_AsyncFunc != null)
                return _improvedSend_AsyncFunc;
            var improvedSend_AsyncFunc =
                $"{WebViewWrap.LowLevelJsObjectName}.{ImprovedSend_AsyncFuncName}" +
                @"=function(messageName, callArguments){
            var num = Math.floor(Math.random() * 10001);
            resolveFunctionName = 'randomFunc_Resolve_' + num;
            rejectFunctionName = 'randomFunc_Reject_' + num;
            var resPromise = new Promise(function(rs, rj) {
                window[resolveFunctionName] = rs;
                window[rejectFunctionName] = rj;
            });
            var callArgumentsArr=Array.prototype.slice.call(callArguments);" +
            WebViewWrap.LowLevelJsObjectName + 
            @".SendAsync(messageName, JSON.stringify(callArgumentsArr), resolveFunctionName, rejectFunctionName);
            return resPromise;
            };";
            //Минимизация кода. Да, не очень оптимально, но это происходит один раз.
            var trimmer = new Regex(@"\s\s+");
            improvedSend_AsyncFunc = trimmer.Replace(improvedSend_AsyncFunc, " ");
            improvedSend_AsyncFunc = improvedSend_AsyncFunc
                .Replace("resolveFunctionName", "rsfn").Replace("rejectFunctionName", "rjfn")
                .Replace("callArguments", "ca").Replace("messageName", "mn").Replace("resPromise", "p");
            _improvedSend_AsyncFunc = improvedSend_AsyncFunc;
            return _improvedSend_AsyncFunc;
        }

        static string GetImprovedSend_SyncFunc()
        {
            if (_improvedSend_SyncFunc != null)
                return _improvedSend_SyncFunc;
            var improvedSend_SyncFunc =
                $"{WebViewWrap.LowLevelJsObjectName}.{ImprovedSend_SyncFuncName}" +
                @"=function(messageName, callArguments){
                var callArgumentsArr=Array.prototype.slice.call(callArguments);
                var sendFuncRes=" +
                WebViewWrap.LowLevelJsObjectName + 
                @".SendSync(messageName, JSON.stringify(callArgumentsArr));
                sendFuncRes=JSON.parse(sendFuncRes);
                if(sendFuncRes.isError)
                    throw sendFuncRes.res;
                return sfr.res;};";
            //Минимизация кода. Да, не очень оптимально, но это происходит один раз.
            var trimmer = new Regex(@"\s\s+");
            improvedSend_SyncFunc = trimmer.Replace(improvedSend_SyncFunc, " ");
            improvedSend_SyncFunc = improvedSend_SyncFunc.Replace("callArguments", "ca")
                .Replace("messageName", "mn").Replace("sendFuncRes", "sfr");
            _improvedSend_SyncFunc = improvedSend_SyncFunc;
            return _improvedSend_SyncFunc;
        }
        #endregion

        #region Private instance fields
        WebViewWrap _wvw;

        object _jsInterface;

        string _objName;

        ReflectionMap _refMap;

        string _jsProxyObject;
        #endregion

        /// <summary>
        /// </summary>
        /// <param name="wvw"></param>
        /// <param name="jsInterface"></param>
        /// <param name="objName">Может быть комплексным, типа 'MyApp.FileSystem'</param>
        public JsMessagingProxy(WebViewWrap wvw,string objName, object jsInterface)
        {
            _wvw = wvw;
            _jsInterface = jsInterface;
            _objName = objName;
            _refMap = new ReflectionMap(
                jsInterface.GetType(),
                ".", 
                false, 
                objName, 
                false
                );

            _jsProxyObject = GenerateJsProxyObject(objName);
        }

        /// <summary>
        /// Возвращает строку с кодом js объекта, который проксирует вызовы к нативной части.
        /// </summary>
        public string GetJsProxyObject()
        {
            return _jsProxyObject;
        }

        /// <summary>
        /// Должен быть вызван из js.
        /// При вызове NativeMessages.SendAsync([messageName], [params]); из javascript будет вызван обработчик,
        /// который вы передали. 
        /// Возвращаемый результат (или исключение) будет передан в Promise из javascript. 
        /// Если JsMessageHandler вернул Task, то он сам будет конвертирован в Promise.        
        /// <param name="resolveFunctionName">Имя функции из js, которая будет вызвана при успешном выполнении.</param>
        /// <param name="rejectFunctionName">Имя функции из js, которая будет вызвана при исключении.</param>
        /// /// </summary>
        public async Task SendJsMessageAsync(string messageName, string sendedObjectJson, string resolveFunctionName, string rejectFunctionName)
        {
            object result = null;            
            bool isError=false;
            try
            {
                var methodAndParsedParams = GetMethodAndParseParamsOrThrow(messageName, sendedObjectJson);
                IReflectionMapMethod reflectionMapMethod = methodAndParsedParams.Item1;
                var paramsArray = methodAndParsedParams.Item2;
                //Получаем результат в виде таска.
                result = await reflectionMapMethod.ExecuteAndAwait(_jsInterface, paramsArray);
            }
            catch (Exception ex)
            {
                isError = true;
                //При ошибке в обработчике.
                result = ex;
            }
            string jsonRes = GetJsonResultStr(isError, result, messageName);
            HandleAsyncMethodFinished(isError,jsonRes, messageName, resolveFunctionName, rejectFunctionName);
        }

        

        public string SendJsMessageSync(string messageName, string sendedObjectJson)
        {
            object result = null;
            bool isError = false;
            try
            {
                var methodAndParsedParams = GetMethodAndParseParamsOrThrow(messageName, sendedObjectJson);
                IReflectionMapMethod reflectionMapMethod = methodAndParsedParams.Item1;
                var paramsArray = methodAndParsedParams.Item2;
                //Получаем результат без оберток в виде таска
                result = reflectionMapMethod.Execute(_jsInterface, paramsArray);
            }
            catch (Exception ex)
            {
                isError = true;
                //При ошибке в обработчике.
                result = ex;
            }
            string jsonRes = GetJsonResultStr(isError, result, messageName);

            string jsonFullRes = "{\"isError\":" + (isError ? "true" : "false") + ",";
            jsonFullRes += "\"res\":" + jsonRes + "}";
            return jsonFullRes;
        }

        (IReflectionMapMethod, object[]) GetMethodAndParseParamsOrThrow(string messageName, string sendedObjectJson)
        {
            if (!_refMap.LongNameAndMethod.ContainsKey(messageName))
            {
                throw new Exception("Can`t find handler.");
            }
            IReflectionMapMethod reflectionMapMethod = _refMap.LongNameAndMethod[messageName];

            //Десериализируем параметры json.
            var paramsArray = JsonToParamsBindings.Inst.ResolveFromArray(
                sendedObjectJson,
                reflectionMapMethod.Parameters
                ).ToArray();
            return (reflectionMapMethod, paramsArray);
        }

        /// <summary>
        /// Первый парметр результата - если истина, то метод был завершен исключением.
        /// </summary>
        /// <returns></returns>
        string GetJsonResultStr(bool isError, object resObj,string messageName)
        {
            Exception exception=resObj as Exception;

            if (isError)
                JsMessagingSystem.OnException(exception);

            string jsonResult;
            if(isError)
            {                
                resObj = GetErrorText(messageName, exception);
            }
            jsonResult = JsonConvert.SerializeObject(resObj);

            return jsonResult;
        }

        void HandleAsyncMethodFinished(bool isError, string jsonResultStr, 
            string messageName, string resolveFunctionName, string rejectFunctionName)
        {
            //Вызов js колбека.
            try
            {
                string clearGarbageScript = $"{rejectFunctionName}=null;{resolveFunctionName}=null;";
                //Двойная сереализация нужна для его перечи строки в js
                jsonResultStr = JsonConvert.SerializeObject(jsonResultStr);
                if (isError)
                {
                    _wvw.ExJs(rejectFunctionName + $"(JSON.parse({jsonResultStr}));"+clearGarbageScript);
                }
                else
                {                    
                    _wvw.ExJs(resolveFunctionName + $"(JSON.parse({jsonResultStr}));"+clearGarbageScript);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Exception when call callback of message in {nameof(JsMessagingProxy)}.");
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Возвращает строку с кодом js объекта, который проксирует вызовы к нативной части.
        /// </summary>
        string GenerateJsProxyObject(string objName)
        {
            var jsStrBuilder = new StringBuilder();
            jsStrBuilder.Append("window." + objName + "={};");

            //Передаем переменные в анонимную функцию для сокращения кода.
            //on -> obj в переменной с именем objName
            //isaf -> ImprovedSendAsync function
            //issf -> ImprovedSendSync function
            jsStrBuilder.Append("(function(o,isaf,issf){");
            foreach (var item in _refMap.LongNameAndMethod)
            {
                var refMapMethod = item.Value;
                string messageName = item.Key;

                var isAsync = typeof(Task).IsAssignableFrom(refMapMethod.ReturnType);
                string funcDefineStr=GetFuncDefineString(messageName, objName,isAsync);                
                jsStrBuilder.Append(funcDefineStr);
            }
            var asyncFuncName=WebViewWrap.LowLevelJsObjectName+"."+ImprovedSend_AsyncFuncName;
            var syncFuncName=WebViewWrap.LowLevelJsObjectName+"."+ImprovedSend_SyncFuncName;
            jsStrBuilder.Append($"}})({objName},{asyncFuncName},{syncFuncName});");

            return jsStrBuilder.ToString();
        }

        string GetFuncDefineString(string messageName, string objName, bool isAsync)
        {
            string messageNameWithoutPrefix = messageName.Substring(objName.Length);
            string funcToCall = isAsync ? "isaf" : "issf";
            string funcDefineStr = "o" + messageNameWithoutPrefix + $"=function(){{return {funcToCall}('{messageName}',arguments);}};";
            return funcDefineStr;
        }

        string GetErrorText(string messageName, Exception ex)
        {
            string str = $"Error while calling native'{0}': {ex.Message} .";
            return str;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
