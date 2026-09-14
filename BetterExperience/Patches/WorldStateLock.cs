namespace BetterExperience.Patches
{
    /// <summary>
    /// 第 1 轮世界状态规则：夜间/雷暴快传、天气锁定、危险度锁定。
    /// 只描述“是否拦截原版自动变更”，不读写游戏对象；控制页手动写入仍由各 Set* 补丁负责。
    /// </summary>
    internal static class WorldStateLock
    {
        internal const string NightThunderAlert = "Alert_cannot_fast_travel";
        internal const string ScenarioLockAlert = "Alert_bench_execute_scenario_locked";

        internal static bool AllowsNightThunderTravel(bool fastTravelAnywhere, bool allowNightTravel)
        {
            return fastTravelAnywhere || allowNightTravel;
        }

        /// <summary>
        /// 原版夜间/雷暴限制：夜间且战斗计数大于 0，或当前是雷暴。
        /// 不含自由快传解锁、调试监听器和本模组开关。
        /// </summary>
        internal static bool VanillaNightOrThunderBlocksTravel(bool isNight, int battleCount, bool hasThunder)
        {
            return (isNight && battleCount > 0) || hasThunder;
        }

        /// <summary>
        /// 仅在本模组放行了原版会拦住的夜间/雷暴快传时，阻止把 <c>free_travel_analyzed</c> 写成 true。
        /// 当前地图白天已经解锁过自由快传时，仍按原版给目标地图写解锁。
        /// </summary>
        internal static bool ShouldPersistFreeTravelAnalyzed(
            bool settingTrue,
            bool assistOn,
            bool vanillaNightThunderBlocks,
            bool currentMapAlreadyUnlocked)
        {
            if (!settingTrue)
                return true;
            if (!assistOn || !vanillaNightThunderBlocks)
                return true;
            return currentMapAlreadyUnlocked;
        }

        internal static bool ComputeMenuFastTravelAllowed(
            bool onBench,
            bool anywhere,
            bool commandEnabled,
            bool mapCanHandle)
        {
            return commandEnabled && mapCanHandle && (onBench || anywhere);
        }

        /// <summary>
        /// 夜间椅子传送开关的实时规则。
        /// 开启时只清掉夜间/雷暴提示，剧情锁保留。
        /// 关闭时只要原版夜间/雷暴条件成立，就重新拦下，避免自由快传标记把限制留下。
        /// </summary>
        internal static string ApplyNightTravelAssist(
            string original,
            bool assistOn,
            bool vanillaNightThunderBlocks)
        {
            if (assistOn)
                return original == NightThunderAlert ? null : original;

            if (vanillaNightThunderBlocks)
                return original == ScenarioLockAlert ? original : NightThunderAlert;

            return original;
        }

        /// <summary>兼容旧测试名，等同于未考虑夜间条件时只清夜间提示。</summary>
        internal static string OverrideCantFastTravel(string original, bool allowNightThunder)
        {
            return ApplyNightTravelAssist(original, allowNightThunder, vanillaNightThunderBlocks: false);
        }

        internal static bool ShouldSkipWeatherShuffle(bool lockWeather)
        {
            return lockWeather;
        }

        internal static bool ShouldSkipDangerEventWrite(bool lockDanger)
        {
            return lockDanger;
        }

        /// <summary>
        /// 原版只在 <c>consider_debug_lock_danger</c> 时让锁定把战斗结算增量变成 0；
        /// 掉落倍率等其它调用保持原值。
        /// </summary>
        internal static int OverrideReservedObtainableGrade(int original, bool considerLock, bool lockDanger)
        {
            return considerLock && lockDanger ? 0 : original;
        }
    }
}
