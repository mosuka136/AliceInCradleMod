using BepInEx.Configuration;

namespace BetterExperience
{
    internal sealed class ConfigManager
    {
        private ConfigManager()
        {
            
        }

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<bool> EnableHarmonyLog { get; private set; }
        public static ConfigEntry<bool> EnableMosaic { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableReplaceTexture { get; private set; }
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInPuppetNpcDefeated { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }

        private static ConfigEntry<string> LogReadme { get; set; }
        public static ConfigEntry<HLog.LogLevel> HarmonyLogLevel { get; private set; }
        public static ConfigEntry<HLog.LogLevel> BepInExLogLevel { get; private set; }

        private static ConfigEntry<string> HotkeyReadme { get; set; }
        public static ConfigEntry<string> ReloadConfigHotkey { get; private set; }
        public static ConfigEntry<string> FlushAllStoreHotkey { get; private set; }

        public static ConfigEntry<bool> EnableBetterReelEffect { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInTreasureChests { get; private set; }
        public static ConfigEntry<float> SetReelSpeed { get; private set; }

        public static ConfigEntry<bool> EnableBeingAttacked { get; private set; }
        public static ConfigEntry<bool> EnableNoHpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoMpDamage { get; private set; }
        public static ConfigEntry<bool> EnableNoEpDamage { get; private set; }
        public static ConfigEntry<bool> EnableInfiniteShield { get; private set; }
        public static ConfigEntry<bool> EnableHolyBurstFaint { get; private set; }
        public static ConfigEntry<bool> EnableMpBreak { get; private set; }
        public static ConfigEntry<bool> EnablePreloadBackpackCapacity { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerHp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerEp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxHp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxMp { get; private set; }
        public static ConfigEntry<bool> EnablePreloadPlayerMaxSatiety { get; private set; }
        public static ConfigEntry<bool> EnablePreloadOverChargeSlotCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadEnhancerSlotCount { get; private set; }
        public static ConfigEntry<int> SetBackpackCapacity { get; private set; }
        public static ConfigEntry<int> SetPlayerHp { get; private set; }
        public static ConfigEntry<int> SetPlayerMp { get; private set; }
        public static ConfigEntry<int> SetPlayerEp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxHp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxMp { get; private set; }
        public static ConfigEntry<int> SetPlayerMaxSatiety { get; private set; }
        public static ConfigEntry<int> SetOverChargeSlotCount { get; private set; }
        public static ConfigEntry<int> SetEnhancerSlotCount { get; private set; }
        public static ConfigEntry<float> SetPlayerWalkSpeed { get; private set; }

        private static ConfigEntry<string> CaneReadme { get; set; }
        public static ConfigEntry<bool> EnablePreloadCaneSwingSpeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastSpeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneBalance { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneEfficiency { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneRetention { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneLockOn { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneLongRange { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneShortRange { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneReach { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneNearPower { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneNearShotgunPower { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneStability { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneManaSplashRatio { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastspeedOverhold { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneDrainAfterLock { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneCastspeed { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCaneMagicPrepareSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneSwingSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneCastSpeed { get; private set; }
        public static ConfigEntry<float> SetCaneBalance { get; private set; }
        public static ConfigEntry<float> SetCaneEfficiency { get; private set; }
        public static ConfigEntry<float> SetCaneRetention { get; private set; }
        public static ConfigEntry<float> SetCaneLockOn { get; private set; }
        public static ConfigEntry<float> SetCaneLongRange { get; private set; }
        public static ConfigEntry<float> SetCaneShortRange { get; private set; }
        public static ConfigEntry<float> SetCaneReach { get; private set; }
        public static ConfigEntry<float> SetCaneNearPower { get; private set; }
        public static ConfigEntry<float> SetCaneNearShotgunPower { get; private set; }
        public static ConfigEntry<float> SetCaneStability { get; private set; }
        public static ConfigEntry<float> SetCaneManaSplashRatio { get; private set; }
        public static ConfigEntry<float> SetCaneCastspeedOverhold { get; private set; }
        public static ConfigEntry<float> SetCaneDrainAfterLock { get; private set; }
        public static ConfigEntry<float> SetCaneCastspeed { get; private set; }
        public static ConfigEntry<float> SetCaneMagicPrepareSpeed { get; private set; }

        public static ConfigEntry<bool> EnableWormTrap { get; private set; }
        public static ConfigEntry<bool> EnableMapDamage { get; private set; }
        public static ConfigEntry<bool> EnableDrowning { get; private set; }

        public static ConfigEntry<bool> EnablePreloadCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnablePreloadCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyGoldCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<bool> EnableLockCurrencyJuiceCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyGoldCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyCraftsCount { get; private set; }
        public static ConfigEntry<long> SetCurrencyJuiceCount { get; private set; }

        public static ConfigEntry<bool> EnableSensitivities { get; private set; }
        public static ConfigEntry<bool> EnableTextureImmediateReload { get; private set; }

        private const string SectionGeneral = "General";
        private const string SectionLog = "Log";
        private const string SectionHotkey = "Hotkey";
        private const string SectionReel = "Reel";
        private const string SectionPlayerStatus = "PlayerStatus";
        private const string SectionCane = "Cane";
        private const string SectionMapTrap = "MapTrap";
        private const string SectionCurrency = "Currency";
        private const string SectionTexture = "Texture";

        public static void Initialization()
        {
            var Config = BetterExperience.Instance.Config;

            Config.SaveOnConfigSet = false;

            EnableBetterExperience = Config.Bind(
                SectionGeneral,
                nameof(EnableBetterExperience),
                true,
                "Enable Better Experience mod.\n启用 Better Experience 模组。"
                );
            EnableHarmonyLog = Config.Bind(
                SectionGeneral,
                nameof(EnableHarmonyLog),
                true,
                "Enable Harmony log. It will generate a log file in BetterExperience\\logs folder.\n启用 Harmony 日志。将在 BetterExperience\\logs 文件夹中生成日志文件。"
                );
            EnableMosaic = Config.Bind(
                SectionGeneral,
                nameof(EnableMosaic),
                false,
                "Enable mosaic effect.\n启用马赛克效果。"
                );
            EnableFlushAllStore = Config.Bind(
                SectionGeneral,
                nameof(EnableFlushAllStore),
                false,
                "Enable flush all store function.\n启用一键刷新商店功能。"
                );
            EnableReplaceTexture = Config.Bind(
                SectionGeneral,
                nameof(EnableReplaceTexture),
                false,
                "Enable replace texture. " +
                "It will use the texture from the BetterExperience\\ReplaceTexture folder to replace the original texture.\n" +
                "Please ensure that the file to be replaced has the same name as the original file.\n" +
                "Supported file formats are PNG files, with extensions .png or .btep.\n" +
                "启用替换贴图。将使用 BetterExperience\\ReplaceTexture 文件夹中的贴图替换原始贴图。\n" +
                "请确保需要替换的文件和被替换的文件名相同。支持的文件格式为png文件，后缀可为.png或.btep。"
                );
            EnableBetterSaveSite = Config.Bind(
                SectionGeneral,
                nameof(EnableBetterSaveSite),
                false,
                "Enable better save site. It will allow saving anywhere.\n启用更好的存档点功能。允许在任意位置保存。"
                );
            EnableRemoveLimitInPuppetNpcDefeated = Config.Bind(
                SectionGeneral,
                nameof(EnableRemoveLimitInPuppetNpcDefeated),
                false,
                "Enable the restriction that prevents the Puppet Merchant from spawning before the revenge quest is completed.\n" +
                "启用移除木偶商人在复仇战未完成前无法生成的限制。"
                );
            EnableRemoveLimitInBenchMenu = Config.Bind(
                SectionGeneral,
                nameof(EnableRemoveLimitInBenchMenu),
                false,
                "Enable the restriction that certain options in the chair menu are unavailable for players under specific circumstances\n" +
                "启用移除玩家在某些情况下椅子菜单中的某些选项不可用的限制。"
                );

            LogReadme = Config.Bind(
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
            HarmonyLogLevel = Config.Bind(
                SectionLog,
                nameof(HarmonyLogLevel),
                HLog.LogLevel.Warning,
                "The log level of Harmony log. Default is Info.\nHarmony 日志等级。默认值为 Info。"
                );
            BepInExLogLevel = Config.Bind(
                SectionLog,
                nameof(BepInExLogLevel),
                HLog.LogLevel.Warning,
                "The log level of BepInEx log. Default is Warning.\nBepInEx 日志等级。默认值为 Warning。"
                );

            HotkeyReadme = Config.Bind(
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
            ReloadConfigHotkey = Config.Bind(
                SectionHotkey,
                nameof(ReloadConfigHotkey),
                "Ctrl+R",
                "The hotkey to reload config. Default is Ctrl+R.\n重新加载配置的热键。默认值为 Ctrl+R。"
                );
            FlushAllStoreHotkey = Config.Bind(
                SectionHotkey,
                nameof(FlushAllStoreHotkey),
                "F",
                "The hotkey to flush all store. Default is F.\n一键刷新商店的热键。默认值为 F。"
                );

            EnableBetterReelEffect = Config.Bind(
                SectionReel,
                nameof(EnableBetterReelEffect),
                true,
                "Enable better reel effect.\n启用更好的转轮效果。"
                );
            EnableRemoveLimitInTreasureChests = Config.Bind(
                SectionReel,
                nameof(EnableRemoveLimitInTreasureChests),
                false,
                "Enable removal of the 99-item limit in treasure chests.\n启用移除宝箱99物品数量上限。"
                );
            SetReelSpeed = Config.Bind(
                SectionReel,
                nameof(SetReelSpeed),
                -1f,
                "Set reel speed. Set a value between 0 and 1 to adjust the wheel speed. The larger the value, the slower the speed.\n" +
                "设置转轮速度。设为 0 和 1 之间的值可调节转轮速度。数值越大速度越慢。"
                );

            EnableBeingAttacked = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableBeingAttacked),
                true,
                "Enable being attacked. If disabled, players will not be attacked by enemies, but traps may still be triggered.\n" +
                "启用被攻击。若关闭，玩家将不会受到敌人的攻击，但仍可能触发陷阱。"
                );
            EnableNoHpDamage = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoHpDamage),
                false,
                "Enable no HP damage.\n启用无 HP 伤害。"
                );
            EnableNoMpDamage = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoMpDamage),
                false,
                "Enable no MP damage.\n启用无 MP 伤害。"
                );
            EnableNoEpDamage = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableNoEpDamage),
                false,
                "Enable no EP damage.\n启用无 EP 伤害。玩家“好感度”将不会增加。"
                );
            EnableInfiniteShield = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableInfiniteShield),
                false,
                "Enable infinite shield.\n启用无限护盾。"
                );
            EnableHolyBurstFaint = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableHolyBurstFaint),
                true,
                "Enable Holy Burst Faint. When disabled, players will not faint after using Holy Burst.\n" +
                "启用圣光爆发昏厥。关闭后，玩家将不会因为使用圣光爆发而晕厥。"
                );
            EnableMpBreak = Config.Bind(
                SectionPlayerStatus,
                nameof(EnableMpBreak),
                true,
                "Enable MP break. When disabled, the player's MP slot will not break.\n" +
                "启用 MP 破裂。关闭后，玩家 MP 槽将不会破裂。"
                );
            EnablePreloadBackpackCapacity = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadBackpackCapacity),
                false,
                "Enable preload backpack capacity. When enabled, the backpack capacity will be automatically set once after loading a save, " +
                "using the configured value to override the original backpack capacity.\n" +
                "启用预加载背包容量。开启后，背包容量将在存档读取后自动设置一次，使用设置值覆盖原始的背包容量。"
                );
            EnablePreloadPlayerHp = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerHp),
                false,
                "Enable preload player HP. When enabled, the player HP will be automatically set once after loading a save, " +
                "using the configured value to override the original player HP.\n" +
                "启用预加载玩家 HP。开启后，玩家 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 HP。"
                );
            EnablePreloadPlayerMp = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerMp),
                false,
                "Enable preload player MP. When enabled, the player MP will be automatically set once after loading a save, " +
                "using the configured value to override the original player MP.\n" +
                "启用预加载玩家 MP。开启后，玩家 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 MP。"
                );
            EnablePreloadPlayerEp = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerEp),
                false,
                "Enable preload player EP. When enabled, the player EP will be automatically set once after loading a save, " +
                "using the configured value to override the original player EP.\n" +
                "启用预加载玩家 EP。开启后，玩家 EP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家 EP。"
                );
            EnablePreloadPlayerMaxHp = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerMaxHp),
                false,
                "Enable preload player max HP. When enabled, the player max HP will be automatically set once after loading a save, " +
                "using the configured value to override the original player max HP.\n" +
                "启用预加载玩家最大 HP。开启后，玩家最大 HP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 HP。"
                );
            EnablePreloadPlayerMaxMp = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerMaxMp),
                false,
                "Enable preload player max MP. When enabled, the player max MP will be automatically set once after loading a save, " +
                "using the configured value to override the original player max MP.\n" +
                "启用预加载玩家最大 MP。开启后，玩家最大 MP 将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大 MP。"
                );
            EnablePreloadPlayerMaxSatiety = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadPlayerMaxSatiety),
                false,
                "Enable preload player max satiety. When enabled, the player max satiety will be automatically set once after loading a save, " +
                "using the configured value to override the original player max satiety.\n" +
                "启用预加载玩家最大饱食度。开启后，玩家最大饱食度将在存档读取后自动设置一次，使用设置值覆盖原始的玩家最大饱食度。"
                );
            EnablePreloadOverChargeSlotCount = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadOverChargeSlotCount),
                false,
                "Enable preload over charge slot count. When enabled, the over charge slot count will be automatically set once after loading a save, " +
                "using the configured value to override the original over charge slot count.\n" +
                "启用预加载过充插槽数量。开启后，过充插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的过充插槽数量。"
                );
            EnablePreloadEnhancerSlotCount = Config.Bind(
                SectionPlayerStatus,
                nameof(EnablePreloadEnhancerSlotCount),
                false,
                "Enable preload enhancer slot count. When enabled, the enhancer slot count will be automatically set once after loading a save, " +
                "using the configured value to override the original enhancer slot count.\n" +
                "启用预加载强化插槽数量。开启后，强化插槽数量将在存档读取后自动设置一次，使用设置值覆盖原始的强化插槽数量。"
                );
            SetBackpackCapacity = Config.Bind(
                SectionPlayerStatus,
                nameof(SetBackpackCapacity),
                -1,
                "Set backpack capacity. It will override the original backpack capacity. Set to -1 to keep the capacity at its current value.\n" +
                "设置背包容量。将覆盖原始的背包容量。设为 -1 可保持为当前值。"
                );
            SetPlayerHp = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerHp),
                -1,
                "Set player HP. It will override the original player HP. Set to -1 to keep the HP at its current value.\n" +
                "设置玩家 HP。将覆盖原始的玩家 HP。设为 -1 可保持为当前值。"
                );
            SetPlayerMp = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerMp),
                -1,
                "Set player MP. It will override the original player MP. Set to -1 to keep the MP at its current value.\n" +
                "设置玩家 MP。将覆盖原始的玩家 MP。设为 -1 可保持为当前值。"
                );
            SetPlayerEp = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerEp),
                -1,
                "Set player EP. It will override the original player EP. Set to -1 to keep the EP at its current value.\n" +
                "设置玩家 EP。将覆盖原始的玩家 EP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxHp = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerMaxHp),
                -1,
                "Set player max HP. It will override the original player max HP. Set to -1 to keep the max HP at its current value.\n" +
                "设置玩家最大 HP。将覆盖原始的玩家最大 HP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxMp = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerMaxMp),
                -1,
                "Set player max MP. It will override the original player max MP. Set to -1 to keep the max MP at its current value.\n" +
                "设置玩家最大 MP。将覆盖原始的玩家最大 MP。设为 -1 可保持为当前值。"
                );
            SetPlayerMaxSatiety = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerMaxSatiety),
                -1,
                "Set player max satiety. It will override the original player max satiety. Set to -1 to keep the max satiety at its current value.\n" +
                "设置玩家最大饱食度。将覆盖原始的玩家最大饱食度。设为 -1 可保持为当前值。"
                );
            SetOverChargeSlotCount = Config.Bind(
                SectionPlayerStatus,
                nameof(SetOverChargeSlotCount),
                -1,
                "Set over charge slot count. It will override the original over charge slot count. Set to -1 to keep the over charge slot count at its current value.\n" +
                "设置过充插槽数量。将覆盖原始的过充插槽数量。设为 -1 可保持为当前值。"
                );
            SetEnhancerSlotCount = Config.Bind(
                SectionPlayerStatus,
                nameof(SetEnhancerSlotCount),
                -1,
                "Set enhancer slot count. It will override the original enhancer slot count. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置强化插槽数量。将覆盖原始的强化插槽数量。设为 -1 可保持为当前值。"
                );
            SetPlayerWalkSpeed = Config.Bind(
                SectionPlayerStatus,
                nameof(SetPlayerWalkSpeed),
                -1f,
                "Set player walk speed. The set value is a multiplier, where values between 0 and 1 decrease player speed, and values greater than 1 increase speed.\n" +
                "设置玩家行走速度。设置的值为倍率，即 0 - 1 内玩家速度减小，大于 1 速度增大。"
                );

            CaneReadme = Config.Bind(
                SectionCane,
                "_README",
                "",
                "Cane config notes / 法杖属性配置说明：\n" +
                "1) All SetCane* values use -1 to keep current value.\n" +
                "1) 所有 SetCane* 项设为 -1 表示保持当前值。\n" +
                "2) Value range / 取值范围：\n" +
                "   - Mana consumption efficiency (SetCaneEfficiency): 0-169 / 魔力消耗效率：0-169\n" +
                "   - Properties without a Chinese display-name mapping: >= 0 / 无对应中文命名的属性：>= 0\n" +
                "   - All other cane properties: 0-255 / 其余法杖属性：0-255\n" +
                "\n" +
                "Displayed stats / 面板显示属性计算公式：\n" +
                "- 近战攻击速度 (Swing Speed): 50 * near_punch_speed\n" +
                "- 近战攻击距离 (Reach): 50 * near_reach\n" +
                "- 近战威力 (Short Range): 55 * (0.25 * near_power + 0.75) * near_shotgun_power\n" +
                "- 射击威力 (Long Range): 46 * far_power\n" +
                "- 锁定性能 (Lock-On): 50 * lockon_power\n" +
                "- 魔力稳定性 (Retention): 55 * stability * mana_splash_ratio * (0.75 * castspeed_overhold + 0.25) * (0.5 * drain_after_lock + 0.5)\n" +
                "- 魔力消耗效率 (Efficiency): mp_use_ratio < 1 ? (169 - 104 * mp_use_ratio) : 65 / (mp_use_ratio * mp_use_ratio)\n" +
                "- 魔力亲和性 (Balance): 60 * neutral\n" +
                "- 咏唱速度 (Cast Speed): 50 * castspeed * (0.33 * magic_prepare_speed + 0.67) * (0.25 * castspeed_overhold + 0.75)\n" +
                "\n" +
                "Raw/internal properties (no Chinese display-name mapping) / 无中文命名映射的内部属性：\n" +
                "- SetCaneNearPower, SetCaneNearShotgunPower, SetCaneStability, SetCaneManaSplashRatio\n" +
                "- SetCaneCastspeedOverhold, SetCaneDrainAfterLock, SetCaneCastspeed, SetCaneMagicPrepareSpeed"
                );

            EnablePreloadCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneSwingSpeed),
                false,
                "Enable preload cane swing speed. When enabled, the cane swing speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane swing speed.\n" +
                "启用预加载法杖近战攻击速度。开启后，法杖近战攻击速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击速度。"
                );
            EnablePreloadCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastSpeed),
                false,
                "Enable preload cane cast speed. When enabled, the cane cast speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane cast speed.\n" +
                "启用预加载法杖咏唱速度。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。"
                );
            EnablePreloadCaneBalance = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneBalance),
                false,
                "Enable preload cane balance. When enabled, the cane balance will be automatically set once after loading a save, " +
                "using the configured value to override the original cane balance.\n" +
                "启用预加载法杖魔力亲和性。开启后，法杖魔力亲和性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力亲和性。"
                );
            EnablePreloadCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneEfficiency),
                false,
                "Enable preload cane efficiency. When enabled, the cane efficiency will be automatically set once after loading a save, " +
                "using the configured value to override the original cane efficiency.\n" +
                "启用预加载法杖魔力消耗效率。开启后，法杖魔力消耗效率将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力消耗效率。"
                );
            EnablePreloadCaneRetention = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneRetention),
                false,
                "Enable preload cane retention. When enabled, the cane retention will be automatically set once after loading a save, " +
                "using the configured value to override the original cane retention.\n" +
                "启用预加载法杖魔力稳定性。开启后，法杖魔力稳定性将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力稳定性。"
                );
            EnablePreloadCaneLockOn = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLockOn),
                false,
                "Enable preload cane lock-on. When enabled, the cane lock-on will be automatically set once after loading a save, " +
                "using the configured value to override the original cane lock-on.\n" +
                "启用预加载法杖锁定性能。开启后，法杖锁定性能将在存档读取后自动设置一次，使用设置值覆盖原始的法杖锁定性能。"
                );
            EnablePreloadCaneLongRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneLongRange),
                false,
                "Enable preload cane long-range attack range. When enabled, the cane long-range attack range will be automatically set once after loading a save, " +
                "using the configured value to override the original cane long-range attack range.\n" +
                "启用预加载法杖射击威力。开启后，法杖射击威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖射击威力。"
                );
            EnablePreloadCaneShortRange = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneShortRange),
                false,
                "Enable preload cane short-range attack range. When enabled, the cane short-range attack range will be automatically set once after loading a save, " +
                "using the configured value to override the original cane short-range attack range.\n" +
                "启用预加载法杖近战威力。开启后，法杖近战威力将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战威力。"
                );
            EnablePreloadCaneReach = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneReach),
                false,
                "Enable preload cane reach. When enabled, the cane reach will be automatically set once after loading a save, " +
                "using the configured value to override the original cane reach.\n" +
                "启用预加载法杖近战攻击距离。开启后，法杖近战攻击距离将在存档读取后自动设置一次，使用设置值覆盖原始的法杖近战攻击距离。"
                );
            EnablePreloadCaneNearPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearPower),
                false,
                "Enable preload cane near power. When enabled, the cane near power will be automatically set once after loading a save, " +
                "using the configured value to override the original cane near power.\n" +
                "启用预加载法杖 near power。开启后，法杖 near power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near power。"
                );
            EnablePreloadCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneNearShotgunPower),
                false,
                "Enable preload cane near shotgun power. When enabled, the cane near shotgun power will be automatically set once after loading a save, " +
                "using the configured value to override the original cane near shotgun power.\n" +
                "启用预加载法杖 near shotgun power。开启后，法杖 near shotgun power 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 near shotgun power。"
                );
            EnablePreloadCaneStability = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneStability),
                false,
                "Enable preload cane stability. When enabled, the cane stability will be automatically set once after loading a save, " +
                "using the configured value to override the original cane stability.\n" +
                "启用预加载法杖 stability。开启后，法杖 stability 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 stability。"
                );
            EnablePreloadCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneManaSplashRatio),
                false,
                "Enable preload cane mana splash ratio. When enabled, the cane mana splash ratio will be automatically set once after loading a save, " +
                "using the configured value to override the original cane mana splash ratio.\n" +
                "启用预加载法杖 mana splash ratio。开启后，法杖 mana splash ratio 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 mana splash ratio。"
                );
            EnablePreloadCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeedOverhold),
                false,
                "Enable preload cane castspeed overhold. When enabled, the cane castspeed overhold will be automatically set once after loading a save, " +
                "using the configured value to override the original cane castspeed overhold.\n" +
                "启用预加载法杖 castspeed overhold。开启后，法杖 castspeed overhold 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 castspeed overhold。"
                );
            EnablePreloadCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneDrainAfterLock),
                false,
                "Enable preload cane drain after lock. When enabled, the cane drain after lock will be automatically set once after loading a save, " +
                "using the configured value to override the original cane drain after lock.\n" +
                "启用预加载法杖 drain after lock。开启后，法杖 drain after lock 将在存档读取后自动设置一次，使用设置值覆盖原始的法杖 drain after lock。"
                );
            EnablePreloadCaneCastspeed = Config.
                Bind(
                SectionCane,
                nameof(EnablePreloadCaneCastspeed),
                false,
                "Enable preload cane castspeed. When enabled, the cane castspeed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane castspeed.\n" +
                "启用预加载法杖 castspeed。开启后，法杖咏唱速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖咏唱速度。"
                );
            EnablePreloadCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(EnablePreloadCaneMagicPrepareSpeed),
                false,
                "Enable preload cane magic prepare speed. When enabled, the cane magic prepare speed will be automatically set once after loading a save, " +
                "using the configured value to override the original cane magic prepare speed.\n" +
                "启用预加载法杖 magic prepare speed。开启后，法杖魔力准备速度将在存档读取后自动设置一次，使用设置值覆盖原始的法杖魔力准备速度。"
                );
            SetCaneSwingSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneSwingSpeed),
                -1f,
                "Set cane swing speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战攻击速度。设为 -1 可保持为当前值。"
                );
            SetCaneCastSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastSpeed),
                -1f,
                "Set cane cast speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖咏唱速度。设为 -1 可保持为当前值。"
                );
            SetCaneBalance = Config.Bind(
                SectionCane,
                nameof(SetCaneBalance),
                -1f,
                "Set cane balance. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力亲和性。设为 -1 可保持为当前值。"
                );
            SetCaneEfficiency = Config.Bind(
                SectionCane,
                nameof(SetCaneEfficiency),
                -1f,
                "Set cane efficiency. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力消耗效率。设为 -1 可保持为当前值。"
                );
            SetCaneRetention = Config.Bind(
                SectionCane,
                nameof(SetCaneRetention),
                -1f,
                "Set cane retention. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖魔力稳定性。设为 -1 可保持为当前值。"
                );
            SetCaneLockOn = Config.Bind(
                SectionCane,
                nameof(SetCaneLockOn),
                -1f,
                "Set cane lock-on. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖锁定性能。设为 -1 可保持为当前值。"
                );
            SetCaneLongRange = Config.Bind(
                SectionCane,
                nameof(SetCaneLongRange),
                -1f,
                "Set cane long-range attack range. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖射击威力。设为 -1 可保持为当前值。"
                );
            SetCaneShortRange = Config.Bind(
                SectionCane,
                nameof(SetCaneShortRange),
                -1f,
                "Set cane short-range attack range. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战威力。设为 -1 可保持为当前值。"
                );
            SetCaneReach = Config.Bind(
                SectionCane,
                nameof(SetCaneReach),
                -1f,
                "Set cane reach. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖近战攻击距离。设为 -1 可保持为当前值。"
                );
            SetCaneNearPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearPower),
                -1f,
                "Set cane near power. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 near power。设为 -1 可保持为当前值。"
                );
            SetCaneNearShotgunPower = Config.Bind(
                SectionCane,
                nameof(SetCaneNearShotgunPower),
                -1f,
                "Set cane near shotgun power. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 near shotgun power。设为 -1 可保持为当前值。"
                );
            SetCaneStability = Config.Bind(
                SectionCane,
                nameof(SetCaneStability),
                -1f,
                "Set cane stability. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 stability。设为 -1 可保持为当前值。"
                );
            SetCaneManaSplashRatio = Config.Bind(
                SectionCane,
                nameof(SetCaneManaSplashRatio),
                -1f,
                "Set cane mana splash ratio. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 mana splash ratio。设为 -1 可保持为当前值。"
                );
            SetCaneCastspeedOverhold = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeedOverhold),
                -1f,
                "Set cane castspeed overhold. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 castspeed overhold。设为 -1 可保持为当前值。"
                );
            SetCaneDrainAfterLock = Config.Bind(
                SectionCane,
                nameof(SetCaneDrainAfterLock),
                -1f,
                "Set cane drain after lock. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 drain after lock。设为 -1 可保持为当前值。"
                );
            SetCaneCastspeed = Config.Bind(
                SectionCane,
                nameof(SetCaneCastspeed),
                -1f,
                "Set cane castspeed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 castspeed。设为 -1 可保持为当前值。"
                );
            SetCaneMagicPrepareSpeed = Config.Bind(
                SectionCane,
                nameof(SetCaneMagicPrepareSpeed),
                -1f,
                "Set cane magic prepare speed. Set to -1 to keep the enhancer slot count at its current value.\n" +
                "设置法杖 magic prepare speed。设为 -1 可保持为当前值。"
                );

            EnableWormTrap = Config.Bind(
                SectionMapTrap,
                nameof(EnableWormTrap),
                true,
                "Enable worm trap.\n启用虫墙。"
                );
            EnableMapDamage = Config.Bind(
                SectionMapTrap,
                nameof(EnableMapDamage),
                true,
                "Enable map damage, including spikes, thorns, electric shock, and acid. Disabling will prevent taking the above damage.\n" +
                "启用地图伤害，包括地刺，荆棘，电击，酸液。禁用后将不再受到以上伤害"
                );
            EnableDrowning = Config.Bind(
                SectionMapTrap,
                nameof(EnableDrowning),
                true,
                "Enable drowning. Disabling will prevent drowning damage.\n启用溺水。禁用后将不再受到溺水伤害。"
                );

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

            EnableSensitivities = Config.Bind(
                SectionTexture,
                nameof(EnableSensitivities),
                true,
                "Enable sensitivities. If disabled, textures in the BetterExperience\\ReplaceTexture\\Sensitive folder will not be loaded to replace the original textures.\n" +
                "启用敏感内容贴图。若关闭，将不会加载 BetterExperience\\ReplaceTexture\\Sensitive 文件夹中的贴图来替换原始贴图。"
                );
            EnableTextureImmediateReload = Config.Bind(
                SectionTexture,
                nameof(EnableTextureImmediateReload),
                false,
                "Enable immediate reload. If enabled, the texture will be reloaded immediately after the texture is changed.\n" +
                "启用立即重载。开启后，贴图发生变化时会立刻重新加载。"
                );

            Config.SaveOnConfigSet = true;
            Config.Save();
        }

        public static void ReloadConfig()
        {
            BetterExperience.Instance.Config.Reload();
        }
    }
}
