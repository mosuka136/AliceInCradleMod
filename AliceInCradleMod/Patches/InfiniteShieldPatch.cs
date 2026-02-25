using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class InfiniteShieldPatch
        {
            [HarmonyPatch(typeof(M2Shield), "canGuard")]
            private static bool Prefix(M2Shield __instance, ref bool __result)
            {
                if (!ConfigManager.EnableInfiniteShield.Value)
                    return true;

                __instance.cure();

                return true;
            }
        }
    }
}
