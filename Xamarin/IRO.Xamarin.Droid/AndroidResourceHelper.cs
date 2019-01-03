using Android.App;

namespace IRO.Xamarin.Droid
{
    public static class AndroidResourceHelper
    {
        /// <summary>
        /// Получает id ресурса динамически.
        /// </summary>
        public static int GetId(string name, string defType)
        {
            return Application.Context.Resources.GetIdentifier(
                name,
                defType,
                Application.Context.PackageName
                );
        }

        /// <summary>
        /// Получает id ресурса динамически.
        /// </summary>
        /// <param name="longName">Something like 'Layout.Main'</param>
        /// <returns></returns>
        public static int GetId(string longName)
        {
            string[] arr = longName.Split('.');
            return Application.Context.Resources.GetIdentifier(
                arr[1],
                arr[0].ToLower(),
                Application.Context.PackageName
                );
        }
    }
}