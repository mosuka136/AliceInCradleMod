using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using nel;
using System;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<bool> SetWeatherWind { get; private set; }
        internal static ControlEntry<bool> SetWeatherThunder { get; private set; }
        internal static ControlEntry<bool> SetWeatherMist { get; private set; }
        internal static ControlEntry<bool> SetWeatherDrought { get; private set; }
        internal static ControlEntry<bool> SetWeatherDenseMist { get; private set; }
        internal static ControlEntry<bool> SetWeatherPlague { get; private set; }

        private const string SectionWeather = "Weather";

        /// <summary>
        /// 创建天气控制表并绑定天气开关条目。
        /// </summary>
        internal static void InitializeWeather()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionWeather,
                    new Translator(chinese: "天气", english: "Weather"),
                    new Translator(
                        chinese: "查看和修改当前游戏中的天气状态。",
                        english: "View and modify weather in the current game."
                        )
                    );

                SetWeatherWind = BindWeather(
                    nameof(SetWeatherWind),
                    WeatherItem.WEATHER.WIND,
                    new Translator(chinese: "设置天气旋风", english: "Set Weather Wind")
                    );
                SetWeatherThunder = BindWeather(
                    nameof(SetWeatherThunder),
                    WeatherItem.WEATHER.THUNDER,
                    new Translator(chinese: "设置天气雷暴", english: "Set Weather Thunder")
                    );
                SetWeatherMist = BindWeather(
                    nameof(SetWeatherMist),
                    WeatherItem.WEATHER.MIST,
                    new Translator(chinese: "设置天气雾", english: "Set Weather Mist")
                    );
                SetWeatherDrought = BindWeather(
                    nameof(SetWeatherDrought),
                    WeatherItem.WEATHER.DROUGHT,
                    new Translator(chinese: "设置天气干旱", english: "Set Weather Drought")
                    );
                SetWeatherDenseMist = BindWeather(
                    nameof(SetWeatherDenseMist),
                    WeatherItem.WEATHER.MIST_DENSE,
                    new Translator(chinese: "设置天气浓雾", english: "Set Weather Dense Mist")
                    );
                SetWeatherPlague = BindWeather(
                    nameof(SetWeatherPlague),
                    WeatherItem.WEATHER.PLAGUE,
                    new Translator(chinese: "设置天气瘟疫", english: "Set Weather Plague")
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for weather.", ex);
            }
        }

        /// <summary>
        /// 绑定天气开关，读取当前 NightController 状态并把界面修改即时写回游戏。
        /// </summary>
        private static ControlEntry<bool> BindWeather(
            string key,
            WeatherItem.WEATHER weather,
            Translator name)
        {
            return Bind(
                SectionWeather,
                key,
                () => HPatches.SetWeatherPatch.GetWeather(weather),
                value => HPatches.SetWeatherPatch.SetWeather(weather, value),
                name,
                new Translator(
                    chinese: "设置当前游戏中的天气状态。",
                    english: "Set the weather state in the current game."
                    )
                );
        }
    }
}
