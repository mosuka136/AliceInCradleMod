using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 拘束挣脱 Gacha 跳过：把进度直接拉满，由原版 AbsorbManager 走释放流程。
        /// 事件脚本 QTE 和自慰小游戏不跳过。关掉配置后新的 Gacha 恢复手动。
        /// </summary>
        internal static class AbsorbGachaSkip
        {
            private static readonly FieldInfo CountField = AccessTools.Field(typeof(PrGachaItem), "count");
            private static readonly FieldInfo Count0Field = AccessTools.Field(typeof(PrGachaItem), "count0");
            private static readonly FieldInfo CorruptField = AccessTools.Field(typeof(PrGachaItem), "corrupt_val");
            private static readonly FieldInfo CorruptLockField = AccessTools.Field(typeof(PrGachaItem), "t_lock_coruppt");

            private static bool _logged;

            internal static bool SkipOn => IsOn(ConfigManager.EnableAbsorbGachaSkip);

            internal static bool ShouldFinish(PrGachaItem gacha)
            {
                if (gacha == null || gacha.Con == null)
                    return false;
                var listener = gacha.Con.Listener;
                return AbsorbGachaSkipLogic.ShouldFinish(
                    SkipOn,
                    gacha.type,
                    gacha.ev_assign,
                    listener is M2PrMasturbate,
                    listener is M2PrGachaEventHandler);
            }

            internal static void TryFinish(PrGachaItem gacha)
            {
                if (!ShouldFinish(gacha))
                    return;
                if (CountField == null || Count0Field == null)
                    return;

                int count0 = (int)Count0Field.GetValue(gacha);
                if (!gacha.isFinished())
                    CountField.SetValue(gacha, AbsorbGachaSkipLogic.FinishedCount(count0));
                CorruptField?.SetValue(gacha, 0f);
                CorruptLockField?.SetValue(gacha, 0f);
                if (gacha.Con != null)
                    gacha.Con.gacha_releaseable = true;
                LogOnce();
            }

            internal static bool TryForceRelease(AbsorbManagerContainer con)
            {
                if (con == null || !SkipOn || con.Length <= 0)
                    return false;

                int length = con.Length;
                bool anySkip = false;
                bool allUseableReleaseable = true;
                for (int i = 0; i < length; i++)
                {
                    var absorb = con.GetManagerItem(i);
                    if (absorb == null || !absorb.isActive())
                        continue;
                    var gacha = absorb.get_Gacha();
                    if (gacha == null || !gacha.isUseable())
                        continue;
                    if (ShouldFinish(gacha))
                    {
                        anySkip = true;
                        TryFinish(gacha);
                        absorb.gacha_releaseable = true;
                    }
                    else if (!absorb.gacha_releaseable)
                    {
                        allUseableReleaseable = false;
                    }
                }

                if (!AbsorbGachaSkipLogic.ShouldForceContainerRelease(SkipOn, anySkip, allUseableReleaseable))
                    return false;

                for (int i = length - 1; i >= 0; i--)
                {
                    var absorb = con.GetManagerItem(i);
                    var gacha = absorb?.get_Gacha();
                    if (gacha == null || !gacha.isUseable())
                        continue;
                    gacha.releaseEffect();
                    absorb.destruct();
                }

                con.release_type |= AbsorbManagerContainer.RELEASE_TYPE.GACHA;
                LogOnce();

                bool anyActive = false;
                for (int i = 0; i < con.Length; i++)
                {
                    var absorb = con.GetManagerItem(i);
                    if (absorb != null && absorb.isActive())
                    {
                        anyActive = true;
                        break;
                    }
                }
                if (anyActive)
                    return false;

                con.clear();
                return true;
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }

            private static void LogOnce()
            {
                if (_logged)
                    return;
                BLog.Debug($"{nameof(AbsorbGachaSkip)} applied: finish absorb gacha.");
                _logged = true;
            }
        }

        [HarmonyPatch]
        public class AbsorbGachaSkipPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PrGachaItem), nameof(PrGachaItem.run))]
            public static void Prefix(PrGachaItem __instance)
            {
                try
                {
                    AbsorbGachaSkip.TryFinish(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AbsorbGachaSkipPatch)} prefix", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PrGachaItem), nameof(PrGachaItem.run))]
            public static void Postfix(PrGachaItem __instance, ref bool __result)
            {
                try
                {
                    if (!AbsorbGachaSkipLogic.ShouldTreatRunAsComplete(AbsorbGachaSkip.ShouldFinish(__instance)))
                        return;
                    __result = false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AbsorbGachaSkipPatch)} postfix", ex);
                }
            }
        }

        /// <summary>
        /// 原版只在 Gacha 绘制绑定存在时才真正释放；跳过时同一帧拆掉拘束。
        /// </summary>
        [HarmonyPatch]
        public class AbsorbGachaForceReleasePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AbsorbManagerContainer), nameof(AbsorbManagerContainer.runAbsorbPr))]
            public static void Postfix(AbsorbManagerContainer __instance, ref bool __result)
            {
                try
                {
                    if (AbsorbGachaSkip.TryForceRelease(__instance))
                        __result = false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AbsorbGachaForceReleasePatch)}", ex);
                }
            }
        }
    }
}
