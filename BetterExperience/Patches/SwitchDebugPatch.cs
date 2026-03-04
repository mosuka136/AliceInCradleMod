using BetterExperience.BepConfigManager;
using HarmonyLib;
using XX;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SwitchDebugPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(X), nameof(X.init1))]
            private static void Prefix()
            {
                if (ConfigManager.EnableDebugMode.Value)
                {
                    X.DEBUGANNOUNCE = true;
                    X.DEBUGTIMESTAMP = true;
                }
                else
                {
                    X.DEBUGTIMESTAMP = false;
                }
            }
        }
    }
}
