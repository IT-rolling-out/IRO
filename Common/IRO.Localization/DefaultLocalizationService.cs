using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace IRO.Localization
{
    public class DefaultLocalizationService : ILocalizationService
    {
        public async Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            return sourceString;
        }
    }
}
