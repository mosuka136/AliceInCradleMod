using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭圣光爆发后的昏厥概率。
        /// 配置关闭该机制时直接返回 0 概率并跳过原计算。
        /// </summary>
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
