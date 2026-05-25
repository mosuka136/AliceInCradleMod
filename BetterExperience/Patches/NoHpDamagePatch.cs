using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 阻止非玩家来源造成 HP 伤害。
        /// 玩家自身来源会放行，避免破坏对敌人造成伤害的逻辑。
        /// </summary>
        [HarmonyPatch]
        public class NoHpDamagePatch
        {
            /// <summary>
            /// 使用反射查找目标方法，以便目标签名在引用信息不完整时仍能安全失败。
            /// </summary>
            public static IEnumerable<MethodBase> TargetMethods()
            {
                var m = AccessTools.Method(typeof(nel.PR), "applyHpDamage");
                if (m == null)
                {
                    HLog.Error("applyHpDamage not found on PR.");
                    yield break;
                }

                yield return m;
            }

            public static bool Prefix(AttackInfo Atk)
            {
                if (GetSource(Atk) is PR)
                    return true;

                try
                {
                    if (!ConfigManager.EnableNoHpDamage.Value)
                        return true;

                    HLog.Debug($"{nameof(NoHpDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoHpDamagePatch)}.", ex);
                    return true;
                }
            }

            public static object GetSource(AttackInfo attack_info)
            {
                if (attack_info == null)
                    return null;

                if (attack_info is NelAttackInfo atk)
                {
                    if (atk.Caster != null)
                        return atk.Caster;

                    if (atk.PublishMagic?.Caster != null)
                        return atk.PublishMagic.Caster;
                }

                if (attack_info.AttackFrom != null)
                    return attack_info.AttackFrom;

                return null;
            }
        }
    }
}
