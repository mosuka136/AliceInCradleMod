using BetterExperience.BepConfigManager;
using HarmonyLib;
using m2d;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class NoMapDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "applyDamageFromMap")]
            public static bool Prefix(ref AttackInfo __result)
            {
                if (ConfigManager.EnableMapDamage.Value)
                    return true;

                __result = null;

                return false;
            }
        }
    }
}
