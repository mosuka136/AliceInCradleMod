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

        /// <summary>
        /// 图标特效：流动模式的色相循环周期（帧数，非毫秒；X.ANMPT 以帧计时）。
        /// 快捷物品栏逐帧重绘，使用随时间循环的色相。
        /// </summary>
        internal const int IconEffectPeriodFrames = 144;

        /// <summary>图标特效：叠加层的最大不透明度（乘以图标自身透明度）。</summary>
        internal const float IconEffectOverlayAlpha = 0.35f;

        /// <summary>
        /// 由物品键稳定推导的色相偏移（0–1），
        /// 用作流动色相的相位偏移，让不同物品在色相环上错开、不同步闪。
        /// </summary>
        internal static float HueFromKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0f;

            uint hash = 2166136261U;
            foreach (char c in key)
                hash = (hash ^ c) * 16777619U;
            return (float)(hash % 1000U) / 1000f;
        }

        /// <summary>
        /// 色相（0–1 环形，自动回绕）转 RGB，饱和度与明度固定为 1：
        /// 0 红 → 1/6 黄 → 2/6 绿 → 3/6 青 → 4/6 蓝 → 5/6 品红 → 1 红。
        /// </summary>
        internal static void HueToRgb(float hue, out float r, out float g, out float b)
        {
            float sector = (hue - (float)global::System.Math.Floor((double)hue)) * 6f;
            if (float.IsNaN(sector))
            {
                r = g = b = 1f;
                return;
            }

            int index = (int)sector;
            float f = sector - index;
            switch (index)
            {
                case 0:
                    r = 1f;
                    g = f;
                    b = 0f;
                    break;
                case 1:
                    r = 1f - f;
                    g = 1f;
                    b = 0f;
                    break;
                case 2:
                    r = 0f;
                    g = 1f;
                    b = f;
                    break;
                case 3:
                    r = 0f;
                    g = 1f - f;
                    b = 1f;
                    break;
                case 4:
                    r = f;
                    g = 0f;
                    b = 1f;
                    break;
                default:
                    r = 1f;
                    g = 0f;
                    b = 1f - f;
                    break;
            }
        }
    }
}
