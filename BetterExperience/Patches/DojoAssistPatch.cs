using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel.mgm.dojo;
using System;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 道场小游戏的手动辅助（非全自动代打），两个配置开关相互独立、持久化保存：
        /// 判定放宽——把出拳快/慢窗口从原版 ±14 帧放大到 3 倍，出现动画和失败倒计时不放宽；
        /// 自动命中——GO 拍后自动打出克制手势，自己按任意键也会改成正确手势。
        /// 教程（skill_key == _tuto）完全不介入，强制失败与按键教学保持原版。
        /// 关掉配置或总开关后字段不再改写，判定回到原版。
        /// </summary>
        internal static class DojoAssist
        {
            private static readonly FieldInfo TInputAlloc = AccessTools.Field(typeof(DjGM), "t_input_alloc");
            private static readonly FieldInfo HandTypeBits = AccessTools.Field(typeof(DjGM), "hand_type_bits");
            private static readonly MethodInfo HitHand = AccessTools.Method(typeof(DjGM), "hitHand", new[] { typeof(int) });

            private static bool _loggedWider;
            private static bool _loggedAutoHit;

            internal static bool WiderOn => IsOn(ConfigManager.EnableDojoWiderTiming);
            internal static bool AutoHitOn => IsOn(ConfigManager.EnableDojoAutoHit);

            internal static bool ShouldAssist(DjGM gm)
            {
                return gm != null && gm.isActive() && !gm.is_tuto;
            }

            internal static void ApplyHitAssist(DjGM gm, ref int rpc)
            {
                if (!ShouldAssist(gm) || (!WiderOn && !AutoHitOn))
                    return;

                if (WiderOn && TInputAlloc != null)
                {
                    float current = (float)TInputAlloc.GetValue(gm);
                    float adjusted = DojoAssistLogic.AdjustTiming(current, true);
                    if (adjusted != current)
                    {
                        TInputAlloc.SetValue(gm, adjusted);
                        LogOnce(ref _loggedWider, "wider timing");
                    }
                }

                if (AutoHitOn && gm.HK != null)
                {
                    rpc = DojoAssistLogic.ResolvePlayerHand(rpc, gm.HK.cur_hand, true);
                    LogOnce(ref _loggedAutoHit, "correct hand");
                }
            }

            internal static void TryAutoHit(DjGM gm)
            {
                if (!ShouldAssist(gm) || !AutoHitOn || !gm.isHitableState())
                    return;
                if (gm.HK == null || !gm.HK.hk_generated || HitHand == null || TInputAlloc == null || HandTypeBits == null)
                    return;

                float t = (float)TInputAlloc.GetValue(gm);
                uint bits = (uint)HandTypeBits.GetValue(gm);
                if (!DojoAssistLogic.ShouldAutoHit(t, bits))
                    return;

                int hand = DojoAssistLogic.WinningHand(gm.HK.cur_hand);
                HitHand.Invoke(gm, new object[] { hand });
                LogOnce(ref _loggedAutoHit, "auto hit");
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }

            private static void LogOnce(ref bool logged, string what)
            {
                if (logged)
                    return;
                BLog.Debug($"{nameof(DojoAssist)} applied: {what}.");
                logged = true;
            }
        }

        /// <summary>
        /// 出拳时改写手势并夹紧快/慢窗口。hitHand 为游戏 private 方法。
        /// </summary>
        [HarmonyPatch]
        public class DojoHitAssistPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(DjGM), "hitHand", new Type[] { typeof(int) })]
            public static void Prefix(DjGM __instance, ref int rpc)
            {
                try
                {
                    DojoAssist.ApplyHitAssist(__instance, ref rpc);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(DojoHitAssistPatch)}", ex);
                }
            }
        }

        /// <summary>
        /// 玩家未出拳时，在 GO 拍后的原版窗口内补一次克制手。
        /// </summary>
        [HarmonyPatch]
        public class DojoAutoHitPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(DjGM), nameof(DjGM.run))]
            public static void Postfix(DjGM __instance)
            {
                try
                {
                    DojoAssist.TryAutoHit(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(DojoAutoHitPatch)}", ex);
                }
            }
        }
    }
}
