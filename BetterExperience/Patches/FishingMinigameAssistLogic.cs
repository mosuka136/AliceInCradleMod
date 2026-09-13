using nel.mgm.fis;
using System;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 钓鱼收杆小游戏的数值调整：鱼标、套圈、掉条和收杆次数。
    /// 不触碰 Unity；配置哨兵 -1 表示保持游戏当前计算结果。
    /// </summary>
    internal static class FishingMinigameAssistLogic
    {
        internal const float MarkerSwitchMul = 1.6f;
        internal const float MarkerEaseMul = 0.8f;
        internal const float CatcherRangeMulMin = 0.5f;
        internal const float CatcherRangeMulMax = 3f;
        internal const float CatcherMarginMin = 0f;
        internal const float CatcherMarginMax = 0.2f;
        internal const float MissDamageMulMin = 0.1f;
        internal const float MissDamageMulMax = 2f;
        internal const int PunchNeedMin = 1;
        internal const int PunchNeedMax = 6;

        internal static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        internal static bool TryConfigured(float configured, float min, float max, out float value)
        {
            value = configured;
            return IsFinite(configured) && configured >= min && configured <= max;
        }

        internal static float ScaleOrKeep(float original, float configured, float min, float max)
        {
            if (!IsFinite(original) || !TryConfigured(configured, min, max, out float mul))
                return original;

            float result = original * mul;
            return IsFinite(result) ? result : original;
        }

        internal static float ReplaceOrKeep(float original, float configured, float min, float max)
        {
            if (!TryConfigured(configured, min, max, out float value))
                return original;
            return value;
        }

        internal static int PunchCountOrKeep(int original, float configured)
        {
            if (!IsFinite(configured) || configured < 0f)
                return original;

            int count = (int)Math.Round(configured);
            if (count < PunchNeedMin)
                count = PunchNeedMin;
            if (count > PunchNeedMax)
                count = PunchNeedMax;
            return count;
        }

        /// <summary>
        /// 拉长切换间隔、缩短跳跃，保持每个 min/max 区间宽度，避免后续逻辑依赖差值时被破坏。
        /// </summary>
        internal static void ApplyEasierMarker(ref FishData.MKInfo mki)
        {
            ShiftRange(ref mki.t_switch_min, ref mki.t_switch_max, MarkerSwitchMul);
            ShiftRange(ref mki.jump01_min, ref mki.jump01_max, MarkerEaseMul);
            ShiftRange(ref mki.switchspeed01_min, ref mki.switchspeed01_max, MarkerEaseMul);
            ShiftRange(ref mki.move_fast_range_min, ref mki.move_fast_range_max, MarkerEaseMul);

            mki.switch_another_ratio *= MarkerEaseMul;
            mki.switch_another_lock *= MarkerSwitchMul;
            mki.force_goto_reverse_ratio *= MarkerEaseMul;
            mki.switch_slower_ratio *= MarkerSwitchMul;

            ShiftRange(ref mki.switch_slower_multiple_min, ref mki.switch_slower_multiple_max, MarkerSwitchMul);

            mki.first_speed_ratio *= MarkerEaseMul;
            ShiftRange(ref mki.fistjump_multiple_min, ref mki.fistjump_multiple_max, MarkerEaseMul);
        }

        private static void ShiftRange(ref float min, ref float max, float scale)
        {
            float width = max - min;
            min *= scale;
            max = min + width;
        }
    }
}
