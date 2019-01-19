using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using IRO.Localization;
using NUnit.Framework;

namespace IRO.SlnUnitTests
{
    public class LocalizationTests
    {
        [Test]
        public async Task TestDictionaryCache()
        {
            var commonCahceServ = new DictionaryCacheLocalizationService();
            var localizationService = new ComplexLocalizationService(
                new List<ILocalizationService>(){
                    commonCahceServ,
                    new DefaultLocalizationService()
                });
            var record = await localizationService.GetTranslated(
                "привет",
                CultureInfo.GetCultureInfo("ru-RU"),
                CultureInfo.GetCultureInfo("en-US")
                );
            Assert.AreEqual("привет", record);
        }

        [Test(Description = "Testing DefaultLocalizationService - return passed string.")]
        public async Task TestDefaultLocalization()
        {
            var commonCahceServ = new DictionaryCacheLocalizationService();
            var localizationService = new ComplexLocalizationService(
                new List<ILocalizationService>(){
                    commonCahceServ,
                    new DefaultLocalizationService()
                });
            var record1 = await localizationService.GetTranslated(
                "привет",
                CultureInfo.GetCultureInfo("ru-RU"),
                CultureInfo.GetCultureInfo("en-US")
                );

            var record2 = await localizationService.GetTranslated(
                "привет",
                CultureInfo.GetCultureInfo("ru-RU"),
                CultureInfo.GetCultureInfo("en-US")
                );
            var localizationService_CacheOnlyTest = new ComplexLocalizationService(
                new List<ILocalizationService>(){
                    commonCahceServ,
                    new DefaultLocalizationService()
                });
            Assert.AreEqual("привет", record2);
        }

        [Test]
        public async Task TestAll()
        {
            Assert.Warn("Services not ready.");
            return;
            var localizationService = new ComplexLocalizationService(
                new List<ILocalizationService>(){
                    new DictionaryCacheLocalizationService(),
                    new JsonFileLocalizationService("translate.json"),
                    new LitedbCacheLocalizationService("test_cache.db"),
                    new GoogleTranslateLocalizationService(),
                    new DefaultLocalizationService()
                });
            var record = await localizationService.GetTranslated(
                "привет",
                CultureInfo.GetCultureInfo("ru-RU"),
                CultureInfo.GetCultureInfo("en-US"));
            Assert.AreEqual("hello", record);
        }
    }
}