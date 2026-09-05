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
        [EntrySlider(1, -1f, nel.GuildManager.GQ_POINT_MAX, 1f)]
        public static ConfigEntry<bool, int> SetGuildPoint { get; private set; }

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
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to initialize config manager for currency.", ex);
            }
        }
    }
}
