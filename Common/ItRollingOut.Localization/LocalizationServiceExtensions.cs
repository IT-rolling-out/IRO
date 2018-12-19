using System.Globalization;
using System.Threading.Tasks;

namespace ItRollingOut.Localization
{
    public static class LocalizationServiceExtensions
    {
        /// <summary>
        /// Return null on error.
        /// </summary>
        public static async Task<string> TryGetTranslated(this ILocalizationService localizationService, string sourceString, 
            CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            try
            {
                return await localizationService.GetTranslated(sourceString, sourceCultureInfo, translateCultureInfo);
            }
            catch
            {
                return null;
            }
        }
    }
}
