using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetReelSpeedPatch
        {
            private static bool _hasLoggedActivation = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.fineSpeed))]
            public static void SetReelSpeed(ref float reduce_level)
            {
                try
                {
                    if (ConfigManager.SetReelSpeed.Value < 0f || ConfigManager.SetReelSpeed.Value > 1f)
                        return;

                    reduce_level = ConfigManager.SetReelSpeed.Value;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug($"{nameof(SetReelSpeedPatch)} applied.");
                        _hasLoggedActivation = true;
                    }
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetReelSpeedPatch)}", ex);
                }
            }
        }
    }
}
