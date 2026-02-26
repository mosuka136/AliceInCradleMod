using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        private ConfigManager()
        {
            
        }

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableBetterSaveSite { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInPuppetNpcDefeated { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableAccessWarehouseAnywhere { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }
        public static ConfigEntry<float> SetLootDropRatio { get; private set; }

        private const string SectionGeneral = "General";

        public static void Initialize()
        {
            var Config = BetterExperience.Instance.Config;

            Config.SaveOnConfigSet = false;

            EnableBetterExperience = Config.Bind(
                SectionGeneral,
                nameof(EnableBetterExperience),
                true,
                "Enable Better Experience mod.\n启用 Better Experience 模组。"
                );
            EnableFlushAllStore = Config.Bind(
                SectionGeneral,
                nameof(EnableFlushAllStore),
                false,
                "Enable flush all store function.\n启用一键刷新商店功能。"
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
            EnableBetterFishing = Config.Bind(
                SectionGeneral,
                nameof(EnableBetterFishing),
                false,
                "Enable better fishing. It will allow players to catch fish more easily.\n" +
                "启用更好的钓鱼。它将允许玩家更容易地钓到鱼。"
                );
            EnableAccessWarehouseAnywhere = Config.Bind(
                SectionGeneral,
                nameof(EnableAccessWarehouseAnywhere),
                false,
                "Enable access warehouse anywhere. It will allow players to access warehouse inventory anywhere. This will replace the original Chest Reels.\n" +
                "启用随时访问仓库。它将允许玩家在任何地方访问仓库库存。这将会取代原来的宝箱效果转轮。"
                );
            EnableDebugMode = Config.Bind(
                SectionGeneral,
                nameof(EnableDebugMode),
                false,
                "Enable debug mode, must be set before launching the game. \n启用调试模式,必须在游戏启动前设置。"
                );
            SetLootDropRatio = Config.Bind(
                SectionGeneral,
                nameof(SetLootDropRatio),
                -1f,
                "Set loot drop ratio. Default value is -1, which means no change. Set it to 0 to disable loot drop. Set it to n(n >= 1) to multiply loot drop by n.\n" +
                "设置战利品掉落倍率。默认值为 -1，表示不改变。设置为 0 禁用掉落，设置为 n(n >= 1) 为 n 倍掉落。"
                );

            InitializePlayerStatus();
            InitializeCane();
            InitializeReel();
            InitializeMapTrap();
            InitializeCurrency();
            InitializeTexture();
            InitializeHotkey();
            InitializeLog();

            Config.SaveOnConfigSet = true;
            Config.Save();
        }

        public static void ReloadConfig()
        {
            BetterExperience.Instance.Config.Reload();
        }
    }
}
