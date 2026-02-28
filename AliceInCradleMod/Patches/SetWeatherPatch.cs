using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using XX;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetWeatherPatch
        {
            private static bool _initialized = false;
            private static bool _isApplying = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += Flush;


                ConfigManager.SetWeatherWind.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.WIND, ConfigManager.SetWeatherWind.Value);
                };
                ConfigManager.SetWeatherThunder.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.THUNDER, ConfigManager.SetWeatherThunder.Value);
                };
                ConfigManager.SetWeatherMist.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.MIST, ConfigManager.SetWeatherMist.Value);
                };
                ConfigManager.SetWeatherDrought.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.DROUGHT, ConfigManager.SetWeatherDrought.Value);
                };
                ConfigManager.SetWeatherDenseMist.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.MIST_DENSE, ConfigManager.SetWeatherDenseMist.Value);
                };
                ConfigManager.SetWeatherPlague.SettingChanged += (s, e) =>
                {
                    if (!_isApplying)
                        SetWeather(WeatherItem.WEATHER.PLAGUE, ConfigManager.SetWeatherPlague.Value);
                };

                _initialized = true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(NightController), nameof(NightController.weatherShuffle))]
            private static void WeatherShufflePostfix()
            {
                Flush();
            }

            public static void SetWeather(WeatherItem.WEATHER weather, bool setWeather)
            {
                var nc = GetNightController();
                if (nc == null)
                    return;

                var nct = Traverse.Create(nc);
                if (nct == null)
                    return;

                var cur_weather = nct.Field("cur_weather_");
                var AWeather = nct.Field("AWeather");

                if (setWeather)
                {
                    if ((cur_weather.GetValue<int>() & 1 << ((int)weather & 0x1F)) == 0)
                    {
                        var weatherItemList = new List<WeatherItem>(AWeather.GetValue<WeatherItem[]>());
                        var weatherItem = new WeatherItem(weather, nct.Field("dlevel").GetValue<int>() + nct.Field("dlevel_add").GetValue<int>()).initS(null);
                        if (weatherItem.get_conflict() != 0)
                        {
                            for (int index = weatherItemList.Count - 1; index >= 0; --index)
                            {
                                if ((1 << ((int)weatherItemList[index].weather & 0x1F) & (int)weatherItem.get_conflict()) != 0)
                                {
                                    cur_weather.SetValue(cur_weather.GetValue<int>() & ~(1 << ((int)weatherItemList[index].weather & 0x1F)));
                                    weatherItemList[index].destruct();
                                    weatherItemList.RemoveAt(index);
                                }
                            }
                        }
                        weatherItemList.Add(weatherItem);
                        AWeather.SetValue(weatherItemList.ToArray());
                        cur_weather.SetValue(cur_weather.GetValue<int>() | 1 << ((int)weather & 0x1F));
                        weatherItem.showLog();
                    }
                }
                else if ((cur_weather.GetValue<int>() & 1 << ((int)weather & 0x1F)) != 0)
                {
                    int s;
                    var oldWeather = nc.getWeather(weather);
                    if (oldWeather != null && (s = X.isinC<WeatherItem>(AWeather.GetValue<WeatherItem[]>(), oldWeather)) >= 0)
                    {
                        oldWeather.destruct();
                        WeatherItem[] newWeather = AWeather.GetValue<WeatherItem[]>();
                        X.splice(ref newWeather, s, 1);
                        AWeather.SetValue(newWeather);
                        cur_weather.SetValue(cur_weather.GetValue<int>() & ~(1 << ((int)weather & 0x1F)));
                    }
                }

                Flush();
            }

            private static NightController GetNightController()
            {
                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return null;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return null;

                return m2d.NightCon;
            }

            private static void Flush()
            {
                var nc = GetNightController();
                if (nc == null)
                    return;

                _isApplying = true;
                ConfigManager.SetWeatherWind.Value = nc.getWeather(WeatherItem.WEATHER.WIND) != null;
                ConfigManager.SetWeatherThunder.Value = nc.getWeather(WeatherItem.WEATHER.THUNDER) != null;
                ConfigManager.SetWeatherMist.Value = nc.getWeather(WeatherItem.WEATHER.MIST) != null;
                ConfigManager.SetWeatherDrought.Value = nc.getWeather(WeatherItem.WEATHER.DROUGHT) != null;
                ConfigManager.SetWeatherDenseMist.Value = nc.getWeather(WeatherItem.WEATHER.MIST_DENSE) != null;
                ConfigManager.SetWeatherPlague.Value = nc.getWeather(WeatherItem.WEATHER.PLAGUE) != null;
                _isApplying = false;
            }
        }
    }
}
