using MvvmCross;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.Xamarin
{
    /// <summary>
    /// Just proxy calls to ioc container.
    /// </summary>
    public static class Srv
    {
        //С кешированием
        static IMessagesService messages;
        public static IMessagesService Messages
        {
            get
            {
                if (messages == null)
                {
                    messages = Mvx.IoCProvider.Resolve<IMessagesService>();
                }
                return messages;
            }
        }

        //Без кеширования
        public static ILicenseService License
        {
            get => Mvx.IoCProvider.Resolve<ILicenseService>();
        }

        public static IGuiContext GuiContext
        {
            get => Mvx.IoCProvider.Resolve<IGuiContext>();
        }

        public static IProtectedStorageService ProtectedStorage
        {
            get => Mvx.IoCProvider.Resolve<IProtectedStorageService>();
        }

        public static IShareService Share
        {
            get => Mvx.IoCProvider.Resolve<IShareService>();
        }
    }

    public class XamarinServices
    {

    }
}
