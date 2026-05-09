using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveHolyBurstFaintPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MDAT), "calcBurstFaintedRatio")]
            public static bool Prefix(ref float __result)
            {
                try
                {
                    if (ConfigManager.EnableHolyBurstFaint.Value)
                        return true;

                    __result = 0f;

                    HLog.Debug($"{nameof(RemoveHolyBurstFaintPatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveHolyBurstFaintPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
