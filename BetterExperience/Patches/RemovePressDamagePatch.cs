using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭玩家受到挤压伤害的入口。
        /// 只在配置关闭挤压伤害时跳过原方法，避免影响其他伤害路径。
        /// </summary>
        [HarmonyPatch]
        public class RemovePressDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), nameof(PR.applyPressDamage), new Type[] { typeof(NelAttackInfo), typeof(bool), typeof(int) })]
            public static bool Prefix()
            {
                try
                {
                    if (ConfigManager.EnablePressDamage.Value)
                        return true;

                    HLog.Debug($"{nameof(RemovePressDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemovePressDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
