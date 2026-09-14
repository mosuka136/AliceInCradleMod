namespace BetterExperience.Patches
{
    /// <summary>
    /// 饱食读写与消化拦截规则。不直接碰游戏对象。
    /// </summary>
    internal static class SatietyLogic
    {
        /// <summary>
        /// 不消耗开启时跳过食物消化。水分消化、料理预览胃袋仍放行。
        /// </summary>
        internal static bool ShouldSkipProgress(
            bool noDrain,
            bool onlyWater,
            bool isTemporaryStomach)
        {
            return noDrain && !onlyWater && !isTemporaryStomach;
        }
    }
}
