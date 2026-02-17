using BepInEx.Configuration;

namespace BetterExperience
{
    internal class ConfigManager
    {
        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<bool> EnableHarmonyLog { get; private set; }
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableBiggerBackpack { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCount { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInPuppetNpcDefeated { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableInfiniteShield { get; private set; }

        public static ConfigEntry<string> LogReadme { get; private set; }
        public static ConfigEntry<HLog.LogLevel> HarmonyLogLevel { get; private set; }
        public static ConfigEntry<HLog.LogLevel> BepInExLogLevel { get; private set; }

        private static ConfigEntry<string> HotkeyReadme { get; set; }
        public static ConfigEntry<string> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<string> FlushAllStoreHotkey { get; private set; }

        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }

        public static ConfigEntry<bool> EnableBeingAttacked { get; private set; }
        public static ConfigEntry<bool> EnableNoHpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoMpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoEpDamage { get; private set; }

        public static ConfigEntry<bool> EnableWormTrap { get; private set; }
        public static ConfigEntry<bool> EnableMapDamage { get; private set; }
        public static ConfigEntry<bool> EnableDrowning { get; private set; }

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
        private static string SectionReel = "Reel";
        private static string SectionPlayerStatus = "PlayerStatus";
        private static string SectionMapTrap = "MapTrap";
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
                "Enable Better Experience mod.\n启用 Better Experience 模组。"
                );
            EnableHarmonyLog = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableHarmonyLog),
                true,
                "Enable Harmony log. It will generate a log file in BetterExperience\\logs folder.\n启用 Harmony 日志。将在 BetterExperience\\logs 文件夹中生成日志文件。"
                );
            EnableMosaic = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableMosaic),
                false,
                "Enable mosaic effect.\n启用马赛克效果。"
                );
            EnableBiggerBackpack = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBiggerBackpack),
                true,
                "Enable bigger backpack.\n启用更大的背包容量。"
                );
            EnableFlushAllStore = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableFlushAllStore),
                false,
                "Enable flush all store function.\n启用一键刷新商店功能。"
                );
            EnableLockCurrencyCount = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableLockCurrencyCount),
                false,
                "Enable lock currency count. It will prevent the currency count from increasing or decreasing when you get gold, crafts or juice.\n" +
                "启用货币数量锁定。开启后在获得金币、兑锭或精萃时，数量不会增加或减少。"
                );
            EnableReplaceTexture = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableReplaceTexture),
                false,
                "Enable replace texture. It will use the texture from the BetterExperience\\ReplaceTexture folder to replace the original texture.\n" +
                "启用替换贴图。将使用 BetterExperience\\ReplaceTexture 文件夹中的贴图替换原始贴图。"
                );
            EnableBetterSaveSite = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableBetterSaveSite),
                false,
                "Enable better save site. It will allow saving anywhere.\n启用更好的存档点功能。允许在任意位置保存。"
                );
            EnableRemoveLimitInPuppetNpcDefeated = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableRemoveLimitInPuppetNpcDefeated),
                false,
                "Enable the restriction that prevents the Puppet Merchant from spawning before the revenge quest is completed.\n" +
                "启用移除木偶商人在复仇战未完成前无法生成的限制。"
                );
            EnableRemoveLimitInBenchMenu = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableRemoveLimitInBenchMenu),
                false,
                "Enable the restriction that certain options in the chair menu are unavailable for players under specific circumstances\n" +
                "启用移除玩家在某些情况下椅子菜单中的某些选项不可用的限制。"
                );
            EnableInfiniteShield = BetterExperience.Instance.Config.Bind(
                SectionGeneral,
                nameof(EnableInfiniteShield),
                false,
                "Enable infinite shield.\n启用无限护盾。"
                );

            LogReadme = BetterExperience.Instance.Config.Bind(
                SectionLog,
                "_README",
                "",
                "Harmony log will be generated in BetterExperience\\logs folder.\n" +
                "The log level of Harmony log and BepInEx log can be set separately.\n" +
                "If the log level is set to Info, it will log all messages.\n" +
                "If the log level is set to Warning, it will only log warning and error messages.\n" +
                "If the log level is set to Error, it will only log error messages.\n" +
                "Harmony 日志将生成在 BetterExperience\\logs 文件夹中。\n" +
                "Harmony 日志和 BepInEx 日志的等级可分别设置。\n" +
                "若日志等级为 Info，将记录所有消息。\n" +
                "若日志等级为 Warning，仅记录警告和错误消息。\n" +
                "若日志等级为 Error，仅记录错误消息。"
                );
            HarmonyLogLevel = BetterExperience.Instance.Config.Bind(
                SectionLog,
                nameof(HarmonyLogLevel),
                HLog.LogLevel.Info,
                "The log level of Harmony log. Default is Info.\nHarmony 日志等级。默认值为 Info。"
                );
            BepInExLogLevel = BetterExperience.Instance.Config.Bind(
                SectionLog,
                nameof(BepInExLogLevel),
                HLog.LogLevel.Warning,
                "The log level of BepInEx log. Default is Warning.\nBepInEx 日志等级。默认值为 Warning。"
                );

            HotkeyReadme = BetterExperience.Instance.Config.Bind(
            SectionHotkey,
            "_README",
            "",
            new ConfigDescription(
                "Hotkey notation guide:\n" +
                "热键写法说明：\n" +
                "1) Single key:F / F1 / Space / Tab\n" +
                "1) 单键：F / F1 / Space / Tab\n" +
                "2) Combination:Ctrl+Shift+F\n" +
                "2) 组合键：Ctrl+Shift+F\n" +
                "3) Gamepad:GamepadStart+GamepadSouth\n" +
                "3) 手柄：GamepadStart+GamepadSouth\n" +
                "4) Alternatives:Ctrl+F, GamepadStart+GamepadSouth(comma-separated)\n" +
                "4) 备选热键：Ctrl+F, GamepadStart+GamepadSouth（用逗号分隔）\n" +
                "\n" +
                "Modifier keys:\n" +
                "修饰键：\n" +
                "- Ctrl / Shift / Alt\n" +
                "- Ctrl / Shift / Alt\n" +
                "- Or:LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                "- 或：LeftCtrl / RightCtrl / LeftShift / RightShift / LeftAlt / RightAlt\n" +
                "\n" +
                "Gamepad button names:\n" +
                "手柄按键名称：\n" +
                "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                "- GamepadSouth(A/×), GamepadEast(B/○), GamepadWest(X/□), GamepadNorth(Y/△)\n" +
                "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                "- GamepadStart, GamepadSelect, GamepadLeftShoulder(LB/L1), GamepadRightShoulder(RB/R1)\n" +
                "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                "- GamepadDpadUp/Down/Left/Right, GamepadLeftStick, GamepadRightStick\n" +
                "\n" +
                "Examples:\n" +
                "示例：\n" +
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
                "The hotkey to reload config. Default is Ctrl+R.\n重新加载配置的热键。默认值为 Ctrl+R。"
                );
            FlushAllStoreHotkey = BetterExperience.Instance.Config.Bind(
                SectionHotkey,
                nameof(FlushAllStoreHotkey),
                "F",
                "The hotkey to flush all store. Default is F.\n一键刷新商店的热键。默认值为 F。"
                );

            EnableBetterReelEffect = BetterExperience.Instance.Config.Bind(
                SectionReel,
                nameof(EnableBetterReelEffect),
                true,
                "Enable better reel effect.\n启用更好的转轮效果。"
                );
            EnableRemoveLimitInTreasureChests = BetterExperience.Instance.Config.Bind(
                SectionReel,
                nameof(EnableRemoveLimitInTreasureChests),
                false,
                "Enable removal of the 99-item limit in treasure chests.\n启用移除宝箱99物品数量上限。"
                );

            EnableBeingAttacked = BetterExperience.Instance.Config.Bind(
                SectionPlayerStatus,
                nameof(EnableBeingAttacked),
                true,
                "Enable being attacked. If disabled, players will not be attacked by enemies, but traps may still be triggered.\n" +
                "启用被攻击。若关闭，玩家将不会受到敌人的攻击，但仍可能触发陷阱。"
                );
            EnableNoHpDamage = BetterExperience.Instance.Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoHpDamage),
                false,
                "Enable no HP damage.\n启用无 HP 伤害。"
                );
            EnableNoMpDamage = BetterExperience.Instance.Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoMpDamage),
                false,
                "Enable no MP damage.\n启用无 MP 伤害。"
                );
            EnableNoEpDamage = BetterExperience.Instance.Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoEpDamage),
                false,
                "Enable no EP damage.\n启用无 EP 伤害。玩家“好感度”将不会增加。"
                );

            EnableWormTrap = BetterExperience.Instance.Config.Bind(
                SectionMapTrap,
                nameof(EnableWormTrap),
                true,
                "Enable worm trap.\n启用虫墙。"
                );
            EnableMapDamage = BetterExperience.Instance.Config.Bind(
                SectionMapTrap,
                nameof(EnableMapDamage),
                true,
                "Enable map damage, including spikes, thorns, electric shock, and acid. Disabling will prevent taking the above damage.\n" +
                "启用地图伤害，包括地刺，荆棘，电击，酸液。禁用后将不再受到以上伤害"
                );
            EnableDrowning = BetterExperience.Instance.Config.Bind(
                SectionMapTrap,
                nameof(EnableDrowning),
                true,
                "Enable drowning. Disabling will prevent drowning damage.\n启用溺水。禁用后将不再受到溺水伤害。"
                );

            BackpackCapacity = BetterExperience.Instance.Config.Bind(
                SectionBackpack,
                nameof(BackpackCapacity),
                500,
                "The backpack capacity. Default is 500.\n背包容量。默认值为 500。"
                );

            EnableLockCurrencyGoldCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyGoldCount),
                false,
                "Enable lock gold count. It will prevent the gold count from increasing or decreasing when you get gold.\n" +
                "启用金币数量锁定。开启后在获得金币时，数量不会增加或减少。"
                );
            EnableLockCurrencyCraftsCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyCraftsCount),
                false,
                "Enable lock crafts count. It will prevent the crafts count from increasing or decreasing when you get crafts.\n" +
                "启用兑锭数量锁定。开启后在获得兑锭时，数量不会增加或减少。"
                );
            EnableLockCurrencyJuiceCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(EnableLockCurrencyJuiceCount),
                false,
                "Enable lock juice count. It will prevent the juice count from increasing or decreasing when you get juice.\n" +
                "启用精萃数量锁定。开启后在获得精萃时，数量不会增加或减少。"
                );
            LockCurrencyGoldCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyGoldCount),
                -1L,
                "The gold count to lock. It will be used when lock gold count is enabled. Set to -1 to maintain the original count.\n" +
                "要锁定的金币数量。在启用金币数量锁定时生效。设为 -1 可保持原始数量。"
                );
            LockCurrencyCraftsCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyCraftsCount),
                -1L,
                "The crafts count to lock. It will be used when lock crafts count is enabled. Set to -1 to maintain the original count.\n" +
                "要锁定的兑锭数量。在启用兑锭数量锁定时生效。设为 -1 可保持原始数量。"
                );
            LockCurrencyJuiceCount = BetterExperience.Instance.Config.Bind(
                SectionCurrency,
                nameof(LockCurrencyJuiceCount),
                -1L,
                "The juice count to lock. It will be used when lock juice count is enabled. Set to -1 to maintain the original count.\n" +
                "要锁定的精萃数量。在启用精萃数量锁定时生效。设为 -1 可保持原始数量。"
                );

            EnableSensitivities = BetterExperience.Instance.Config.Bind(
                SectionTexture,
                nameof(EnableSensitivities),
                true,
                "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures.\n" +
                "启用敏感内容贴图。若关闭，将不会加载 BetterExperience\\ReplaceTexture\\Sensitive 文件夹中的贴图来替换原始贴图。"
                );
            EnableTextureImmediateReload = BetterExperience.Instance.Config.Bind(
                SectionTexture,
                nameof(EnableTextureImmediateReload),
                false,
                "Enable immediate reload. If enabled, the texture will be reloaded immediately after the texture is changed.\n" +
                "启用立即重载。开启后，贴图发生变化时会立刻重新加载。"
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
