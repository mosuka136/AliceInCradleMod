using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetReelSpeedPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.fineSpeed))]
            private static void SetReelSpeed(ref float reduce_level)
            {
                if (ConfigManager.SetReelSpeed.Value < 0f || ConfigManager.SetReelSpeed.Value > 1f)
                    return;

                reduce_level = ConfigManager.SetReelSpeed.Value;
            }
        }
    }
}
