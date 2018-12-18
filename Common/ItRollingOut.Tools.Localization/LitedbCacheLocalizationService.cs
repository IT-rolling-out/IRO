using System;
using System.Globalization;
using System.Threading.Tasks;
using LiteDB;

namespace ItRollingOut.Tools.Localization
{
    public class LitedbCacheLocalizationService : ICahceLocalizationService, IDisposable
    {
        private LiteDatabase database;
        private LiteCollection<TranslatedRecord> records;
        private string path;

        public LitedbCacheLocalizationService(string path)
        {   using (database = new LiteDatabase(path))
            {
                records = database.GetCollection<TranslatedRecord>("translatedRecords");
                records.EnsureIndex(x => x.Key);
            }
        }

        public async Task<string> GetTranslated(string sourceString, CultureInfo sourceCultureInfo, CultureInfo translateCultureInfo)
        {
            using (database = new LiteDatabase(path))
            {
                records = database.GetCollection<TranslatedRecord>("translatedRecords");
                var wantedKey = TranslatedRecord.GetKey(sourceString,
                        sourceCultureInfo,
                        translateCultureInfo);
                return records.FindOne(x => x.Key == wantedKey).TranslatedString;
            }   
        }

        public async Task SaveTranslated(TranslatedRecord translatedRecord)
        {
            using (database = new LiteDatabase(path))
            {
                records = database.GetCollection<TranslatedRecord>("translatedRecords");
                records.Insert(translatedRecord);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    database.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
