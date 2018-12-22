using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;

namespace S2A.Plugins.WebViewSuite.Droid
{
    public static class WebViewExtensions
    {
        /// <summary>
        /// Расширение WebView для выполнения скрипта  и получения результата.
        /// </summary>
        public static Task<object> ExJsWithResult(this WebView wv, string script, int? timeoutMS = null)
        {
            //TODO Такой код лочит главный поток при вызове Wait в нем. Не знаю возможноно ли вообще это исправить, но желательно.
            var callback = new JsValueCallback();           
            Application.SynchronizationContext.Post((obj) =>
            {
                wv.EvaluateJavascript(script, callback);
            },null);
            
            var taskCompletionSource = callback.GetTaskCompletionSource();
            var t = taskCompletionSource.Task;
            if (timeoutMS != null)
            {
                Task.Run(async () =>
                {
                    await Task.WhenAny(
                        t,
                        Task.Delay(timeoutMS.Value)
                        );
                    if (!t.IsCompleted)
                        taskCompletionSource.TrySetException(new Exception($"Js evaluation timeout {timeoutMS}"));
                });
            }     
            return t;
        }

        class JsValueCallback : Java.Lang.Object,IValueCallback
        {
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

            public void OnReceiveValue(Java.Lang.Object value)
            {
                taskCompletionSource.SetResult(value);
            }

            public TaskCompletionSource<object> GetTaskCompletionSource()
            {
                return taskCompletionSource;
            }
        }
        
    }
}