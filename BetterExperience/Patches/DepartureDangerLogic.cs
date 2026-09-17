using System;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 出门危险度滑条规则。不触碰 Unity 或 UI 实例。
    /// 原版按已完成的 16 点周期解锁正午/傍晚/夜间三档，最多 3 天（夜间 41）。
    /// </summary>
    internal static class DepartureDangerLogic
    {
        internal const int LevelOneDay = 16;
        internal const int SlotsPerDay = 3;
        internal const int EveningOffset = 5;
        internal const int NightOffset = 9;
        internal const int VanillaMaxDays = 3;
        internal const int MeterCap = 160;

        internal static int NightLevelFromSlider(int val)
        {
            int day = Math.Max(0, val) / SlotsPerDay;
            int offset = 0;
            switch (Math.Max(0, val) % SlotsPerDay)
            {
                case 1:
                    offset = EveningOffset;
                    break;
                case 2:
                    offset = NightOffset;
                    break;
            }
            return day * LevelOneDay + offset;
        }

        /// <summary>
        /// 不高于已达危险度的最高正午/傍晚/夜间档。计量显示上限 160。
        /// </summary>
        internal static int MaxSliderValue(int reachedNightLevel)
        {
            int reached = Math.Max(0, Math.Min(reachedNightLevel, MeterCap));
            int day = reached / LevelOneDay;
            int rem = reached % LevelOneDay;
            int slot = rem >= NightOffset ? 2 : rem >= EveningOffset ? 1 : 0;
            return day * SlotsPerDay + slot;
        }

        /// <summary>
        /// 复原版：每完成 16 点解锁 3 档，最多 3 天。未达 16 时滑条不会出现，返回 0。
        /// </summary>
        internal static int VanillaMaxSliderValue(int reachedNightLevel)
        {
            int slots = 0;
            for (int index = 0; index < VanillaMaxDays && reachedNightLevel >= LevelOneDay * (index + 1); index++)
                slots += SlotsPerDay;
            return Math.Max(0, slots - 1);
        }

        internal static int ResolveSliderMax(bool unlockReached, int reachedNightLevel, int vanillaMax)
        {
            if (!unlockReached)
                return vanillaMax;
            return Math.Max(vanillaMax, MaxSliderValue(reachedNightLevel));
        }
    }
}
