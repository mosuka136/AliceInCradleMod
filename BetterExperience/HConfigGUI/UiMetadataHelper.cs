using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.HConfigFileSpace;

namespace BetterExperience.HConfigGUI
{
    public static class UiMetadataHelper
    {
        public static IUiMetadata GetMetadata(IConfigEntry entry)
        {
            if (entry == null)
                return null;

            var sliderInfo = ClassHelper.GetSliderInfo<ConfigManager>(entry.Key);
            if (sliderInfo.HasValue)
            {
                return new UiSliderMetadata(sliderInfo.Value.Min, sliderInfo.Value.Max, sliderInfo.Value.Step);
            }
            return null;
        }
    }
}
