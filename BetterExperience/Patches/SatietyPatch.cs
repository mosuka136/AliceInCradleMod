using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭食物消化。水分消化、料理预览用的临时胃袋、椅子清空胃袋仍走原版。
        /// </summary>
        [HarmonyPatch]
        public class SatietyPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Stomach), nameof(Stomach.progress))]
            public static bool ProgressPrefix(
                Stomach __instance,
                float lvl_cost,
                bool fine_pr_state,
                out float water_progress,
                bool announce,
                bool only_water,
                float katayori01,
                ref float __result)
            {
                water_progress = 0f;
                try
                {
                    if (!SatietyLogic.ShouldSkipProgress(
                        ConfigManager.EnableNoSatietyDrain?.Value == true,
                        only_water,
                        ReferenceEquals(__instance, Stomach.Temporary)))
                        return true;

                    __result = 0f;
                    BLog.Debug($"{nameof(SatietyPatch)} skipped food satiety progress.");
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ProgressPrefix)}", ex);
                    return true;
                }
            }
        }
    }
}
