using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
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
            Config.CreateTable(SectionWeather, new Translator(chinese: "天气", english: "Weather"));

            EnableVisualImpactOfFog = Config.Bind(
                SectionWeather,
                nameof(EnableVisualImpactOfFog),
                true,
                new Translator(chinese: "启用雾的视觉效果", english: "Enable Visual Impact Of Fog"),
                new Translator(
                    chinese: "启用雾的视觉效果。关闭后雾将不会显示或遮挡视野。",
                    english: "Enable visual impact of fog. After disabling, the fog will not be displayed or block the view."
                )
                );
            SetWeatherWind = Config.Bind(
                SectionWeather,
                nameof(SetWeatherWind),
                false,
                new Translator(chinese: "设置天气旋风", english: "Set Weather Wind"),
                new Translator(
                    chinese: "设置天气为旋风。",
                    english: "Set weather to wind."
                )
                );
            SetWeatherThunder = Config.Bind(
                SectionWeather,
                nameof(SetWeatherThunder),
                false,
                new Translator(chinese: "设置天气雷暴", english: "Set Weather Thunder"),
                new Translator(
                    chinese: "设置天气为雷暴。",
                    english: "Set weather to thunder."
                )
                );
            SetWeatherMist = Config.Bind(
                SectionWeather,
                nameof(SetWeatherMist),
                false,
                new Translator(chinese: "设置天气雾", english: "Set Weather Mist"),
                new Translator(
                    chinese: "设置天气为雾。",
                    english: "Set weather to mist."
                )
                );
            SetWeatherDrought = Config.Bind(
                SectionWeather,
                nameof(SetWeatherDrought),
                false,
                new Translator(chinese: "设置天气干旱", english: "Set Weather Drought"),
                new Translator(
                    chinese: "设置天气为干旱。",
                    english: "Set weather to drought."
                )
                );
            SetWeatherDenseMist = Config.Bind(
                SectionWeather,
                nameof(SetWeatherDenseMist),
                false,
                new Translator(chinese: "设置天气浓雾", english: "Set Weather Dense Mist"),
                new Translator(
                    chinese: "设置天气为浓雾。",
                    english: "Set weather to dense mist."
                )
                );
            SetWeatherPlague = Config.Bind(
                SectionWeather,
                nameof(SetWeatherPlague),
                false,
                new Translator(chinese: "设置天气瘟疫", english: "Set Weather Plague"),
                new Translator(
                    chinese: "设置天气为瘟疫。",
                    english: "Set weather to plague."
                )
                );
        }
    }
}
