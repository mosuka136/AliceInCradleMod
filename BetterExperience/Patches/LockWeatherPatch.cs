using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 跳过原版天气轮换。不依赖调试监听器；控制页手动改天气仍写 <c>AWeather</c>。
        /// </summary>
        [HarmonyPatch]
        public class LockWeatherPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NightController), nameof(NightController.weatherShuffle))]
            public static bool WeatherShufflePrefix()
            {
                try
                {
                    if (!WorldStateLock.ShouldSkipWeatherShuffle(ConfigManager.EnableLockWeather?.Value == true))
                        return true;

                    BLog.Debug($"{nameof(LockWeatherPatch)} skipped weather shuffle.");
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(LockWeatherPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
