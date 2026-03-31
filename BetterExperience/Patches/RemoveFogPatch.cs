using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
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
                if (ConfigManager.EnableVisualImpactOfFog.Value)
                    return;

                __instance.DrM?.destruct();
                __instance.DrM = null;
            }
        }
    }
}
