using System.Threading.Tasks;

namespace IRO.Xamarin.Droid
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
                await Task.Delay(10000); 

            });
        }
    }

    
}