using nel;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 拘束挣脱 Gacha 跳过规则。不触碰 Unity 或 AbsorbManager 实例。
    /// 不可挣脱、事件脚本 QTE、自慰小游戏不跳过。
    /// </summary>
    internal static class AbsorbGachaSkipLogic
    {
        internal static bool ShouldFinish(
            bool skipOn,
            PrGachaItem.TYPE type,
            bool evAssign,
            bool isMasturbate,
            bool isEventGacha)
        {
            if (!skipOn)
                return false;
            if (type == PrGachaItem.TYPE.CANNOT_RELEASE)
                return false;
            if (evAssign || isMasturbate || isEventGacha)
                return false;
            return true;
        }

        internal static float FinishedCount(int count0)
        {
            return count0;
        }

        /// <summary>
        /// 跳过对象要把 run() 当成已完成，否则 AbsorbManager 会把 gacha_releaseable 打回去。
        /// </summary>
        internal static bool ShouldTreatRunAsComplete(bool skipEligible)
        {
            return skipEligible;
        }

        /// <summary>
        /// 有可跳过的挣脱条、且所有仍在用的条都已可释放时，立刻走原版 GACHA 释放。
        /// 不要求 CameraRenderBinder 已经建好。
        /// </summary>
        internal static bool ShouldForceContainerRelease(
            bool skipOn,
            bool anySkipEligibleUseable,
            bool allUseableReleaseable)
        {
            return skipOn && anySkipEligibleUseable && allUseableReleaseable;
        }
    }
}
