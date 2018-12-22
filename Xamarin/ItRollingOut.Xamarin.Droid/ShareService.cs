
using Android.App;
using Android.Content;

namespace ItRollingOut.Xamarin.Droid
{
    class ShareService : IShareService
    {
        public void ShareText(string text)//string shareTitle)
        {
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionSend)
                  .PutExtra(Intent.ExtraText, text)
                  .SetType("text/plain");
            //Надо определиться, что писать вместо Share
            Application.Context.StartActivity(Intent.CreateChooser(intent, "Share"));
            //Application.Context.StartActivity(Intent.CreateChooser(intent, shareTitle));
        }
    }
}