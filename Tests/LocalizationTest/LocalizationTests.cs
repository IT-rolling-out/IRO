using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ItRollingOut.Tools.Localization;
using System.Globalization;

namespace ItRollingOut.Tools.Tests
{
    public class LocalizationTests
    {
        [Fact]
        public async void TestSingle()
        {
            ILocalizationService _localizationService = new ComplexLocalizationService(
                new List<ILocalizationService>(){
                    new DictionaryCacheLocalizationService(),
                    new JsonFileLocalizationService("translate.json"),
                    new LitedbCacheLocalizationService("test_cache.db"),
                    new GoogleTranslateLocalizationService(),
                    new DefaultLocalizationService()
            });
            var record = await _localizationService.GetTranslated(
                "привет",
                CultureInfo.GetCultureInfo("ru-RU"),
                CultureInfo.GetCultureInfo("en-US"));
            Assert.Equal("hello", record);
        }
    }
}
