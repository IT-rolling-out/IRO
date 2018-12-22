namespace ItRollingOut.Xamarin.Droid
{
    public class Plugin 
    {
        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IGuiContext>(new GuiContext());
            Mvx.IoCProvider.RegisterSingleton<ILicenseService>(new LicenseService());
            Mvx.IoCProvider.RegisterSingleton<IMessagesService>(new MessagesService());
            Mvx.IoCProvider.RegisterSingleton<IShareService>(new ShareService());
        }
    }
}