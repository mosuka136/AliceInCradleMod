namespace BetterExperience.Patches
{
    /// <summary>
    /// 商店刷新按钮的注入判定逻辑（不访问游戏对象，便于单元测试）。
    /// </summary>
    internal static class StoreRefreshButtonLogic
    {
        // 原版命令列表固定为 4 键且以结算结尾；布局一致时才追加刷新键，版本变动时保持原样。
        internal const string CheckoutTitle = "&&cmd_checkout";

        /// <summary>
        /// 匹配原版命令列表的 titles 数组并追加刷新键；布局不符时原样返回。
        /// </summary>
        internal static string[] ExpandTitles(string[] titles, string refreshKey)
        {
            if (titles == null || titles.Length != 4 || titles[3] != CheckoutTitle)
                return titles;

            var expanded = new string[titles.Length + 1];
            titles.CopyTo(expanded, 0);
            expanded[titles.Length] = refreshKey;
            return expanded;
        }
    }
}
