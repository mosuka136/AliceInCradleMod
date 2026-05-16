using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在护盾可防御判定前修复护盾状态，实现持续可用。
        /// 该补丁不跳过原方法，只在原判定前调用游戏自带的恢复逻辑。
        /// </summary>
        [HarmonyPatch]
        public class InfiniteShieldPatch
        {
            [HarmonyPatch(typeof(M2Shield), "canGuard")]
            public static void Prefix(M2Shield __instance, ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableInfiniteShield.Value)
                        return;

                    __instance.cure();

                    HLog.Debug($"{nameof(InfiniteShieldPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(InfiniteShieldPatch)}", ex);
                }
            }
        }
    }
}
