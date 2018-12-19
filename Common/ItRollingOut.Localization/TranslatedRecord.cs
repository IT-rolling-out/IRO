using System.Globalization;

namespace ItRollingOut.Localization
{
    public class TranslatedRecord
    {
        public string Key => GetKey();
        public string SourceString { get; set; }
        public string TranslatedString { get; set; }
        public CultureInfo SourceCultureInfo { get; set; }
        public CultureInfo TranslateCultureInfo { get; set; }

        string GetKey()
        {
            return GetKey(SourceString, SourceCultureInfo, TranslateCultureInfo);
        }

        public static string GetKey(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            return sourceString + "__" + sourceCultureInfo.Name + "__" + translateCultureInfo.Name;
        }
    }
}
