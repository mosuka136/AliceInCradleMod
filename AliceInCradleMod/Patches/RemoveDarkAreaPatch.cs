using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class RemoveDarkAreaPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PR), nameof(PR.fineLightSize))]
            private static void Postfix(PR __instance)
            {
                if (ConfigManager.EnableDarkArea.Value)
                    return;

                if (__instance.NM2D.map_dark_area)
                {
                    __instance.MyLight.Col.Set(2866067885);
                    int light_dep_size = 1000;
                    Traverse.Create(__instance).Field("light_dep_size").SetValue(light_dep_size);
                    __instance.MyLight.radius = light_dep_size;
                }
            }
        }
    }
}
