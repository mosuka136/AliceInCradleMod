using BetterExperience.ConfigFileSpace;
using BetterExperience.TranslatorSpace;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigEntry<bool> EnablePreloadCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyGoldCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyJuiceCount { get; private set; }

        private const string SectionCurrency = "Currency";

        public static void InitializeCurrency()
        {
            Config.CreateTable(SectionCurrency, new Translator(chinese: "货币", english: "Currency"));

            EnablePreloadCurrencyGoldCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyGoldCount),
                false,
                new Translator(chinese: "预加载金币数量", english: "Preload Gold Count"),
                new Translator(
                    chinese: "启用预加载金币数量。开启后，金币数量将在存档读取后自动设置一次，使用设置值覆盖原始的金币数量。",
                    english: "Enable preload gold count. When enabled, the gold count will be automatically set once after loading a save, " +
                    "using the configured value to override the original gold count."
                )
                );
            EnablePreloadCurrencyCraftsCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyCraftsCount),
                false,
                new Translator(chinese: "预加载兑锭数量", english: "Preload Crafts Count"),
                new Translator(
                    chinese: "启用预加载兑锭数量。开启后，兑锭数量将在存档读取后自动设置一次，使用设置值覆盖原始的兑锭数量。",
                    english: "Enable preload crafts count. When enabled, the crafts count will be automatically set once after loading a save, " +
                    "using the configured value to override the original crafts count."
                )
                );
            EnablePreloadCurrencyJuiceCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyJuiceCount),
                false,
                new Translator(chinese: "预加载精萃数量", english: "Preload Juice Count"),
                new Translator(
                    chinese: "启用预加载精萃数量。开启后，精萃数量将在存档读取后自动设置一次，使用设置值覆盖原始的精萃数量。",
                    english: "Enable preload juice count. When enabled, the juice count will be automatically set once after loading a save, " +
                    "using the configured value to override the original juice count."
                )
                );
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
            SetCurrencyGoldCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyGoldCount),
                -1L,
                new Translator(chinese: "设置金币数量", english: "Set Gold Count"),
                new Translator(
                    chinese: "设置金币数量。设为 -1 可保持为当前数量。",
                    english: "Set gold count. Set to -1 to keep the current count."
                )
                );
            SetCurrencyCraftsCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyCraftsCount),
                -1L,
                new Translator(chinese: "设置兑锭数量", english: "Set Crafts Count"),
                new Translator(
                    chinese: "设置兑锭数量。设为 -1 可保持为当前数量。",
                    english: "Set crafts count. Set to -1 to keep the current count."
                )
                );
            SetCurrencyJuiceCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyJuiceCount),
                -1L,
                new Translator(chinese: "设置精萃数量", english: "Set Juice Count"),
                new Translator(
                    chinese: "设置精萃数量。设为 -1 可保持为当前数量。",
                    english: "Set juice count. Set to -1 to keep the current count."
                )
                );
        }
    }
}
