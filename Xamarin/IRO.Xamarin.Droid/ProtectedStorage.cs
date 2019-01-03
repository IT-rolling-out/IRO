
using Android.App;
using Android.Content;
using Android.Preferences;

namespace IRO.Xamarin.Droid
{
    /// <summary>
    /// Вроде как хранилище паролей в андроид или реестр. Пока не используется. 
    /// </summary>
    public static class ProtectedStorage
    {
        public static void Set(string key, string value)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            prefs.Edit().PutString(key, value).Commit();
        }

        public static string Get(string key)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            return prefs.GetString(key, null);
        }
    }
}