using BetterExperience.BLogSpace;
using BetterExperience.Patches;
using nel;
using System;
using UnityModBase.HControlSpace;
using UnityModBase.HGuiSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BControlManager
{
    internal static partial class ControlManager
    {
        internal static ControlEntry<long> SetCurrencyGoldCount { get; private set; }
        internal static ControlEntry<long> SetCurrencyCraftsCount { get; private set; }
        internal static ControlEntry<long> SetCurrencyJuiceCount { get; private set; }
        internal static ControlEntry<long> SetCurrencyBarScoreCount { get; private set; }
        internal static ControlEntry<int> SetGuildPoint { get; private set; }
        internal static ControlEntry<int> SetFoodTicketCount { get; private set; }

        private const string SectionCurrency = "Currency";

        /// <summary>
        /// 创建货币控制表并绑定货币数量条目。
        /// </summary>
        internal static void InitializeCurrency()
        {
            try
            {
                BService.Control.CreateTable(
                    SectionCurrency,
                    new Translator(chinese: "货币", english: "Currency"),
                    new Translator(
                        chinese: "查看和修改当前游戏中的货币数量。",
                        english: "View and modify currency amounts in the current game."
                        )
                    );

                SetCurrencyGoldCount = BindCurrency(
                    nameof(SetCurrencyGoldCount),
                    HPatches.SetCurrencyCountPatch.GetCurrencyGoldCount,
                    HPatches.SetCurrencyCountPatch.SetCurrencyGoldCount,
                    new Translator(chinese: "设置金币数量", english: "Set Gold Count")
                    );
                SetCurrencyCraftsCount = BindCurrency(
                    nameof(SetCurrencyCraftsCount),
                    HPatches.SetCurrencyCountPatch.GetCurrencyCraftsCount,
                    HPatches.SetCurrencyCountPatch.SetCurrencyCraftsCount,
                    new Translator(chinese: "设置兑锭数量", english: "Set Crafts Count")
                    );
                SetCurrencyJuiceCount = BindCurrency(
                    nameof(SetCurrencyJuiceCount),
                    HPatches.SetCurrencyCountPatch.GetCurrencyJuiceCount,
                    HPatches.SetCurrencyCountPatch.SetCurrencyJuiceCount,
                    new Translator(chinese: "设置精萃数量", english: "Set Juice Count")
                    );
                SetCurrencyBarScoreCount = BindCurrency(
                    nameof(SetCurrencyBarScoreCount),
                    HPatches.SetCurrencyCountPatch.GetCurrencyBarScoreCount,
                    HPatches.SetCurrencyCountPatch.SetCurrencyBarScoreCount,
                    new Translator(chinese: "设置酒吧积分", english: "Set Bar Score")
                    );
                SetGuildPoint = Bind(
                    SectionCurrency,
                    nameof(SetGuildPoint),
                    HPatches.SetGuildPointPatch.GetGuildPoint,
                    HPatches.SetGuildPointPatch.SetGuildPoint,
                    new Translator(chinese: "设置工会积分", english: "Set Guild Points"),
                    new Translator(
                        chinese: "设置工会积分，同时影响工会等级。",
                        english: "Set guild points, which also affect guild rank."
                        ),
                    new UiSliderMetadata(0f, GuildManager.GQ_POINT_MAX, 1f)
                    );
                SetFoodTicketCount = Bind(
                    SectionCurrency,
                    nameof(SetFoodTicketCount),
                    HPatches.SetFoodTicketCountPatch.GetFoodTicketCount,
                    HPatches.SetFoodTicketCountPatch.SetFoodTicketCount,
                    new Translator(chinese: "设置餐券数量", english: "Set Food Ticket Count"),
                    new Translator(
                        chinese: "设置当前绑定酒店的食事餐券数量。新增餐券沿用现有最高折扣（无餐券时 50%）；尚未绑定酒店时请先购买一次含餐券的套餐。",
                        english: "Set the food ticket count of the bound hotel. New tickets use the best existing discount (50% when none); buy a ticket-included plan first if none are bound."
                        ),
                    new UiSliderMetadata(0f, 99f, 1f)
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize control manager for currency.", ex);
            }
        }

        /// <summary>
        /// 绑定货币数量，统一使用下限 0（Set* 方法拒绝负数）、上限 1000000、步进 1 的滑杆。
        /// </summary>
        private static ControlEntry<long> BindCurrency(
            string key,
            Func<long> valueGetter,
            Action<long> valueSetter,
            Translator name)
        {
            return Bind(
                SectionCurrency,
                key,
                valueGetter,
                valueSetter,
                name,
                new Translator(
                    chinese: "设置当前游戏中的货币数量。",
                    english: "Set the currency amount in the current game."
                    ),
                new UiSliderMetadata(0f, 1000000f, 1f)
                );
        }
    }
}
