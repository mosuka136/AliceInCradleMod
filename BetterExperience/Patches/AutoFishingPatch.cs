using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using HarmonyLib;
using m2d;
using nel;
using nel.mgm.fis;
using System;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在玩家已经开钓后代打原版流程：瞄准最近的鱼、抛竿、咬钩起竿、跟随头标、确认结算。
        /// 不拦截 NPC（Firstman）的 <see cref="FisPrRunner.IFishingListener"/>，也不主动发送 FISHPOND 事件。
        /// </summary>
        internal static class AutoFishing
        {
            internal static readonly object StatusKey = new object();

            private static readonly Type StateType;
            private static readonly object StateSuccess;
            private static readonly object StateWait;
            private static readonly bool StateLookupFailed;

            private static FisLure _trackedLure;
            private static float _holdTime;
            private static float _nearTime;
            private static float _diveTime;
            private static float _abortTime;
            private static bool _enabled;
            private static bool _statusShown;
            private static bool _loggedActivation;

            static AutoFishing()
            {
                try
                {
                    StateType = AccessTools.Inner(typeof(FisMain), "STATE");
                    StateSuccess = Enum.Parse(StateType, "G_SUCCESS");
                    StateWait = Enum.Parse(StateType, "G_WAIT");
                }
                catch (Exception ex)
                {
                    StateLookupFailed = true;
                    BLog.Error("Failed to resolve FisMain.STATE for auto fishing result confirm.", ex);
                }
            }

            internal static bool GetAutoFishing() => _enabled;

            internal static void SetAutoFishing(bool enabled)
            {
                try
                {
                    if (_enabled == enabled)
                    {
                        SyncStatus();
                        return;
                    }

                    _enabled = enabled;
                    Reset();
                    NoticeGUI.Show(
                        enabled ? TranslatorResource.AutoFishingEnabled : TranslatorResource.AutoFishingDisabled,
                        owner: StatusKey);
                    SyncStatus();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetAutoFishing)}", ex);
                }
            }

            internal static bool IsEnabled()
            {
                return ConfigManager.EnableBetterExperience?.Value == true && _enabled;
            }

            internal static void Reset()
            {
                _trackedLure = null;
                _holdTime = 0f;
                _nearTime = 0f;
                _diveTime = 0f;
                _abortTime = 0f;
            }

            internal static void TickLure(FisLure lure, float fcnt)
            {
                if (!IsEnabled() || lure == null || !lure.can_handle || !lure.isActive())
                    return;

                Track(lure);
                LogActivationOnce();

                if (lure.is_holding)
                {
                    _diveTime = 0f;
                    _abortTime = 0f;
                    TickHold(lure, fcnt);
                    return;
                }

                _holdTime = 0f;
                _nearTime = 0f;

                if (lure.is_eating)
                {
                    _diveTime = 0f;
                    _abortTime = 0f;
                    return;
                }

                if (lure.on_water)
                    TickWater(lure, fcnt);
            }

            internal static bool TryGetCatcherInput(FisPrRunner runner, out int input)
            {
                input = 0;
                if (runner == null || runner.FRun != null || !runner.is_main_pr || !IsEnabled())
                    return false;
                if (!runner.stateIs(FisLure.STATE.CATCHING) && (runner.FIS == null || !runner.FIS.isCatchingState()))
                    return false;

                var catchMgm = GetCatchMgm(runner.FIS);
                if (catchMgm?.Catcher == null)
                    return false;

                input = AutoFishingLogic.ComputeCatcherInput(
                    catchMgm.Catcher.position,
                    catchMgm.fishmarker_pos,
                    catchMgm.Catcher.range,
                    catchMgm.Catcher.isin_margin);
                return true;
            }

            internal static void TickResult(FisMain fis)
            {
                if (StateLookupFailed || fis == null || !IsEnabled() || !IsPlayerSession(fis))
                    return;

                object state = Traverse.Create(fis).Field("state").GetValue();
                float time = Traverse.Create(fis).Field("t").GetValue<float>();
                var level = GetBxLevel(fis);
                bool resultActive = fis.ResultUi != null && fis.ResultUi.gameObject.activeSelf;

                if (!AutoFishingLogic.ShouldConfirmResult(
                    true,
                    true,
                    Equals(state, StateSuccess),
                    resultActive,
                    level != null && level.isActive(),
                    level != null && level.isAnimating(),
                    time))
                    return;

                ConfirmResult(fis);
            }

            private static void TickHold(FisLure lure, float fcnt)
            {
                _holdTime += Positive(fcnt);
                var surf = GetSurf(lure.FIS);
                Vector2 pos = lure.getPosition();
                Vector3 fishPos = default;
                bool hasFish = surf != null && surf.getNearFishPos(pos, AutoFishingLogic.AimLookAhead, out fishPos);
                bool close = hasFish && (fishPos.z <= AutoFishingLogic.ThrowDistanceSq ||
                    AutoFishingLogic.IsCloseEnough(pos.x, pos.y, fishPos.x, fishPos.y, AutoFishingLogic.ThrowDistanceSq));

                if (hasFish && !close)
                    _nearTime = 0f;
                else if (hasFish && close)
                    _nearTime += Positive(fcnt);
                else
                    _nearTime = 0f;

                switch (AutoFishingLogic.DecideHoldAction(_holdTime, hasFish, close, _nearTime))
                {
                    case AutoFishingLogic.HoldAction.Aim:
                        lure.setEndPosition(
                            AutoFishingLogic.MoveToward(pos.x, fishPos.x, AutoFishingLogic.AimStep),
                            AutoFishingLogic.MoveToward(pos.y, fishPos.y, AutoFishingLogic.AimStep));
                        break;
                    case AutoFishingLogic.HoldAction.Throw:
                        lure.initThrow();
                        _holdTime = 0f;
                        _nearTime = 0f;
                        break;
                }
            }

            private static void TickWater(FisLure lure, float fcnt)
            {
                float dt = Positive(fcnt);
                if (lure.aborted)
                    _abortTime += dt;
                else
                    _abortTime = 0f;

                if (lure.is_diving)
                    _diveTime += dt;
                else
                    _diveTime = 0f;

                var surf = GetSurf(lure.FIS);
                int aiming = surf == null ? 0 : surf.getAimingCount(lure);
                switch (AutoFishingLogic.DecideWaterAction(
                    lure.is_diving,
                    lure.aborted,
                    lure.is_eating,
                    _diveTime,
                    _abortTime,
                    lure.t_ignored_time,
                    aiming))
                {
                    case AutoFishingLogic.WaterAction.Hook:
                        lure.initCatchPhase();
                        _diveTime = 0f;
                        break;
                    case AutoFishingLogic.WaterAction.Retrieve:
                        lure.releaseFromWater(true);
                        _abortTime = 0f;
                        break;
                }
            }

            private static void ConfirmResult(FisMain fis)
            {
                try
                {
                    SND.Ui.play("talk_progress");
                }
                catch (Exception ex)
                {
                    BLog.Debug($"{nameof(AutoFishing)} skipped result sound: {ex.Message}");
                }

                fis.ResultUi?.deactivate(true);
                FineFirstCatchFeedUi(fis);

                var changeState = AccessTools.Method(typeof(FisMain), "changeState", new[] { StateType });
                if (changeState == null)
                {
                    BLog.Warn("Auto fishing could not confirm the result: changeState was not found.");
                    return;
                }
                changeState.Invoke(fis, new[] { StateWait });
            }

            private static void FineFirstCatchFeedUi(FisMain fis)
            {
                try
                {
                    var traverse = Traverse.Create(fis);
                    var rec = traverse.Field("FisRec").GetValue<FisRecord>();
                    var catchMgm = traverse.Field("CatchMgm").GetValue<FisCatchMgm>();
                    var uiEq = traverse.Field("UiEq").GetValue<UiFishEquip>();
                    FishData fish = catchMgm == null ? default : catchMgm.getCurrentFishData();
                    FisGyotakuRecord record;
                    if (rec == null || uiEq == null || !fish.valid)
                        return;
                    if (rec.GetGyotaku().TryGetValue(fish.key, out record) && record.catch_count == 1)
                        uiEq.FineFeedString();
                }
                catch (Exception ex)
                {
                    BLog.Debug($"{nameof(AutoFishing)} skipped feed UI refresh: {ex.Message}");
                }
            }

            private static void Track(FisLure lure)
            {
                if (ReferenceEquals(_trackedLure, lure))
                    return;
                Reset();
                _trackedLure = lure;
            }

            private static bool IsPlayerSession(FisMain fis)
            {
                if (fis?.Mp == null)
                    return false;
                var target = Traverse.Create(fis).Field("TargetMv_").GetValue<M2Mover>();
                return target != null && (UnityEngine.Object)target == (UnityEngine.Object)fis.Mp.Pr;
            }

            private static FishSwimmingSurface GetSurf(FisMain fis)
            {
                return fis == null ? null : Traverse.Create(fis).Field("Surf").GetValue<FishSwimmingSurface>();
            }

            private static FisCatchMgm GetCatchMgm(FisMain fis)
            {
                return fis == null ? null : Traverse.Create(fis).Field("CatchMgm").GetValue<FisCatchMgm>();
            }

            private static UiFishLevel GetBxLevel(FisMain fis)
            {
                return fis == null ? null : Traverse.Create(fis).Field("BxLevel").GetValue<UiFishLevel>();
            }

            private static float Positive(float fcnt) => AutoFishingLogic.IsFinite(fcnt) && fcnt > 0f ? fcnt : 0f;

            private static void LogActivationOnce()
            {
                if (_loggedActivation)
                    return;
                BLog.Debug($"{nameof(AutoFishing)} applied.");
                _loggedActivation = true;
            }

            private static void SyncStatus()
            {
                if (IsEnabled())
                {
                    if (_statusShown)
                        return;
                    NoticeGUI.SetStatus(StatusKey, TranslatorResource.AutoFishingActive);
                    _statusShown = true;
                }
                else
                    HideStatus();
            }

            private static void HideStatus()
            {
                if (!_statusShown)
                    return;
                NoticeGUI.RemoveStatus(StatusKey);
                _statusShown = false;
            }
        }

        [HarmonyPatch]
        public class AutoFishingLurePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FisLure), nameof(FisLure.run))]
            public static void Postfix(FisLure __instance, float fcnt)
            {
                try
                {
                    AutoFishing.TickLure(__instance, fcnt);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoFishingLurePatch)}", ex);
                }
            }
        }

        [HarmonyPatch]
        public class AutoFishingInputPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(FisPrRunner), nameof(FisPrRunner.getInput))]
            public static bool Prefix(FisPrRunner __instance, ref bool ignore_reverse, ref int __result)
            {
                try
                {
                    if (!AutoFishing.TryGetCatcherInput(__instance, out int input))
                        return true;

                    ignore_reverse = true;
                    __result = input;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoFishingInputPatch)}", ex);
                    return true;
                }
            }
        }

        [HarmonyPatch]
        public class AutoFishingResultPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FisMain), nameof(FisMain.runPre))]
            public static void Postfix(FisMain __instance)
            {
                try
                {
                    AutoFishing.TickResult(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(AutoFishingResultPatch)}", ex);
                }
            }
        }
    }
}
