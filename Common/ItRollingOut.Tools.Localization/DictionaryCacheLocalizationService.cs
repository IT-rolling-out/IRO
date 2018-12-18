using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ItRollingOut.Tools.Localization
{
    public class DictionaryCacheLocalizationService : ICahceLocalizationService
    {
        ConcurrentDictionary<string, TranslatedRecord> _cacheDict = new ConcurrentDictionary<string, TranslatedRecord>();
        Queue<TranslatedRecord> _cacheQueue = new Queue<TranslatedRecord>();

        public int Limit { get; }

        public DictionaryCacheLocalizationService(int limit=300)
        {
            Limit = limit;
        }

        public async Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            string key = TranslatedRecord.GetKey(sourceString, sourceCultureInfo, translateCultureInfo);
            _cacheDict.TryGetValue(key, out TranslatedRecord translateRec);            
            return translateRec.TranslatedString;
        }

        public async Task SaveTranslated(TranslatedRecord translatedRecord)
        {
            //Delete first element of the queue if we've hit the limit.
            if (_cacheDict.Count > Limit && _cacheDict.ContainsKey(translatedRecord.Key))
            {
                var toRemove = _cacheQueue.Peek();
                _cacheDict.TryRemove(toRemove.Key, out var tr);
            }

            _cacheQueue.Enqueue(translatedRecord);
            _cacheDict.TryAdd(translatedRecord.Key, translatedRecord);
        }
    }
}
