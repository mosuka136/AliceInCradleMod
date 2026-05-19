using BetterExperience.HClassAttribute;
using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    /// <summary>
    /// 插件配置项声明入口。
    /// 该类把配置文件模型中的表/项绑定为静态强类型属性，供补丁、GUI 和输入处理直接读取。
    /// 具体配置项按功能拆分到同名 partial 文件中，避免单个文件过长。
    /// </summary>
    public partial class ConfigManager
    {
        // 初始化和重载都会改写静态配置引用，需要串行化以避免 GUI 或输入回调读到中间状态。
        private static readonly object _configSyncRoot = new object();

        /// <summary>
        /// 当前配置文件管理器。
        /// </summary>
        public static ConfigFileManager Config { get; private set; }

        /// <summary>
        /// 运行时配置表集合，供 GUI 构建配置页使用。
        /// </summary>
        public static ConfigSheet Sheet => Config.Sheet;

        private ConfigManager()
        {
            
        }

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<LanguageType> SetLanguage { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableDamageCounter { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }
        [ConfigSlider(-1f, 20f, 0.1f)]
        public static ConfigEntry<float> SetLootDropRatio { get; private set; }

        private const string SectionGeneral = "General";

        /// <summary>
        /// 初始化全部配置表和配置项。
        /// 调用会读取现有配置文件，补齐缺失项，并在完成绑定后保存一次规范化后的配置文件。
        /// </summary>
        /// <param name="configFilePath">配置文件路径。</param>
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
                        chinese: "启用更好的体验模组，必须在游戏启动前设置。",
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
                        chinese: "启用移除玩家在某些情况下椅子菜单中的某些选项不可用的限制，必须在游戏启动前设置。",
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
                EnableDamageCounter = Config.Bind(
                    SectionGeneral,
                    nameof(EnableDamageCounter),
                    false,
                    new Translator(chinese: "启用伤害计数器", english: "Enable Damage Counter"),
                    new Translator(
                        chinese: "启用伤害计数器。它将显示玩家与魔物造成的伤害。",
                        english: "Enable damage counter. It will display the damage dealt by the player and monsters."
                    )
                    );
                EnableDebugMode = Config.Bind(
                    SectionGeneral,
                    nameof(EnableDebugMode),
                    false,
                    new Translator(chinese: "启用调试模式", english: "Enable Debug Mode"),
                    new Translator(
                        chinese: "启用调试模式，必须在游戏启动前设置。",
                        english: "Enable debug mode, must be set before launching the game."
                    )
                    );
                SetLootDropRatio = Config.Bind(
                    SectionGeneral,
                    nameof(SetLootDropRatio),
                    -1f,
                    new Translator(chinese: "设置战利品掉落倍率", english: "Set Loot Drop Ratio"),
                    new Translator(
                        chinese: "设置战利品掉落倍率。默认值为 -1，表示不改变。设置为 0 禁用掉落，设置为 n（n >= 1）为 n 倍掉落。",
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

                HLog.Info($"Config manager initialized: {configFilePath}");
            }
        }

        /// <summary>
        /// 从磁盘重新读取配置，并将已有静态配置项重新绑定到新文件项。
        /// </summary>
        public static void ReloadConfig()
        {
            lock (_configSyncRoot)
            {
                Config.Reload();
                HLog.Info("Config file reloaded.");
            }
        }
    }
}
