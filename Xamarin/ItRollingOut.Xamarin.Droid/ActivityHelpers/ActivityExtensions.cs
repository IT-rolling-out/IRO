using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace ItRollingOut.Xamarin.Droid.ActivityHelpers
{
    public static class ActivityExtensions
    {
        static Random random = new Random();

        /// <summary>
        /// You doesn`t need to check requestCode in result, it will be checked automatically. 
        /// If requestCode don`t match, task will pe finished with exception.
        /// </summary>
        public static Task<ActivityResultArgs> StartActivityAndReturnResult(Intent intent, int requestCode)
        {
            var taskCompletionSource = new TaskCompletionSource<ActivityResultArgs>();

            ActivityResultReturnedDelegate evHandler = null;
            evHandler=(resultArgs) =>
            {
                //Ивент получения результата от активити. 
                //При нормальных обстоятельствах всегда resultArgs.RequestCode == requestCode.
                ReceiveResultTransperedActivity.ActivityResultReturned -= evHandler;
                if (resultArgs.RequestCode == requestCode)
                {                    
                    taskCompletionSource.SetResult(resultArgs);
                }
                else
                {
                    taskCompletionSource.SetException(new Exception("RequestCode in activity result doesn`t match to passed RequestCode."));
                }
            };


            //Задаем текущие параметры
            var hiddenActivityStartIntent = new Intent(Application.Context,typeof(ReceiveResultTransperedActivity));
            //hiddenActivityStartIntent.PutExtra(ReceiveResultTransperedActivity.IncludedIntentParamName, intent);
            //hiddenActivityStartIntent.PutExtra(ReceiveResultTransperedActivity.RequestCodeParamName, requestCode);
            ReceiveResultTransperedActivity.CurrentIntent = intent;
            ReceiveResultTransperedActivity.CurrentRequestCode =requestCode;
            ReceiveResultTransperedActivity.ActivityResultReturned += evHandler;
            hiddenActivityStartIntent.AddFlags(ActivityFlags.NewTask);

            //Создаем промежуточную прозрачную активити в контексте приложения (не в контексте другой активити).
            Application.Context.StartActivity(hiddenActivityStartIntent);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// RequestCode will be generated.
        /// 
        /// You doesn`t need to check requestCode in result, it will be checked automatically. 
        /// If requestCode don`t match, task will pe finished with exception.
        /// </summary>
        public static Task<ActivityResultArgs> StartActivityAndReturnResult(Intent intent)
        {
            return StartActivityAndReturnResult(intent, random.Next(100000, 999999));
        }
    }
}