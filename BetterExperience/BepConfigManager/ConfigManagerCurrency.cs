using BetterExperience.ConfigFileSpace;

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
            Config.CreateTable(SectionCurrency);

            EnablePreloadCurrencyGoldCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyGoldCount),
                false,
                "Enable preload gold count. When enabled, the gold count will be automatically set once after loading a save, " +
                "using the configured value to override the original gold count.\n" +
                "启用预加载金币数量。开启后，金币数量将在存档读取后自动设置一次，使用设置值覆盖原始的金币数量。"
                );
            EnablePreloadCurrencyCraftsCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyCraftsCount),
                false,
                "Enable preload crafts count. When enabled, the crafts count will be automatically set once after loading a save, " +
                "using the configured value to override the original crafts count.\n" +
                "启用预加载兑锭数量。开启后，兑锭数量将在存档读取后自动设置一次，使用设置值覆盖原始的兑锭数量。"
                );
            EnablePreloadCurrencyJuiceCount = Config.Bind(
                SectionCurrency,
                nameof(EnablePreloadCurrencyJuiceCount),
                false,
                "Enable preload juice count. When enabled, the juice count will be automatically set once after loading a save, " +
                "using the configured value to override the original juice count.\n" +
                "启用预加载精萃数量。开启后，精萃数量将在存档读取后自动设置一次，使用设置值覆盖原始的精萃数量。"
                );
            EnableLockCurrencyGoldCount = Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyGoldCount),
                false,
                "Enable lock gold count. When enabled, the number of gold will not increase or decrease.\n" +
                "启用金币数量锁定。开启后金币数量不会增加或减少。"
                );
            EnableLockCurrencyCraftsCount = Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyCraftsCount),
                false,
                "Enable lock crafts count. When enabled, the number of crafts will not increase or decrease.\n" +
                "启用兑锭数量锁定。开启后兑锭数量不会增加或减少。"
                );
            EnableLockCurrencyJuiceCount = Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyJuiceCount),
                false,
                "Enable lock juice count. When enabled, the number of juice will not increase or decrease.\n" +
                "启用精萃数量锁定。开启后精萃数量不会增加或减少。"
                );
            SetCurrencyGoldCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyGoldCount),
                -1L,
                "Set gold count. Set to -1 to keep the current count.\n设置金币数量。设为 -1 可保持为当前数量。"
                );
            SetCurrencyCraftsCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyCraftsCount),
                -1L,
                "Set crafts count. Set to -1 to keep the current count.\n设置兑锭数量。设为 -1 可保持为当前数量。"
                );
            SetCurrencyJuiceCount = Config.Bind(
                SectionCurrency,
                nameof(SetCurrencyJuiceCount),
                -1L,
                "Set juice count. Set to -1 to keep the current count.\n设置精萃数量。设为 -1 可保持为当前数量。"
                );
        }
    }
}
