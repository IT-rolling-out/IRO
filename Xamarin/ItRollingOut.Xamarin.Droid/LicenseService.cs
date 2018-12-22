using System.Threading.Tasks;
using Android.Widget;
using S2A.AppPart.AppEnvironment;

namespace ItRollingOut.Xamarin.Droid
{
    class LicenseService:ILicenseService
    {
        /// <summary>
        /// Запускает цикл проверки лицензии.
        /// </summary>
        public void StartLicenseCheck()
        {
            Task.Run(async () =>
            {
                int nonVisibleCounters = 0;
                while (CustomEnv.Current.IsAppOnScreen && !CustomEnv.Conf.LicenseOk)
                {
                    await Task.Delay(10000);
                    if (!CustomEnv.Current.IsAppOnScreen)
                    {
                        nonVisibleCounters++;
                        if (nonVisibleCounters > 30)
                            break;
                    }
                    else
                    {
                        nonVisibleCounters = 0;
                    }

                    if (CustomEnv.Current.IsAppOnScreen && !CustomEnv.Conf.LicenseOk)
                    {
                        Srv.GuiContext.Invoke(() =>
                        {
                            Toast.MakeText(Android.App.Application.Context, "It`s demo version, not for publication.", ToastLength.Long).Show();
                        });
                    }

                    

                }
            });
        }
    }


}