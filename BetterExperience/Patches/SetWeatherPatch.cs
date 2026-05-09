using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetWeatherPatch
        {
            private static bool _initialized = false;
            private static bool _isApplying = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    HLog.Debug("Game save/load completed. Synchronizing weather config from game state.");
                    Flush();
                };


                ConfigManager.SetWeatherWind.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.WIND, e);
                };
                ConfigManager.SetWeatherThunder.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.THUNDER, e);
                };
                ConfigManager.SetWeatherMist.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.MIST, e);
                };
                ConfigManager.SetWeatherDrought.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.DROUGHT, e);
                };
                ConfigManager.SetWeatherDenseMist.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.MIST_DENSE, e);
                };
                ConfigManager.SetWeatherPlague.OnValueChanged += (s, e) =>
                {
                    if (!_isApplying)
                        TrySetWeather(WeatherItem.WEATHER.PLAGUE, e);
                };

                _initialized = true;
                HLog.Debug("Weather patch initialized.");
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(NightController), nameof(NightController.weatherShuffle))]
            public static void WeatherShufflePostfix()
            {
                HLog.Debug("Detected weather shuffle. Synchronizing weather config from game state.");
                Flush();
            }

            private static void TrySetWeather(WeatherItem.WEATHER weather, bool setWeather)
            {
                try
                {
                    SetWeather(weather, setWeather);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexcepted error in {nameof(SetWeatherPatch)}", ex);
                }
            }

            public static void SetWeather(WeatherItem.WEATHER weather, bool setWeather)
            {
                var nc = GetNightController();
                if (nc == null)
                {
                    HLog.Notice($"NightController not found while applying weather setting: {weather} => {setWeather}");
                    return;
                }

                var nct = Traverse.Create(nc);
                if (nct == null)
                {
                    HLog.Notice($"Traverse creation failed while applying weather setting: {weather} => {setWeather}");
                    return;
                }

                var cur_weather = nct.Field("cur_weather_");
                var AWeather = nct.Field("AWeather");

                if (cur_weather == null || AWeather == null)
                {
                    HLog.Notice($"Weather fields not found while applying weather setting: {weather} => {setWeather}");
                    return;
                }

                HLog.Debug($"Applying weather setting: {weather} => {setWeather}");

                var hasWeather = GetBit(cur_weather.GetValue<int>(), (int)weather & 0x1F);
                if (setWeather)
                {
                    if (!hasWeather)
                    {
                        var weatherItemList = new List<WeatherItem>(AWeather.GetValue<WeatherItem[]>());
                        var newWeatherItem = new WeatherItem(weather, nct.Field("dlevel").GetValue<int>() + nct.Field("dlevel_add").GetValue<int>()).initS(null);
                        var conflict = newWeatherItem.get_conflict();
                        if (conflict != 0)
                        {
                            var conflictWeather = new List<WeatherItem>();
                            foreach (var weatherItem in weatherItemList)
                            {
                                if (GetBit((int)conflict, (int)weatherItem.weather & 0x1F))
                                {
                                    cur_weather.SetValue(SetResetBit(cur_weather.GetValue<int>(), (int)weatherItem.weather & 0x1F, false));
                                    conflictWeather.Add(weatherItem);
                                }
                            }

                            conflictWeather.ForEach(w => w.destruct());
                            weatherItemList.RemoveAll(w => conflictWeather.Contains(w));
                        }

                        weatherItemList.Add(newWeatherItem);
                        AWeather.SetValue(weatherItemList.ToArray());
                        cur_weather.SetValue(SetResetBit(cur_weather.GetValue<int>(), (int)weather & 0x1F, true));
                        newWeatherItem.showLog();
                        HLog.Debug($"Weather enabled: {weather}");
                    }
                }
                else
                {
                    if (hasWeather)
                    {
                        var oldWeather = nc.getWeather(weather);
                        var newWeatherList = new List<WeatherItem>(AWeather.GetValue<WeatherItem[]>());
                        var index = newWeatherList.IndexOf(oldWeather);
                        if (index >= 0)
                        {
                            oldWeather.destruct();
                            newWeatherList.RemoveAt(index);
                            AWeather.SetValue(newWeatherList.ToArray());
                            cur_weather.SetValue(SetResetBit(cur_weather.GetValue<int>(), (int)weather & 0x1F, false));
                            HLog.Debug($"Weather disabled: {weather}");
                        }
                    }
                }

                Flush();
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

            public static NightController GetNightController()
            {
                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return null;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return null;

                return m2d.NightCon;
            }

            public static void Flush()
            {
                try
                {
                    var nc = GetNightController();
                    if (nc == null)
                    {
                        HLog.Notice("NightController not found while synchronizing weather config.");
                        return;
                    }

                    _isApplying = true;
                    ConfigManager.SetWeatherWind.Value = nc.getWeather(WeatherItem.WEATHER.WIND) != null;
                    ConfigManager.SetWeatherThunder.Value = nc.getWeather(WeatherItem.WEATHER.THUNDER) != null;
                    ConfigManager.SetWeatherMist.Value = nc.getWeather(WeatherItem.WEATHER.MIST) != null;
                    ConfigManager.SetWeatherDrought.Value = nc.getWeather(WeatherItem.WEATHER.DROUGHT) != null;
                    ConfigManager.SetWeatherDenseMist.Value = nc.getWeather(WeatherItem.WEATHER.MIST_DENSE) != null;
                    ConfigManager.SetWeatherPlague.Value = nc.getWeather(WeatherItem.WEATHER.PLAGUE) != null;
                    _isApplying = false;

                    HLog.Debug("Weather config synchronized from game state.");
                }
                catch (Exception ex)
                {
                    _isApplying = false;
                    HLog.Error($"Unexpected error in {nameof(Flush)}.", ex);
                }
            }
        }
    }
}
