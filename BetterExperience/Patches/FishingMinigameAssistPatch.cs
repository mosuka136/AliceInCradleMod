using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using m2d;
using nel.mgm.fis;
using System;
using UnityEngine;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在每次收杆小游戏计算难度时读取配置，不把数值写进存档或鱼塘实例。
        /// 读档、切图后再钓只要配置仍开就会生效；自动钓鱼仍走实时控制。
        /// </summary>
        internal static class FishingMinigameAssist
        {
            private static bool _loggedMarker;
            private static bool _loggedCatcher;
            private static bool _loggedPunch;

            internal static bool MasterOn()
            {
                return ConfigManager.EnableBetterExperience?.Value == true;
            }

            internal static bool IsPlayerLure(FisCatchMgm mgm)
            {
                if (mgm == null)
                    return false;
                var lure = Traverse.Create(mgm).Field("LureHitting").GetValue<FisLure>();
                return lure != null && lure.is_mappr_mover;
            }

            internal static bool IsPlayerCatching(FisMain fis)
            {
                if (fis == null || !fis.isCatchingState() || fis.Mp == null)
                    return false;
                var target = Traverse.Create(fis).Field("TargetMv_").GetValue<M2Mover>();
                return target != null && (UnityEngine.Object)target == (UnityEngine.Object)fis.Mp.Pr;
            }

            internal static void ApplyMarker(ref FishData.MKInfo info)
            {
                if (!MasterOn() || ConfigManager.EnableEasierFishMarker?.Value != true)
                    return;
                FishingMinigameAssistLogic.ApplyEasierMarker(ref info);
                LogOnce(ref _loggedMarker, "easier fish marker");
            }

            internal static void ApplyCatcherAndMiss(FisMain fis)
            {
                if (!MasterOn() || !IsPlayerCatching(fis))
                    return;

                var mgm = Traverse.Create(fis).Field("CatchMgm").GetValue<FisCatchMgm>();
                if (!IsPlayerLure(mgm) || mgm.Catcher == null || mgm.HovPuncher == null)
                    return;

                float rangeMul = ConfigManager.SetFishingCatcherRangeMul?.Value ?? -1f;
                float margin = ConfigManager.SetFishingCatcherMargin?.Value ?? -1f;
                float missMul = ConfigManager.SetFishingMissDamageMul?.Value ?? -1f;

                float nextRange = FishingMinigameAssistLogic.ScaleOrKeep(
                    mgm.Catcher.range, rangeMul,
                    FishingMinigameAssistLogic.CatcherRangeMulMin,
                    FishingMinigameAssistLogic.CatcherRangeMulMax);
                float nextMargin = FishingMinigameAssistLogic.ReplaceOrKeep(
                    mgm.Catcher.isin_margin, margin,
                    FishingMinigameAssistLogic.CatcherMarginMin,
                    FishingMinigameAssistLogic.CatcherMarginMax);
                float nextMiss = FishingMinigameAssistLogic.ScaleOrKeep(
                    mgm.HovPuncher.miss_damage_speed, missMul,
                    FishingMinigameAssistLogic.MissDamageMulMin,
                    FishingMinigameAssistLogic.MissDamageMulMax);

                bool changed = nextRange != mgm.Catcher.range ||
                    nextMargin != mgm.Catcher.isin_margin ||
                    nextMiss != mgm.HovPuncher.miss_damage_speed;
                if (!changed)
                    return;

                mgm.Catcher.range = nextRange;
                mgm.Catcher.isin_margin = nextMargin;
                mgm.HovPuncher.miss_damage_speed = nextMiss;
                LogOnce(ref _loggedCatcher, "catcher/miss assist");
            }

            internal static void ApplyPunchNeed(FisHoverPuncher puncher, FisCatchMgm mgm)
            {
                if (!MasterOn() || puncher == null || !IsPlayerLure(mgm))
                    return;

                var punchNeed = Traverse.Create(puncher).Field("punch_need_count");
                float configured = ConfigManager.SetFishingPunchNeedCount?.Value ?? -1f;
                int current = punchNeed.GetValue<int>();
                int next = FishingMinigameAssistLogic.PunchCountOrKeep(current, configured);
                if (next == current)
                    return;

                punchNeed.SetValue(next);
                LogOnce(ref _loggedPunch, "punch need count");
            }

            private static void LogOnce(ref bool logged, string what)
            {
                if (logged)
                    return;
                BLog.Debug($"{nameof(FishingMinigameAssist)} applied: {what}.");
                logged = true;
            }
        }

        [HarmonyPatch]
        public class FishingMarkerAssistPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(FisCatchFishMarker), nameof(FisCatchFishMarker.activate))]
            public static void Prefix(FisCatchFishMarker __instance, ref FishData.MKInfo _Mki)
            {
                try
                {
                    if (!FishingMinigameAssist.IsPlayerLure(__instance?.Con))
                        return;
                    FishingMinigameAssist.ApplyMarker(ref _Mki);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FishingMarkerAssistPatch)}", ex);
                }
            }
        }

        [HarmonyPatch]
        public class FishingCatcherAssistPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FisMain), nameof(FisMain.FineMiniGameVariable))]
            public static void Postfix(FisMain __instance)
            {
                try
                {
                    FishingMinigameAssist.ApplyCatcherAndMiss(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FishingCatcherAssistPatch)}", ex);
                }
            }
        }

        [HarmonyPatch]
        public class FishingPunchAssistPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FisHoverPuncher), nameof(FisHoverPuncher.activate))]
            public static void Postfix(FisHoverPuncher __instance)
            {
                try
                {
                    FishingMinigameAssist.ApplyPunchNeed(__instance, __instance?.Con);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FishingPunchAssistPatch)}", ex);
                }
            }
        }
    }
}
