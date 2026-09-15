using System;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 商店价格倍率的取值范围与换算规则，不访问游戏对象。
    /// 原版 buyPrice/sellPrice 返回的是截断为整数的单价，
    /// 这里在整数结果上继续乘以倍率并保持向下截断，与原版取整方向一致。
    /// </summary>
    internal static class StorePriceLogic
    {
        internal const float RatioMin = 0f;
        internal const float BuyRatioMax = 2f;
        internal const float SellRatioMax = 5f;
        internal const float RatioDefault = 1f;

        /// <summary>
        /// 把倍率约束到指定上限的合法区间；非法值（NaN 或无穷）回退为 1。
        /// </summary>
        internal static float ClampRatio(float ratio, float max)
        {
            if (float.IsNaN(ratio) || float.IsInfinity(ratio))
                return RatioDefault;

            if (ratio < RatioMin)
                return RatioMin;

            if (ratio > max)
                return max;

            return ratio;
        }

        /// <summary>
        /// 对原版整数价格应用倍率，结果向下截断且不低于 0；倍率 0 表示免费。
        /// 超出 int 范围时封顶为 <see cref="int.MaxValue"/>，避免 double 显式截断的未定义结果。
        /// </summary>
        internal static int ApplyRatio(int price, float ratio)
        {
            if (price <= 0)
                return 0;

            double scaled = (double)price * ratio;
            if (scaled >= int.MaxValue)
                return int.MaxValue;

            return Math.Max(0, (int)scaled);
        }
    }
}
