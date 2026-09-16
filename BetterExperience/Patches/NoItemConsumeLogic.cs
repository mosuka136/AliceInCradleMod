namespace BetterExperience.Patches
{
    /// <summary>
    /// 物品超量使用不消耗的判定规则，不访问游戏对象。
    /// 以背包格（组）为单位：一个格子是某物品某品级的堆叠，装满为一组；
    /// 品级行保留区分，满格数合计判定。该物品装满的格子数达到阈值时，
    /// 本次使用不扣减；满格数降到阈值以下后恢复正常消耗。不凭空补充物品。
    /// </summary>
    internal static class NoItemConsumeLogic
    {
        internal const int ThresholdMin = 0;
        internal const int ThresholdMax = 20;
        internal const int ThresholdDefault = 1;
        internal const int GradeCount = 5;

        /// <summary>
        /// 阈值约束到 0–20 组。
        /// </summary>
        internal static int ClampThreshold(int threshold)
        {
            if (threshold < ThresholdMin)
                return ThresholdMin;

            if (threshold > ThresholdMax)
                return ThresholdMax;

            return threshold;
        }

        /// <summary>
        /// 统计各品级行中装满（数量达到每格容量）的格子数，跨品级合计。
        /// </summary>
        internal static int CountFullRows(int[] countsPerGrade, int stockable)
        {
            if (stockable <= 0 || countsPerGrade == null)
                return 0;

            int fullRows = 0;
            for (int i = 0; i < countsPerGrade.Length; i++)
            {
                if (countsPerGrade[i] >= stockable)
                    fullRows++;
            }

            return fullRows;
        }

        /// <summary>
        /// 满格数达到阈值时本次使用不消耗；阈值 0 表示只要有库存就不消耗；
        /// 阈值为负表示功能未启用，恒为消耗。
        /// </summary>
        internal static bool ShouldKeep(int fullRows, int totalCount, int threshold)
        {
            if (threshold < 0)
                return false;

            if (threshold == 0)
                return totalCount > 0;

            return fullRows >= threshold;
        }
    }
}
