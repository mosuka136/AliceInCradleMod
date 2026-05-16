using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 移除天气雾效的绘制对象。
        /// 通过 WeatherItem 构造后销毁 DrM，保留天气本身但去掉遮挡视野的视觉影响。
        /// </summary>
        [HarmonyPatch]
        public class RemoveFogPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(
                typeof(WeatherItem), 
                MethodType.Constructor, 
                new Type[] { typeof(WeatherItem.WEATHER), typeof(int), typeof(WeatherItem.WeatherDescription)})]
            public static void WeatherItemPostfix(WeatherItem __instance)
            {
                try
                {
                    if (ConfigManager.EnableVisualImpactOfFog.Value)
                        return;

                    __instance.DrM?.destruct();
                    __instance.DrM = null;

                    HLog.Debug($"{nameof(RemoveFogPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveFogPatch)}", ex);
                }
            }
        }
    }
}
