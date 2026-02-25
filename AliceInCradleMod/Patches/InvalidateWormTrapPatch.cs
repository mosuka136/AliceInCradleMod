using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class InvalidateWormTrapPatch
        {
            [HarmonyPatch(typeof(M2WormTrap), "isCovering")]
            private static bool Prefix(ref bool __result)
            {
                if (ConfigManager.EnableWormTrap.Value)
                    return true;

                __result = false;
                return false;
            }
        }
    }
}
