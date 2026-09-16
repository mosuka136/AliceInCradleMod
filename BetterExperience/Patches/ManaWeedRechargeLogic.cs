namespace BetterExperience.Patches
{
    /// <summary>
    /// 魔力草恢复时间上限的换算规则，不访问游戏对象。
    /// 原版恢复计时以帧为单位（60fps 下 2400–3000 帧约 40–50 秒），
    /// 农场小游戏 1320 帧、战后安全状态 6000 帧。
    /// </summary>
    internal static class ManaWeedRechargeLogic
    {
        internal const float SecondsMin = 0f;
        internal const float SecondsMax = 90f;
        internal const float SecondsDisabled = -1f;
        internal const float FramesPerSecond = 60f;

        /// <summary>
        /// 秒数换算为帧上限；任何负数都表示未启用，返回 -1。
        /// </summary>
        internal static float ResolveRechargeFrames(float seconds)
        {
            if (seconds < 0f)
                return -1f;

            return seconds * FramesPerSecond;
        }

        /// <summary>
        /// 用帧上限封顶当前恢复计时；上限为负表示不修改。
        /// </summary>
        internal static float CapRechargeTime(float currentFrames, float capFrames)
        {
            if (capFrames < 0f)
                return currentFrames;

            if (currentFrames < capFrames)
                return currentFrames;

            return capFrames;
        }
    }
}
