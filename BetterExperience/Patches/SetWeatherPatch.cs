using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在读档完成后按配置预加载天气，并为实时控制界面提供当前天气的读写入口。
        /// </summary>
        public class SetWeatherPatch
        {
            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    ApplyPreloadedWeather(WeatherItem.WEATHER.WIND, ConfigManager.SetWeatherWind);
                    ApplyPreloadedWeather(WeatherItem.WEATHER.THUNDER, ConfigManager.SetWeatherThunder);
                    ApplyPreloadedWeather(WeatherItem.WEATHER.MIST, ConfigManager.SetWeatherMist);
                    ApplyPreloadedWeather(WeatherItem.WEATHER.DROUGHT, ConfigManager.SetWeatherDrought);
                    ApplyPreloadedWeather(WeatherItem.WEATHER.MIST_DENSE, ConfigManager.SetWeatherDenseMist);
                    ApplyPreloadedWeather(WeatherItem.WEATHER.PLAGUE, ConfigManager.SetWeatherPlague);
                };

                _initialized = true;
                BLog.Debug("Weather patch initialized.");
            }

            private static void ApplyPreloadedWeather(WeatherItem.WEATHER weather, ConfigEntry<bool, bool> configEntry)
            {
                if (!configEntry.Value1)
                    return;

                BLog.Debug($"Applying preloaded weather: {weather} => {configEntry.Value2}");
                SetWeather(weather, configEntry.Value2);
            }

            public static void SetWeather(WeatherItem.WEATHER weather, bool setWeather)
            {
                try
                {
                    var nightController = GetNightController();
                    if (nightController == null)
                    {
                        BLog.Notice($"NightController not found while applying weather setting: {weather} => {setWeather}");
                        return;
                    }

                    var nightControllerTraverse = Traverse.Create(nightController);
                    if (nightControllerTraverse == null)
                    {
                        BLog.Notice($"Traverse creation failed while applying weather setting: {weather} => {setWeather}");
                        return;
                    }

                    var currentWeather = nightControllerTraverse.Field("cur_weather_");
                    var weatherItems = nightControllerTraverse.Field("AWeather");

                    if (currentWeather == null || weatherItems == null)
                    {
                        BLog.Notice($"Weather fields not found while applying weather setting: {weather} => {setWeather}");
                        return;
                    }

                    BLog.Debug($"Applying weather setting: {weather} => {setWeather}");

                    var hasWeather = GetBit(currentWeather.GetValue<int>(), (int)weather & 0x1F);
                    if (setWeather)
                    {
                        if (!hasWeather)
                        {
                            var weatherItemList = new List<WeatherItem>(weatherItems.GetValue<WeatherItem[]>());
                            var newWeatherItem = new WeatherItem(
                                weather,
                                nightControllerTraverse.Field("dlevel").GetValue<int>()
                                + nightControllerTraverse.Field("dlevel_add").GetValue<int>()).initS(null);
                            var conflict = newWeatherItem.get_conflict();
                            if (conflict != 0)
                            {
                                // 新天气可能与现有天气互斥，必须同步移除 WeatherItem 和 cur_weather 位标记。
                                var conflictWeather = new List<WeatherItem>();
                                foreach (var weatherItem in weatherItemList)
                                {
                                    if (GetBit((int)conflict, (int)weatherItem.weather & 0x1F))
                                    {
                                        currentWeather.SetValue(SetResetBit(
                                            currentWeather.GetValue<int>(),
                                            (int)weatherItem.weather & 0x1F,
                                            false));
                                        conflictWeather.Add(weatherItem);
                                    }
                                }

                                conflictWeather.ForEach(item => item.destruct());
                                weatherItemList.RemoveAll(item => conflictWeather.Contains(item));
                            }

                            weatherItemList.Add(newWeatherItem);
                            weatherItems.SetValue(weatherItemList.ToArray());
                            currentWeather.SetValue(SetResetBit(
                                currentWeather.GetValue<int>(),
                                (int)weather & 0x1F,
                                true));
                            newWeatherItem.showLog();
                            BLog.Debug($"Weather enabled: {weather}");
                        }
                    }
                    else if (hasWeather)
                    {
                        var oldWeather = nightController.getWeather(weather);
                        var newWeatherList = new List<WeatherItem>(weatherItems.GetValue<WeatherItem[]>());
                        var index = newWeatherList.IndexOf(oldWeather);
                        if (index >= 0)
                        {
                            oldWeather.destruct();
                            newWeatherList.RemoveAt(index);
                            weatherItems.SetValue(newWeatherList.ToArray());
                            currentWeather.SetValue(SetResetBit(
                                currentWeather.GetValue<int>(),
                                (int)weather & 0x1F,
                                false));
                            BLog.Debug($"Weather disabled: {weather}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetWeather)}.", ex);
                }
            }

            /// <summary>
            /// 读取当前天气状态；NightController 尚未加载或读取失败时返回 false。
            /// </summary>
            public static bool GetWeather(WeatherItem.WEATHER weather)
            {
                try
                {
                    return GetNightController()?.getWeather(weather) != null;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(GetWeather)}.", ex);
                    return false;
                }
            }

            public static int SetResetBit(int value, int bit, bool set)
            {
                if (set)
                    return value | (1 << bit);
                else
                    return value & ~(1 << bit);
            }

            public static bool GetBit(int value, int bit)
            {
                return (value & (1 << bit)) != 0;
            }
        }
    }
}
