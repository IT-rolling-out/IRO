namespace ItRollingOut.Xamarin
{
    public interface IMessagesService
    {
        void ClearNotifications();
        void ShowNotification(string msg, string title = null, bool soundEnabled = true);
        void ShowToast(string msg);
    }
}