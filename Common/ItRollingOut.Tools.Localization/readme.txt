Json translate file structure: 

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

						//it means "no translate"
                        "---",
                        "world"                       
                    ]
                }

            ]
*/

Use example:
	ILocalizationService _localizationService = new ComplexLocalizationService(
		new List<ILocalizationService>(){		    
			new DictionaryCacheLocalizationService(),
			new JsonFileLocalizationService("localization.json"),
			new LitedbCacheLocalizationService("localization_cache.db"),
			new GoogleTranslateLocalizationService()
		}
		);