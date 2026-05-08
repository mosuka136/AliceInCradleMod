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
            private static bool _hasLoggedActivation = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "checkEnemySink")]
            public static bool Prefix()
            {
                if(ConfigManager.EnableFallingToGround.Value)
                    return true;

                if (!_hasLoggedActivation)
                {
                    HLog.Debug($"{nameof(RemoveFallingToGroundPatch)} applied.");
                    _hasLoggedActivation = true;
                }

                return false;
            }
        }
    }
}
