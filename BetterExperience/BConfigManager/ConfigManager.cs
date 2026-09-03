using BetterExperience.BLogSpace;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.BConfigManager
{
    /// <summary>
    /// 插件配置项声明入口。
    /// 该类把配置文件模型中的表/项绑定为静态强类型属性，供补丁、GUI 和输入处理直接读取。
    /// 具体配置项按功能拆分到同名 partial 文件中，避免单个文件过长。
    /// </summary>
    public static partial class ConfigManager
    {
        // 初始化期间会集中改写全部静态配置项引用，加锁串行化以避免其他入口在绑定完成前读到 null。
        private static readonly object _configSyncRoot = new object();

        /// <summary>
        /// 当前配置文件管理器。
        /// </summary>
        public static ConfigService Config => BService.Config;

        /// <summary>
        /// 运行时配置表集合，供 GUI 构建配置页使用。
        /// </summary>
        public static ConfigSheet Sheet => Config.Sheet;

        public static ConfigEntry<bool> EnableBetterExperience { get; private set; }
        public static ConfigEntry<LanguageType> SetLanguage { get; private set; }
        public static ConfigEntry<bool> EnableFlushAllStore { get; private set; }
        public static ConfigEntry<bool> EnableRemoveLimitInBenchMenu { get; private set; }
        public static ConfigEntry<bool> EnableBetterFishing { get; private set; }
        public static ConfigEntry<bool> EnableDamageCounter { get; private set; }
        public static ConfigEntry<bool> EnableDebugMode { get; private set; }
        [EntrySlider(-1f, 20f, 0.1f)]
        public static ConfigEntry<float> SetLootDropRatio { get; private set; }

        private const string SectionGeneral = "General";

        /// <summary>
        /// 将“读档后预加载”开关与对应设置值绑定为同一个双值配置项。
        /// 返回项的 <c>Value1</c> 为是否在读档完成后自动应用设置值（默认 false），
        /// <c>Value2</c> 为要应用的值；各调用方的默认值均约定为 -1，表示不覆盖游戏当前值。
        /// 两个子项的说明文案固定，具体用途说明由调用方通过 <paramref name="description"/> 提供。
        /// </summary>
        private static ConfigEntry<bool, T> BindPreloadValue<T>(
            string section,
            string key,
            T defaultValue,
            Translator name,
            Translator description)
        {
            return Config.Bind(
                section,
                key,
                false,
                defaultValue,
                name,
                description,
                new Translator(
                    chinese: "是否在存档读取后自动应用设置值。",
                    english: "Whether to apply the configured value automatically after loading a save."
                ),
                new Translator(
                    chinese: "要应用的设置值。",
                    english: "The configured value to apply."
                )
                );
        }

        public static void Initialize()
        {
            lock (_configSyncRoot)
            {
                try
                {
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

                    SetLanguage.OnValueChanged += (s, e) => Translator.DefaultLanguage = e;
                    Translator.DefaultLanguage = SetLanguage.Value;
                }
                catch (Exception ex)
                {
                    BLog.Error("Failed to initialize config manager.", ex);
                }

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

                BLog.Info($"Config manager initialized.");
            }
        }
    }
}
