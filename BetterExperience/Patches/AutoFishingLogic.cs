using System;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 自动钓鱼的纯决策：瞄准、起竿/收回、套圈方向和结算确认。
    /// 不触碰 Unity 或游戏对象，便于在测试宿主中验证阈值。
    /// </summary>
    internal static class AutoFishingLogic
    {
        internal const float HoldDelay = 50f;
        internal const float ThrowDelay = 25f;
        internal const float HookDelay = 12f;
        internal const float AbortDelay = 60f;
        internal const float IgnoreTimeout = 360f;
        internal const float AimStep = 1.12f;
        internal const float AimLookAhead = 40f;
        internal const float ThrowDistanceSq = 25f;
        internal const float CatcherCenterRatio = 0.35f;
        internal const float CatcherDeadzone = 0.012f;
        internal const float ResultConfirmTime = 1060f;

        internal enum HoldAction
        {
            Wait,
            Aim,
            Throw
        }

        internal enum WaterAction
        {
            Wait,
            Hook,
            Retrieve
        }

        internal static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        internal static float MoveToward(float current, float target, float step)
        {
            if (!IsFinite(current) || !IsFinite(target) || !IsFinite(step) || step < 0f)
                return current;

            float delta = target - current;
            if (Math.Abs(delta) <= step)
                return target;
            return current + Math.Sign(delta) * step;
        }

        internal static bool IsCloseEnough(float x, float y, float targetX, float targetY, float distanceSq)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(targetX) || !IsFinite(targetY) || !IsFinite(distanceSq))
                return false;

            float dx = targetX - x;
            float dy = targetY - y;
            return dx * dx + dy * dy <= distanceSq;
        }

        internal static HoldAction DecideHoldAction(float holdTime, bool hasFish, bool closeEnough, float nearTime)
        {
            if (holdTime < HoldDelay || !hasFish)
                return HoldAction.Wait;
            if (!closeEnough)
                return HoldAction.Aim;
            return nearTime >= ThrowDelay ? HoldAction.Throw : HoldAction.Wait;
        }

        internal static WaterAction DecideWaterAction(
            bool diving,
            bool aborted,
            bool eating,
            float diveTime,
            float abortTime,
            float ignoredTime,
            int aimingCount)
        {
            if (eating)
                return WaterAction.Wait;
            if (aborted)
                return abortTime >= AbortDelay ? WaterAction.Retrieve : WaterAction.Wait;
            if (diving)
                return diveTime >= HookDelay ? WaterAction.Hook : WaterAction.Wait;
            if (ignoredTime >= IgnoreTimeout && aimingCount <= 0)
                return WaterAction.Retrieve;
            return WaterAction.Wait;
        }

        /// <summary>
        /// 返回套圈左右输入：1 向右，-1 向左，0 松手。
        /// 已经罩住鱼标且接近中心时松手，避免惯性把圈带出鱼标。
        /// </summary>
        internal static int ComputeCatcherInput(float catcherPos, float fishPos, float range, float margin)
        {
            if (!IsFinite(catcherPos) || !IsFinite(fishPos) || !IsFinite(range) || !IsFinite(margin))
                return 0;

            float error = fishPos - catcherPos;
            float abs = Math.Abs(error);
            float inner = Math.Max(0f, range + margin) * CatcherCenterRatio;
            if (abs <= Math.Max(CatcherDeadzone, inner))
                return 0;
            return error > 0f ? 1 : -1;
        }

        internal static bool ShouldConfirmResult(
            bool autoEnabled,
            bool isPlayer,
            bool isSuccess,
            bool resultActive,
            bool levelActive,
            bool levelAnimating,
            float successTime)
        {
            return autoEnabled && isPlayer && isSuccess && resultActive && levelActive &&
                !levelAnimating && successTime >= ResultConfirmTime;
        }
    }
}
