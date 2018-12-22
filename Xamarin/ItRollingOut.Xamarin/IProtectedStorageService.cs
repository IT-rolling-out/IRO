namespace ItRollingOut.Xamarin
{
    public interface IProtectedStorageService
    {
        string Get(string key);
        void Set(string key, string value);
    }
}