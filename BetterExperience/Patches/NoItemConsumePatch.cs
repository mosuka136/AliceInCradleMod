using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 物品超量使用不消耗。
        /// 覆盖两条“使用物品”路径：快捷物品选择器（UseItemSelector.useItemOnCurrentCursor）
        /// 与背包菜单的使用命令（UiItemManageBox.fnClickItemCmd 的 "use" 分支）。
        /// 两处都在使用成功后对主背包调用 Reduce(Itm, 1, grade)，这里借助使用期间的
        /// 上下文标志，仅在该调用上按阈值放行——物品总数超过阈值时跳过扣减，
        /// 效果照常生效；丢弃、出售等其它数量变化不受影响。
        /// </summary>
        [HarmonyPatch]
        public static class NoItemConsumePatch
        {
            // 使用物品的上下文标志：入口前缀置位、出口后缀复位，主线程内成对出现。
            private static bool _usingItem;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(UseItemSelector), nameof(UseItemSelector.useItemOnCurrentCursor))]
            public static void QuickUsePrefix()
            {
                _usingItem = IsEnabled();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UseItemSelector), nameof(UseItemSelector.useItemOnCurrentCursor))]
            public static void QuickUsePostfix()
            {
                _usingItem = false;
            }

            // 菜单命令同时处理 use/drop/discard，仅使用命令期间拦截扣减。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiItemManageBox), nameof(UiItemManageBox.fnClickItemCmd))]
            public static void MenuCmdPrefix(aBtn B)
            {
                try
                {
                    _usingItem = IsEnabled() && B != null && B.title == "use";
                }
                catch (Exception ex)
                {
                    _usingItem = false;
                    BLog.Error($"Unexpected error in {nameof(MenuCmdPrefix)}.", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiItemManageBox), nameof(UiItemManageBox.fnClickItemCmd))]
            public static void MenuCmdPostfix()
            {
                _usingItem = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.Reduce),
                new[] { typeof(NelItem), typeof(int), typeof(int), typeof(bool) })]
            public static bool ReducePrefix(ItemStorage __instance, NelItem Itm, int count, int grade)
            {
                try
                {
                    if (!_usingItem || count != 1 || Itm == null || __instance != GetInventory())
                        return true;

                    int threshold = ConfigManager.SetNoItemConsumeCount?.Value
                        ?? NoItemConsumeLogic.ThresholdDefault;
                    // 以背包格（组）为单位：逐品级统计装满的格子数后合计判定。
                    int totalCount = __instance.getCount(Itm);
                    int fullRows = 0;
                    if (threshold > 0)
                    {
                        int stockable = __instance.getItemStockable(Itm);
                        if (stockable > 0)
                        {
                            var countsPerGrade = new int[NoItemConsumeLogic.GradeCount];
                            for (int g = 0; g < countsPerGrade.Length; g++)
                                countsPerGrade[g] = __instance.getCount(Itm, g);
                            fullRows = NoItemConsumeLogic.CountFullRows(countsPerGrade, stockable);
                        }
                    }

                    if (!NoItemConsumeLogic.ShouldKeep(fullRows, totalCount, threshold))
                        return true;

                    BLog.Debug($"{nameof(NoItemConsumePatch)} applied for {Itm.key} (grade {grade}, full rows {fullRows}).");
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ReducePrefix)}.", ex);
                    return true;
                }
            }

            private static bool IsEnabled()
            {
                return ConfigManager.EnableNoItemConsume?.Value == true;
            }
        }
    }
}
