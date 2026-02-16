using BepInEx.Configuration;

namespace BetterExperience
{
    internal class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<bool> EnableHarmonyLog { get; private set; }
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<bool> EnableBiggerBackpack { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }
        public static ConfigEntry<bool> EnableNoHpDamage { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCount { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }

        public static ConfigEntry<string> LogReadme { get; private set; }
        public static ConfigEntry<HLog.LogLevel> HarmonyLogLevel { get; private set; }
        public static ConfigEntry<HLog.LogLevel> BepInExLogLevel { get; private set; }

        private static ConfigEntry<string> HotkeyReadme { get; set; }
        public static ConfigEntry<string> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<string> FlushAllStoreHotkey { get; private set; }

        public static ConfigEntry<int> BackpackCapacity { get; private set; }

        public static ConfigEntry<bool> EnableLockCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<long> LockCurrencyGoldCount { get; private set; }
        public static ConfigEntry<long> LockCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<long> LockCurrencyJuiceCount { get; private set; }

        public static ConfigEntry<bool> EnableSensitivities { get; private set; }
        public static ConfigEntry<bool> EnableTextureImmediateReload { get; private set; }

        private static string SectionGeneral = "General";
        private static string SectionLog = "Log";
        private static string SectionHotkey = "Hotkey";
        private static string SectionBackpack = "Backpack";
        private static string SectionCurrency = "Currency";
        private static string SectionTexture = "Texture";

        public static void Initialization()
        {
            BetterExperience.Instance.Config.SaveOnConfigSet = false;

            EnableBetterExperience = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBetterExperience),
                true,
                "Enable Better Experience mod."
                );
            EnableHarmonyLog = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableHarmonyLog),
                true,
                "Enable Harmony log. It will generate a log file in BetterExperience/logs folder."
                );
            EnableMosaic = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableMosaic),
                false,
                "Enable mosaic effect."
                );
            EnableBetterReelEffect = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBetterReelEffect),
                true,
                "Enable better reel effect."
                );
            EnableBiggerBackpack = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBiggerBackpack),
                true,
                "Enable bigger backpack."
                );
            EnableFlushAllStore = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableFlushAllStore),
                true,
                "Enable flush all store function."
                );
            EnableRemoveLimitInTreasureChests = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableRemoveLimitInTreasureChests),
                true,
                "Enable remove the limit on the number of items in treasure chests."
                );
            EnableNoHpDamage = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableNoHpDamage),
                false,
                "Enable no HP damage."
                );
            EnableLockCurrencyCount = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableLockCurrencyCount),
                false,
                "Enable lock currency count. It will prevent the currency count from increasing or decreasing when you get gold, crafts or juice."
                );
            EnableReplaceTexture = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableReplaceTexture),
                true,
                "Enable replace texture. It will use the texture from the BetterExperience\\ReplaceTexture folder to replace the original texture."
                );
            EnableBetterSaveSite = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBetterSaveSite),
                true,
                "Enable better save site. It will allow saving anywhere."
                );

            LogReadme = BetterExperience.Instance.Config.Bind(
                SectionLog,
                "_README",
                "",
                "Harmony log will be generated in BetterExperience/logs folder.\n" +
                "The log level of Harmony log and BepInEx log can be set separately.\n" +
                "If the log level is set to Info, it will log all messages.\n" +
                "If the log level is set to Warning, it will only log warning and error messages.\n" +
                "If the log level is set to Error, it will only log error messages."
                );
            HarmonyLogLevel = BetterExperience.Instance.Config.Bind(
                SectionLog,
                nameof(HarmonyLogLevel),
                HLog.LogLevel.Info,
                "The log level of Harmony log. Default is Info."
                );
            BepInExLogLevel = BetterExperience.Instance.Config.Bind(
                SectionLog,
                nameof(BepInExLogLevel),
                HLog.LogLevel.Warning,
                "The log level of BepInEx log. Default is Warning."
                );

            HotkeyReadme = BetterExperience.Instance.Config.Bind(
            SectionHotkey,
            "_README",
            "",
            new ConfigDescription(
                "Hotkey notation guide:\n" +
                "1) Single key:F / F1 / Space / Tab\n" +
                "2) Combination:Ctrl+Shift+F\n" +
                "3) Gamepad:GamepadStart+GamepadSouth\n" +
                "4) Alternatives:Ctrl+F, GamepadStart+GamepadSouth(comma-separated)\n" +
                "\n" +
                "Modifier keys:\n" +
                "- Ctrl / Shift / Alt\n" +
                "- Or:LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                "\n" +
                "Gamepad button names:\n" +
                "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                "\n" +
                "Examples:\n" +
                "- Ctrl+F\n" +
                "- LeftCtrl+RightShift+F\n" +
                "- GamepadStart+GamepadSouth\n" +
                "- Ctrl+Shift+F, GamepadStart+GamepadSouth\n" +
                "\n"
            ));
            ReloadConfigHotkey = BetterExperience.Instance.Config.Bind(
                SectionHotkey,
                nameof(ReloadConfigHotkey),
                "Ctrl+R",
                "The hotkey to reload config. Default is Ctrl+R."
                );
            FlushAllStoreHotkey = BetterExperience.Instance.Config.Bind(
                SectionHotkey,
                nameof(FlushAllStoreHotkey),
                "F",
                "The hotkey to flush all store. Default is F."
                );

            BackpackCapacity = BetterExperience.Instance.Config.Bind(
                SectionBackpack,
                nameof(BackpackCapacity),
                500,
                "The backpack capacity. Default is 500."
                );

            EnableLockCurrencyGoldCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyGoldCount),
                false,
                "Enable lock gold count. It will prevent the gold count from increasing or decreasing when you get gold."
                );
            EnableLockCurrencyCraftsCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyCraftsCount),
                false,
                "Enable lock crafts count. It will prevent the crafts count from increasing or decreasing when you get crafts."
                );
            EnableLockCurrencyJuiceCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyJuiceCount),
                false,
                "Enable lock juice count. It will prevent the juice count from increasing or decreasing when you get juice."
                );
            LockCurrencyGoldCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyGoldCount),
                -1L,
                "The gold count to lock. It will be used when lock gold count is enabled. Set to -1 to maintain the original count"
                );
            LockCurrencyCraftsCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyCraftsCount),
                -1L,
                "The crafts count to lock. It will be used when lock crafts count is enabled. Set to -1 to maintain the original count"
                );
            LockCurrencyJuiceCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyJuiceCount),
                -1L,
                "The juice count to lock. It will be used when lock juice count is enabled. Set to -1 to maintain the original count"
                );

            EnableSensitivities = BetterExperience.Instance.Config.Bind(
                SectionTexture,
                nameof(EnableSensitivities),
                true,
                "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures."
                );
            EnableTextureImmediateReload = BetterExperience.Instance.Config.Bind(
                SectionTexture,
                nameof(EnableTextureImmediateReload),
                false,
                "Enable immediate reload. If enabled, the texture will be reloaded immediately after the texture is changed. "
                );

            BetterExperience.Instance.Config.SaveOnConfigSet = true;
            BetterExperience.Instance.Config.Save();
        }

        public static void ReloadConfig()
        {
            BetterExperience.Instance.Config.Reload();
        }
    }
}
