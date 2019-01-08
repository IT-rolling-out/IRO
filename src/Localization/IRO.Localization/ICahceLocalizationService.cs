using System.Threading.Tasks;

namespace IRO.Localization
{
    public interface ICahceLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Save new translated string (with override).
        /// </summary>
        Task SaveTranslated(TranslatedRecord translatedRecord);
    }
}
