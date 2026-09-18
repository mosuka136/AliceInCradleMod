using $safeprojectname$.BLogSpace;
using System;
using UnityModBase.HConfigSpace;
using UnityModBase.HTranslatorSpace;

namespace $safeprojectname$.BConfigManager
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

        /// <summary>插件总开关：关闭后插件在 Awake 阶段直接停止初始化。</summary>
        public static ConfigEntry<bool> EnableMod { get; private set; }

        /// <summary>示例补丁开关：关闭溺水伤害（见 Patches\DisableDrowningPatch.cs）。</summary>
        public static ConfigEntry<bool> EnableNoDrowning { get; private set; }

        private const string SectionGeneral = "General";
        private const string SectionSample = "Sample";

        public static void Initialize()
        {
            lock (_configSyncRoot)
            {
                try
                {
                    Config.SaveOnConfigSet = false;

                    Config.CreateTable(SectionGeneral, new Translator(chinese: "通用", english: "General"));

                    EnableMod = Config.Bind(
                        SectionGeneral,
                        nameof(EnableMod),
                        true,
                        new Translator(chinese: "启用本模组", english: "Enable Mod"),
                        new Translator(
                            chinese: "启用本模组，必须在游戏启动前设置。",
                            english: "Enable this mod, must be set before launching the game."
                            )
                        );

                    // 示例分区：演示如何按功能拆分配置。新增功能配置建议新建 partial 文件（如 ConfigManagerXxx.cs）。
                    Config.CreateTable(SectionSample, new Translator(chinese: "示例功能", english: "Sample"));

                    EnableNoDrowning = Config.Bind(
                        SectionSample,
                        nameof(EnableNoDrowning),
                        false,
                        new Translator(chinese: "启用关闭溺水", english: "Enable No Drowning"),
                        new Translator(
                            chinese: "示例补丁：关闭水中的缺氧伤害。这是本模板自带的示例，可在 Patches\\DisableDrowningPatch.cs 查看写法。",
                            english: "Sample patch: disables drowning damage in water. See Patches\\DisableDrowningPatch.cs for how it is written."
                            )
                        );
                }
                catch (Exception ex)
                {
                    BLog.Error("Failed to initialize config manager.", ex);
                }

                InitializeLog();

                Config.SaveOnConfigSet = true;
                Config.Save();

                BLog.Info($"Config manager initialized.");
            }
        }
    }
}
