using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public static partial class ConfigManager
    {
        // 货币可经双值“读档后预加载”项（Value1 是否应用，Value2 数量，-1 表示不修改）在读档时写入，
        // 也可在游戏中经实时控制界面设置或按类型锁定。
        public static ConfigEntry<bool> EnableLockCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyBarScoreCount { get; private set; }
        public static ConfigEntry<bool> EnableLockGuildPoint { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000000f, 1f)]
        public static ConfigEntry<bool, long> SetCurrencyGoldCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000000f, 1f)]
        public static ConfigEntry<bool, long> SetCurrencyCraftsCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000000f, 1f)]
        public static ConfigEntry<bool, long> SetCurrencyJuiceCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, 1000000f, 1f)]
        public static ConfigEntry<bool, long> SetCurrencyBarScoreCount { get; private set; }
        [EntryGui(2)]
        [EntrySlider(1, -1f, nel.GuildManager.GQ_POINT_MAX, 1f)]
        public static ConfigEntry<bool, int> SetGuildPoint { get; private set; }
        // 商店价格倍率是持续生效的结算规则，不属于存档态覆盖，使用普通单值配置。
        public static ConfigEntry<bool> EnableStorePriceRatio { get; private set; }
        [EntrySlider(0f, 2f, 0.05f)]
        public static ConfigEntry<float> SetBuyPriceRatio { get; private set; }
        [EntrySlider(0f, 5f, 0.05f)]
        public static ConfigEntry<float> SetSellPriceRatio { get; private set; }
        public static ConfigEntry<bool> EnableInfiniteFoodTicket { get; private set; }

        private const string SectionCurrency = "Currency";

        /// <summary>
        /// 初始化货币数量预加载和锁定配置。
        /// </summary>
        public static void InitializeCurrency()
        {
            try
            {
                Config.CreateTable(SectionCurrency, new Translator(chinese: "货币", english: "Currency"));

                EnableLockCurrencyGoldCount = Config.Bind(
                    SectionCurrency,
                    nameof(EnableLockCurrencyGoldCount),
                    false,
                    new Translator(chinese: "启用金币锁定", english: "Enable Lock Gold Count"),
                    new Translator(
                        chinese: "启用金币数量锁定。开启后金币数量不会增加或减少。",
                        english: "Enable lock gold count. When enabled, the number of gold will not increase or decrease."
                        )
                    );
                EnableLockCurrencyCraftsCount = Config.Bind(
                    SectionCurrency,
                    nameof(EnableLockCurrencyCraftsCount),
                    false,
                    new Translator(chinese: "启用兑锭锁定", english: "Enable Lock Crafts Count"),
                    new Translator(
                        chinese: "启用兑锭数量锁定。开启后兑锭数量不会增加或减少。",
                        english: "Enable lock crafts count. When enabled, the number of crafts will not increase or decrease."
                        )
                    );
                EnableLockCurrencyJuiceCount = Config.Bind(
                    SectionCurrency,
                    nameof(EnableLockCurrencyJuiceCount),
                    false,
                    new Translator(chinese: "启用精萃锁定", english: "Enable Lock Juice Count"),
                    new Translator(
                        chinese: "启用精萃数量锁定。开启后精萃数量不会增加或减少。",
                        english: "Enable lock juice count. When enabled, the number of juice will not increase or decrease."
                        )
                    );
                EnableLockCurrencyBarScoreCount = Config.Bind(
                    SectionCurrency,
                    nameof(EnableLockCurrencyBarScoreCount),
                    false,
                    new Translator(chinese: "启用酒吧积分锁定", english: "Enable Lock Bar Score"),
                    new Translator(
                        chinese: "启用酒吧打工积分锁定。开启后结算入账和商店消费都不会改变积分。",
                        english: "Lock bar work points. Settlement payouts and shop purchases will not change the amount."
                        )
                    );
                EnableLockGuildPoint = Config.Bind(
                    SectionCurrency,
                    nameof(EnableLockGuildPoint),
                    false,
                    new Translator(chinese: "启用工会积分锁定", english: "Enable Guild Points Lock"),
                    new Translator(
                        chinese: "锁定当前工会积分，任务奖励、失败惩罚和购物不再改变积分。",
                        english: "Lock current guild points against quest rewards, failure penalties, and shopping."
                        )
                    );
                EnableStorePriceRatio = Config.Bind(
                    SectionCurrency,
                    nameof(EnableStorePriceRatio),
                    false,
                    new Translator(chinese: "启用商店价格倍率", english: "Enable Store Price Ratio"),
                    new Translator(
                        chinese: "启用商店价格倍率。同时作用于购买价和出售价，界面显示与实际结算一致。",
                        english: "Apply price multipliers to stores. Both buying and selling are affected, display and checkout stay consistent."
                        )
                    );
                EnableInfiniteFoodTicket = Config.Bind(
                    SectionCurrency,
                    nameof(EnableInfiniteFoodTicket),
                    false,
                    new Translator(chinese: "启用酒店餐券不消耗", english: "Enable Infinite Food Ticket"),
                    new Translator(
                        chinese: "在酒店使用餐券用餐后不消耗餐券，折扣照常生效。更换酒店时原版仍会清空餐券。",
                        english: "Hotel meals no longer consume food tickets, the discount still applies. Switching hotels still clears them as in vanilla."
                        )
                    );
                SetCurrencyGoldCount = BindPreloadValue(
                    SectionCurrency,
                    nameof(SetCurrencyGoldCount),
                    -1L,
                    new Translator(chinese: "设置金币数量", english: "Set Gold Count"),
                    new Translator(
                        chinese: "设置金币数量。",
                        english: "Set gold count."
                        )
                    );
                SetCurrencyCraftsCount = BindPreloadValue(
                    SectionCurrency,
                    nameof(SetCurrencyCraftsCount),
                    -1L,
                    new Translator(chinese: "设置兑锭数量", english: "Set Crafts Count"),
                    new Translator(
                        chinese: "设置兑锭数量。",
                        english: "Set crafts count."
                        )
                    );
                SetCurrencyJuiceCount = BindPreloadValue(
                    SectionCurrency,
                    nameof(SetCurrencyJuiceCount),
                    -1L,
                    new Translator(chinese: "设置精萃数量", english: "Set Juice Count"),
                    new Translator(
                        chinese: "设置精萃数量。",
                        english: "Set juice count."
                        )
                    );
                SetCurrencyBarScoreCount = BindPreloadValue(
                    SectionCurrency,
                    nameof(SetCurrencyBarScoreCount),
                    -1L,
                    new Translator(chinese: "设置酒吧积分", english: "Set Bar Score"),
                    new Translator(
                        chinese: "设置酒吧打工积分。写入时会把生涯获得量抬到不低于当前值，以便累计满 20000 后解锁无限外带。",
                        english: "Set bar work points. Lifetime earned is raised to at least this amount so 20000 still unlocks unlimited takeout."
                        )
                    );
                SetGuildPoint = BindPreloadValue(
                    SectionCurrency,
                    nameof(SetGuildPoint),
                    -1,
                    new Translator(chinese: "设置工会积分", english: "Set Guild Points"),
                    new Translator(
                        chinese: "设置工会积分，同时影响工会等级。-1 表示不修改。",
                        english: "Set guild points, which also affect guild rank. -1 leaves them unchanged."
                        )
                    );
                SetBuyPriceRatio = Config.Bind(
                    SectionCurrency,
                    nameof(SetBuyPriceRatio),
                    1f,
                    new Translator(chinese: "设置购买价格倍率", english: "Set Buy Price Ratio"),
                    new Translator(
                        chinese: "商店购买价格倍率。0 为免费，1 为原价，小于 1 为折扣。",
                        english: "Store buy price ratio. 0 is free, 1 is vanilla, below 1 is a discount."
                        )
                    );
                SetSellPriceRatio = Config.Bind(
                    SectionCurrency,
                    nameof(SetSellPriceRatio),
                    1f,
                    new Translator(chinese: "设置出售价格倍率", english: "Set Sell Price Ratio"),
                    new Translator(
                        chinese: "商店出售价格倍率。0 为不获利，1 为原价，大于 1 为加价。",
                        english: "Store sell price ratio. 0 earns nothing, 1 is vanilla, above 1 pays more."
                        )
                    );
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for currency.", ex);
            }
        }
    }
}
