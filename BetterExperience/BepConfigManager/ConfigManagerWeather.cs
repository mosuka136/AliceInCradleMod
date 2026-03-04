using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableVisualImpactOfFog { get; private set; }
        public static ConfigEntry<bool> SetWeatherWind { get; private set; }
        public static ConfigEntry<bool> SetWeatherThunder { get; private set; }
        public static ConfigEntry<bool> SetWeatherMist { get; private set; }
        public static ConfigEntry<bool> SetWeatherDrought { get; private set; }
        public static ConfigEntry<bool> SetWeatherDenseMist { get; private set; }
        public static ConfigEntry<bool> SetWeatherPlague { get; private set; }

        private const string SectionWeather = "Weather";

        public static void InitializeWeather()
        {
            var Config = BetterExperience.Instance.Config;

            EnableVisualImpactOfFog = Config.Bind(
                SectionWeather,
                nameof(EnableVisualImpactOfFog),
                true,
                "Enable visual impact of fog. After disabling, the fog will not be displayed or block the view.\n" +
                "启用雾的视觉效果。关闭后雾将不会显示或遮挡视野。"
                );
            SetWeatherWind = Config.Bind(
                SectionWeather,
                nameof(SetWeatherWind),
                false,
                "Set weather to wind.\n设置天气为旋风。"
                );
            SetWeatherThunder = Config.Bind(
                SectionWeather,
                nameof(SetWeatherThunder),
                false,
                "Set weather to thunder.\n设置天气为雷暴。"
                );
            SetWeatherMist = Config.Bind(
                SectionWeather,
                nameof(SetWeatherMist),
                false,
                "Set weather to mist.\n设置天气为雾。"
                );
            SetWeatherDrought = Config.Bind(
                SectionWeather,
                nameof(SetWeatherDrought),
                false,
                "Set weather to drought.\n设置天气为干旱。"
                );
            SetWeatherDenseMist = Config.Bind(
                SectionWeather,
                nameof(SetWeatherDenseMist),
                false,
                "Set weather to dense mist.\n设置天气为浓雾。"
                );
            SetWeatherPlague = Config.Bind(
                SectionWeather,
                nameof(SetWeatherPlague),
                false,
                "Set weather to plague.\n设置天气为瘟疫。"
                );
        }
    }
}
