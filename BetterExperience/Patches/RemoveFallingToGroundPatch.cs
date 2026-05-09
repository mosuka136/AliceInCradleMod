using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

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
                try
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
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveFallingToGroundPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
