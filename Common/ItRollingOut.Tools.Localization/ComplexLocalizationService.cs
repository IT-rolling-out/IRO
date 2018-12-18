using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ItRollingOut.Tools.Localization
{
    public class ComplexLocalizationService : ILocalizationService
    {
        List<ILocalizationService> _localizationServices;
        List<ICahceLocalizationService> _cacheLocalizationServices;        

        /// <summary>
        /// Will be called in order.
        /// </summary>
        /// <param name="anotherServices"></param>
        public ComplexLocalizationService(List<ILocalizationService> anotherServices)
        {
            _localizationServices = anotherServices;
            _cacheLocalizationServices = new List<ICahceLocalizationService>();
            foreach(var serv in _localizationServices)
            {
                if(serv is ICahceLocalizationService)
                {
                    _cacheLocalizationServices.Add((ICahceLocalizationService)serv);
                }
            }
        }

        public async Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            if (string.IsNullOrWhiteSpace(sourceString))
                return sourceString;
            if (sourceCultureInfo == translateCultureInfo)
                return sourceString;
            sourceString = sourceString.Trim();
            foreach (var serv in _localizationServices)
            {
                string translatedStr = await serv.TryGetTranslated(sourceString, sourceCultureInfo, translateCultureInfo);
                if (string.IsNullOrWhiteSpace(translatedStr))
                    continue;
                if(_cacheLocalizationServices.Count>0)
                {
                    //Save to cache
                    TranslatedRecord rec = new TranslatedRecord()
                    {
                        SourceString = sourceString,
                        TranslatedString = translatedStr,
                        SourceCultureInfo = sourceCultureInfo,
                        TranslateCultureInfo = translateCultureInfo
                    };
                    foreach (var cacheServ in _cacheLocalizationServices)
                    {
                        await cacheServ.SaveTranslated(rec);
                    }
                }
                return translatedStr;
            }
            throw new Exception("Can`t find translated string in all services.");
        }
    }
}
