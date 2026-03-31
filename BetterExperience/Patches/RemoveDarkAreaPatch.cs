using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveDarkAreaPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PR), nameof(PR.fineLightSize))]
            public static void Postfix(PR __instance)
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
