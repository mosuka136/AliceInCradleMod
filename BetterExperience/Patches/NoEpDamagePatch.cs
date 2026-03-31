using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class NoEpDamagePatch
        {
            [HarmonyPatch(typeof(EpManager), "applyEpDamage")]
            public static bool Prefix(ref bool __result)
            {
                if (!ConfigManager.EnableNoEpDamage.Value)
                    return true;

                __result = false;
                return false;
            }
        }
    }
}
