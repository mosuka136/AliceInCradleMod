using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class InfiniteShieldPatch
        {
            [HarmonyPatch(typeof(M2Shield), "canGuard")]
            public static bool Prefix(M2Shield __instance, ref bool __result)
            {
                if (!ConfigManager.EnableInfiniteShield.Value)
                    return true;

                __instance.cure();

                return true;
            }
        }
    }
}
