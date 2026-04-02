using BetterExperience.BConfigManager;
using HarmonyLib;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SwitchDebugPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(X), nameof(X.init1))]
            public static void Prefix()
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
