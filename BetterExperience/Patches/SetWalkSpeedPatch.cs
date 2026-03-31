using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetWalkSpeedPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PR), "calcWalkSpeed")]
            public static void Postfix(int move_aim_ex, ref float __result)
            {
                if (ConfigManager.SetPlayerWalkSpeed.Value <= 0f)
                    return;

                if (move_aim_ex == 0)
                    return;

                __result *= ConfigManager.SetPlayerWalkSpeed.Value;
            }
        }
    }
}
