using System.Threading.Tasks;

namespace ItRollingOut.Tools.Localization
{
    public interface ICahceLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Save new translated string (with override).
        /// </summary>
        Task SaveTranslated(TranslatedRecord translatedRecord);
    }
}
