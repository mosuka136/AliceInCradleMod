namespace BetterExperience.Patches
{
    /// <summary>
    /// 公会任务板一键刷新的判定规则，不访问游戏对象。
    /// </summary>
    internal static class GuildQuestBoardRefreshLogic
    {
        /// <summary>
        /// 公会柜台界面是否显示刷新按钮：交付报告界面（digesting）不显示。
        /// </summary>
        internal static bool ShowInGuildUi(bool digesting)
        {
            return !digesting;
        }

        /// <summary>
        /// 可移除的任务条目：未受领（无任务跟踪器）且尚未被销毁；
        /// 已受领任务不受刷新影响。
        /// </summary>
        internal static bool IsRemovable(bool hasQuestTracker, bool destructed)
        {
            return !hasQuestTracker && !destructed;
        }
    }
}
