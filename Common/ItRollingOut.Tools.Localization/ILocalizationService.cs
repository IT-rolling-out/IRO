using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ItRollingOut.Tools.Localization
{
    public interface ILocalizationService
    {
        //!При возвращении перевода не должны теряться подобные шаблоны '{0}'.
        /// <summary>
        /// If can`t find - exception will be thrown.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="sourceCultureInfo">Language of your program. In 99% will be same for all strings.</param>
        /// <param name="translateCultureInfo"></param>
        /// <returns></returns>
        Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo);

    }
}
