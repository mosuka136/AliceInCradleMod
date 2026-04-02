using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveFallingToGroundPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "checkEnemySink")]
            public static bool Prefix()
            {
                if(ConfigManager.EnableFallingToGround.Value)
                    return true;

                return false;
            }
        }
    }
}
