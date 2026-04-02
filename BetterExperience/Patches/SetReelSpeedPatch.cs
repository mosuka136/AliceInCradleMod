using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetReelSpeedPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.fineSpeed))]
            public static void SetReelSpeed(ref float reduce_level)
            {
                if (ConfigManager.SetReelSpeed.Value < 0f || ConfigManager.SetReelSpeed.Value > 1f)
                    return;

                reduce_level = ConfigManager.SetReelSpeed.Value;
            }
        }
    }
}
