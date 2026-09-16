namespace BetterExperience.Patches
{
    /// <summary>
    /// 卷轴可获取数量倍率的换算规则，不访问游戏对象。
    /// 原版数量约等于危险度 / 5（每日累计），界面侧以 byte 存储，
    /// 因此倍增结果按 0–255 收敛，避免溢出回绕。
    /// </summary>
    internal static class SetReelObtainableRatioLogic
    {
        internal const int CountMax = 255;

        /// <summary>
        /// 倍率小于 0 表示未启用，保持原值；否则四舍五入后收敛到 0–255。
        /// </summary>
        internal static int ApplyRatio(int count, float ratio)
        {
            if (ratio < 0f)
                return count;

            float scaled = count * ratio + 0.5f;
            if (scaled <= 0f)
                return 0;

            if (scaled >= CountMax)
                return CountMax;

            return (int)scaled;
        }
    }
}
