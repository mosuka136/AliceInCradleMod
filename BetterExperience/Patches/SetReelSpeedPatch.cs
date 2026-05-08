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
            private static bool _hasLoggedActivation = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.fineSpeed))]
            public static void SetReelSpeed(ref float reduce_level)
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
        }
    }
}
