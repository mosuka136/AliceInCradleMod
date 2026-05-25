using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭虫墙覆盖判定。
        /// 配置关闭虫墙时直接返回 false 并跳过原方法，避免后续逻辑继续按被覆盖处理玩家。
        /// </summary>
        [HarmonyPatch]
        public class InvalidateWormTrapPatch
        {
            [HarmonyPatch(typeof(M2WormTrap), "isCovering")]
            public static bool Prefix(ref bool __result)
            {
                try
                {
                    if (ConfigManager.EnableWormTrap.Value)
                        return true;

                    __result = false;

                    HLog.Debug($"{nameof(InvalidateWormTrapPatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(InvalidateWormTrapPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
