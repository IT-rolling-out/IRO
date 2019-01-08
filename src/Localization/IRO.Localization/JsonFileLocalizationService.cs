using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IRO.Localization
{
    /// <summary>
    /// File with translate.
    /// </summary>
    public class JsonFileLocalizationService : ILocalizationService
    {
        public const string KeywordToUseDefaultTranslate= "----";

        ConcurrentDictionary<CultureInfo, List<string>> _cachedLocalizationsIndexToStr = 
            new ConcurrentDictionary<CultureInfo, List<string>>();

        List<CultureInfo> _availableTranslates = new List<CultureInfo>();

        CultureInfo _defaultCulture;

        string _jsonPath;

        public JsonFileLocalizationService(string path, bool checkRowsCountIsSame = false)
        {
            //Json structure
            /*
            [
                //sourceCulture
                {
                    cultureName:"ru-RF",
                    strings:[
                        "привет",
                        "что",
                        "мир"                        
                    ]
                },
                {
                    cultureName:"en-US",
                    strings:[
                        "hello",
                        "----",
                        "world"                       
                    ]
                }

            ]
            */
            
            _jsonPath = path;
            string jsonStr = File.ReadAllText(path);
            var localizations = JsonConvert.DeserializeObject<Localization[]>(jsonStr);
            if (localizations.Length < 2)
            {
                throw new Exception("There must be 2 or more translates in json file.");
            }

            int rowsCount = localizations[0].Strings.Count;
            foreach(var item in localizations)
            {
                if (checkRowsCountIsSame && rowsCount != item.Strings.Count)
                    throw new Exception("Number of rows must be same for all localizations.");
                var cultInfo = CultureInfo.GetCultureInfo(item.CultureName);
                _availableTranslates.Add(cultInfo);
            }

            //Caching first 2 translates
            TryFindCulture(_availableTranslates[0],jsonStr);
            TryFindCulture(_availableTranslates[1],jsonStr);
            _defaultCulture = _availableTranslates[0];
        }

        public async Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            if (!TryFindCulture(sourceCultureInfo))
            {
                throw new Exception($"Can`t find culture '{sourceCultureInfo.Name}'");
            }
            if (!TryFindCulture(translateCultureInfo))
            {
                throw new Exception($"Can`t find culture '{translateCultureInfo.Name}'");
            }

            int index = _cachedLocalizationsIndexToStr[sourceCultureInfo].IndexOf(sourceString);
            string translateStr = _cachedLocalizationsIndexToStr[translateCultureInfo][index];
            if (translateStr == KeywordToUseDefaultTranslate)
            {
                return _cachedLocalizationsIndexToStr[_defaultCulture][index];
            }
            return translateStr;
        }

        bool TryFindCulture(CultureInfo cultureInfo, string jsonStr = null)
        {
            bool containsCulture = _cachedLocalizationsIndexToStr.ContainsKey(cultureInfo);
            if (!containsCulture && _availableTranslates.Contains(cultureInfo))
            {
                //Попытка найти сultureInfo и закешировать
                if(jsonStr!=null)
                    jsonStr = File.ReadAllText(_jsonPath);
                var localizations = JsonConvert.DeserializeObject<Localization[]>(jsonStr);
                foreach (var item in localizations)
                {
                    if (item.CultureName != cultureInfo.Name)
                        continue;
                    var cultInfo = CultureInfo.GetCultureInfo(item.CultureName);
                    _cachedLocalizationsIndexToStr.TryAdd(cultInfo, item.Strings);
                    containsCulture = true;
                }
            }
            return containsCulture;

        }

        Dictionary<string, int> LocalizationToDict(Localization localizations)
        {
            Dictionary<string, int> localizationsInternalDict = new Dictionary<string, int>();
            for (int i = 0; i < localizations.Strings.Count; i++)
            {
                var sourceStr = localizations.Strings[i];
                localizationsInternalDict.Add(sourceStr, i);
            }
            return localizationsInternalDict;
        }
    }
}
