using BetterExperience.ConfigFileSpace;
using System.Collections.Specialized;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        public static ConfigFileManager Config { get; private set; }

        public static OrderedDictionary Tables => Config.Tables;

        private ConfigManager()
        {
            
        }

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }
        public static ConfigEntry<float> SetLootDropRatio { get; private set; }

        private const string SectionGeneral = "General";

        public static void Initialize(string configFilePath)
        {
            Config = new ConfigFileManager(configFilePath);

            Config.SaveOnConfigSet = false;

            Config.CreateTable(SectionGeneral);

            EnableBetterExperience = Config.Bind(
                SectionGeneral,
                nameof(EnableBetterExperience),
                true,
                "Enable Better Experience mod, must be set before launching the game.\n" +
                "启用更好的体验模组,必须在游戏启动前设置。"
                );
            EnableFlushAllStore = Config.Bind(
                SectionGeneral,
                nameof(EnableFlushAllStore),
                false,
                "Enable flush all store function.\n启用一键刷新商店功能。"
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
            InitializeWeather();
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
            Config.Reload();
        }
    }
}
