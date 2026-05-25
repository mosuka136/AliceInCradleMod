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
        /// 阻止 EP 伤害或增长路径。
        /// 配置开启时设置原方法返回值为 false 并跳过原逻辑。
        /// </summary>
        [HarmonyPatch]
        public class NoEpDamagePatch
        {
            [HarmonyPatch(typeof(EpManager), "applyEpDamage")]
            public static bool Prefix(ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableNoEpDamage.Value)
                        return true;

                    __result = false;

                    HLog.Debug($"{nameof(NoEpDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoEpDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
