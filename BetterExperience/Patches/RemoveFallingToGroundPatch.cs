using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class RemoveFallingToGroundPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "checkEnemySink")]
            private static bool Prefix()
            {
                if(ConfigManager.EnableFallingToGround.Value)
                    return true;

                return false;
            }
        }
    }
}
