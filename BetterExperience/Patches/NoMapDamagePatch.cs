using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭地图环境伤害。
        /// 返回 null 攻击信息并跳过原方法，使地刺、荆棘、电击等地图来源不再生成有效伤害。
        /// </summary>
        [HarmonyPatch]
        public class NoMapDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "applyDamageFromMap")]
            public static bool Prefix(ref AttackInfo __result)
            {
                try
                {
                    if (ConfigManager.EnableMapDamage.Value)
                        return true;

                    __result = null;

                    HLog.Debug($"{nameof(NoMapDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoMapDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
