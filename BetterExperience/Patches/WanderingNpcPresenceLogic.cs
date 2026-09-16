namespace BetterExperience.Patches
{
    /// <summary>
    /// 流浪商人在世界地图上的显示规则，不访问游戏对象。
    /// 原版仅夜莺有图标且需要携带铃铛才显示；其余商人用可出现范围圈配色区分。
    /// </summary>
    internal static class WanderingNpcPresenceLogic
    {
        /// <summary>
        /// 范围圈统一半径（世界地图格数），所有商人一致，与各自的可出现范围（catchable）无关；
        /// 圈只作位置标记，不表达实际可出现范围。
        /// </summary>
        internal const float CircleRadiusCells = 0.5f;

        /// <summary>
        /// 范围圈由多少个点组成（原版为 16）。
        /// </summary>
        internal const int CirclePointCount = 4;

        /// <summary>
        /// 地图显示要求功能开启且该商人已通过剧情解锁（isEnable）。
        /// </summary>
        internal static bool ShouldShowOnMap(bool featureEnabled, bool npcEnabled)
        {
            return featureEnabled && npcEnabled;
        }

        /// <summary>
        /// 各商人在地图上的范围圈颜色（RGB），取红/绿/蓝三原色以保证最大区分度，
        /// 与原版白色点圈也能明确区分。
        /// </summary>
        internal static (float R, float G, float B) CircleColor(SummonWanderingNpcKind kind)
        {
            switch (kind)
            {
                case SummonWanderingNpcKind.CoffeeMaker:
                    return (1f, 0.25f, 0.2f);
                case SummonWanderingNpcKind.Tilde:
                    return (0.25f, 0.9f, 0.3f);
                case SummonWanderingNpcKind.Puppet:
                    return (0.3f, 0.5f, 1f);
                default:
                    return (1f, 1f, 1f);
            }
        }
    }
}
