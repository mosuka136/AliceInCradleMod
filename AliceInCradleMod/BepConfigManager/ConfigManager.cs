using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
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
