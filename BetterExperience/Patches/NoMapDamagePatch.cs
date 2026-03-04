using BetterExperience.BepConfigManager;
using HarmonyLib;
using m2d;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class NoMapDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "applyDamageFromMap")]
            private static bool Prefix(ref AttackInfo __result)
            {
                if (ConfigManager.EnableMapDamage.Value)
                    return true;

                __result = null;

                return false;
            }
        }
    }
}
