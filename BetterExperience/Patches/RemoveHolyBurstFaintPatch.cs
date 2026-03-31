using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveHolyBurstFaintPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MDAT), "calcBurstFaintedRatio")]
            static bool Perfix(ref float __result)
            {
                if (ConfigManager.EnableHolyBurstFaint.Value)
                    return true;

                __result = 0f;
                return false;
            }
        }
    }
}
