using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 雾效开关是持久化偏好；具体天气使用双值“读档后预加载”配置，实时修改由 ControlManager 负责。
        public static ConfigEntry<bool> EnableVisualImpactOfFog { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherWind { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherThunder { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherMist { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherDrought { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherDenseMist { get; private set; }
        [EntryGui(2)]
        public static ConfigEntry<bool, bool> SetWeatherPlague { get; private set; }

        private const string SectionWeather = "Weather";

        /// <summary>
        /// 初始化天气可视效果和具体天气的读档预加载配置。
        /// </summary>
        public static void InitializeWeather()
        {
            try
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
                SetWeatherWind = BindPreloadValue(
                    SectionWeather,
                    nameof(SetWeatherWind),
                    false,
                    new Translator(chinese: "设置天气旋风", english: "Set Weather Wind"),
                    new Translator(
                        chinese: "设置天气为旋风。",
                        english: "Set weather to wind."
                        )
                    );
                SetWeatherThunder = BindPreloadValue(
                    SectionWeather,
                    nameof(SetWeatherThunder),
                    false,
                    new Translator(chinese: "设置天气雷暴", english: "Set Weather Thunder"),
                    new Translator(
                        chinese: "设置天气为雷暴。",
                        english: "Set weather to thunder."
                        )
                    );
                SetWeatherMist = BindPreloadValue(
                    SectionWeather,
                    nameof(SetWeatherMist),
                    false,
                    new Translator(chinese: "设置天气雾", english: "Set Weather Mist"),
                    new Translator(
                        chinese: "设置天气为雾。",
                        english: "Set weather to mist."
                        )
                    );
                SetWeatherDrought = BindPreloadValue(
                    SectionWeather,
                    nameof(SetWeatherDrought),
                    false,
                    new Translator(chinese: "设置天气干旱", english: "Set Weather Drought"),
                    new Translator(
                        chinese: "设置天气为干旱。",
                        english: "Set weather to drought."
                        )
                    );
                SetWeatherDenseMist = BindPreloadValue(
                    SectionWeather,
                    nameof(SetWeatherDenseMist),
                    false,
                    new Translator(chinese: "设置天气浓雾", english: "Set Weather Dense Mist"),
                    new Translator(
                        chinese: "设置天气为浓雾。",
                        english: "Set weather to dense mist."
                        )
                    );
                SetWeatherPlague = BindPreloadValue(
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
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for weather.", ex);
            }
        }
    }
}
