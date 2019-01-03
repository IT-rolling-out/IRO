using Android.App;
using Android.Content;

namespace IRO.Xamarin.Droid.ActivityHelpers
{
    public class ActivityResultArgs
    {
        public int RequestCode { get; set; }
        public Result ResultCode { get; set; }
        public Intent Data { get; set; }
    }
}