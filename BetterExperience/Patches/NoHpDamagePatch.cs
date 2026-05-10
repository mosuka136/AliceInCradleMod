using BetterExperience.BConfigManager;
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
        [HarmonyPatch]
        public class NoHpDamagePatch
        {
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
