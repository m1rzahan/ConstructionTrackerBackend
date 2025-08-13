using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace ConstructionTracker.Localization;

public static class ConstructionTrackerLocalizationConfigurer
{
    public static void Configure(ILocalizationConfiguration localizationConfiguration)
    {
        localizationConfiguration.Sources.Add(
            new DictionaryBasedLocalizationSource(ConstructionTrackerConsts.LocalizationSourceName,
                new XmlEmbeddedFileLocalizationDictionaryProvider(
                    typeof(ConstructionTrackerLocalizationConfigurer).GetAssembly(),
                    "ConstructionTracker.Localization.SourceFiles"
                )
            )
        );
    }
}
