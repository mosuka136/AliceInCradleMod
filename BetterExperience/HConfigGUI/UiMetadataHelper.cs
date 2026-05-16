using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using BetterExperience.HConfigSpace;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// 从配置声明中提取 GUI 元数据。
    /// 通过 <see cref="ConfigSliderAttribute"/> 把数值配置项映射为滑条控件。
    /// </summary>
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
