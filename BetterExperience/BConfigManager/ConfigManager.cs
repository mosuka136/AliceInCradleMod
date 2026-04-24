using BetterExperience.HClassAttribute;
using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        private static readonly object _configSyncRoot = new object();

        public static ConfigFileManager Config { get; private set; }

        public static ConfigSheet Sheet => Config.Sheet;

        private ConfigManager()
        {
            
        }

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<LanguageType> SetLanguage { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }
        [ConfigSlider(-1f, 20f, 0.1f)]
        public static ConfigEntry<float> SetLootDropRatio { get; private set; }

        private const string SectionGeneral = "General";

        public static void Initialize(string configFilePath)
        {
            lock (_configSyncRoot)
            {
                Config = new ConfigFileManager(configFilePath);

                Config.SaveOnConfigSet = false;

                Config.CreateTable(SectionGeneral, new Translator(chinese: "通用", english: "General"));

                EnableBetterExperience = Config.Bind(
                    SectionGeneral,
                    nameof(EnableBetterExperience),
                    true,
                    new Translator(chinese: "启用更好的体验", english: "Enable BetterExperience"),
                    new Translator(
                        chinese: "启用更好的体验模组,必须在游戏启动前设置。",
                        english: "Enable Better Experience mod, must be set before launching the game."
                    )
                    );
                SetLanguage = Config.Bind(
                    SectionGeneral,
                    nameof(SetLanguage),
                    LanguageType.English,
                    new Translator(chinese: "设置语言", english: "Set Language"),
                    new Translator()
                    );
                EnableFlushAllStore = Config.Bind(
                    SectionGeneral,
                    nameof(EnableFlushAllStore),
                    false,
                    new Translator(chinese: "启用一键刷新商店", english: "Enable Flush All Store"),
                    new Translator(
                        chinese: "启用一键刷新商店功能。",
                        english: "Enable flush all store function."
                    )
                    );
                EnableRemoveLimitInBenchMenu = Config.Bind(
                    SectionGeneral,
                    nameof(EnableRemoveLimitInBenchMenu),
                    false,
                    new Translator(chinese: "启用移除椅子菜单限制", english: "Enable Remove Limit In Bench Menu"),
                    new Translator(
                        chinese: "启用移除玩家在某些情况下椅子菜单中的某些选项不可用的限制,必须在游戏启动前设置。",
                        english: "Enable the restriction that certain options in the chair menu are unavailable for players under specific circumstances, must be set before launching the game."
                    )
                    );
                EnableBetterFishing = Config.Bind(
                    SectionGeneral,
                    nameof(EnableBetterFishing),
                    false,
                    new Translator(chinese: "启用更好的钓鱼", english: "Enable Better Fishing"),
                    new Translator(
                        chinese: "启用更好的钓鱼。它将允许玩家更容易地钓到鱼。",
                        english: "Enable better fishing. It will allow players to catch fish more easily."
                    )
                    );
                EnableDebugMode = Config.Bind(
                    SectionGeneral,
                    nameof(EnableDebugMode),
                    false,
                    new Translator(chinese: "启用调试模式", english: "Enable Debug Mode"),
                    new Translator(
                        chinese: "启用调试模式,必须在游戏启动前设置。",
                        english: "Enable debug mode, must be set before launching the game."
                    )
                    );
                SetLootDropRatio = Config.Bind(
                    SectionGeneral,
                    nameof(SetLootDropRatio),
                    -1f,
                    new Translator(chinese: "设置战利品掉落倍率", english: "Set Loot Drop Ratio"),
                    new Translator(
                        chinese: "设置战利品掉落倍率。默认值为 -1，表示不改变。设置为 0 禁用掉落，设置为 n(n >= 1) 为 n 倍掉落。",
                        english: "Set loot drop ratio. Default value is -1, which means no change. Set it to 0 to disable loot drop. Set it to n(n >= 1) to multiply loot drop by n."
                    )
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
        }

        public static void ReloadConfig()
        {
            lock (_configSyncRoot)
            {
                Config.Reload();
            }
        }
    }
}
