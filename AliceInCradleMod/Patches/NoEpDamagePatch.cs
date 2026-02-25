using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class NoEpDamagePatch
        {
            [HarmonyPatch(typeof(EpManager), "applyEpDamage")]
            private static bool Prefix(ref bool __result)
            {
                if (!ConfigManager.EnableNoEpDamage.Value)
                    return true;

                __result = false;
                return false;
            }
        }
    }
}
