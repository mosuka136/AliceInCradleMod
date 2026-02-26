using BepInEx.Configuration;

namespace BetterExperience.BepConfigManager
{
    internal sealed partial class ConfigManager
    {
        private static ConfigEntry<string> LogReadme { get; set; }
        public static ConfigEntry<bool> EnableHarmonyLog { get; private set; }
        public static ConfigEntry<HLog.LogLevel> HarmonyLogLevel { get; private set; }
        public static ConfigEntry<HLog.LogLevel> BepInExLogLevel { get; private set; }

        private const string SectionLog = "Log";

        public static void InitializeLog()
        {
            var Config = BetterExperience.Instance.Config;

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
            EnableHarmonyLog = Config.Bind(
                SectionLog,
                nameof(EnableHarmonyLog),
                true,
                "Enable Harmony log. It will generate a log file in BetterExperience\\logs folder.\n" +
                "启用 Harmony 日志。将在 BetterExperience\\logs 文件夹中生成日志文件。"
                );
            HarmonyLogLevel = Config.Bind(
                SectionLog,
                nameof(HarmonyLogLevel),
                HLog.LogLevel.Warning,
                "The log level of Harmony log. Default is Warning.\nHarmony 日志等级。默认值为 Warning。"
                );
            BepInExLogLevel = Config.Bind(
                SectionLog,
                nameof(BepInExLogLevel),
                HLog.LogLevel.Warning,
                "The log level of BepInEx log. Default is Warning.\nBepInEx 日志等级。默认值为 Warning。"
                );
        }
    }
}
