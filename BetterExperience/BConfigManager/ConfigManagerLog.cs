using BetterExperience.HConfigSpace;
using BetterExperience.HLogSpace;
using BetterExperience.HTranslatorSpace;
using System;

namespace BetterExperience.BConfigManager
{
    public partial class ConfigManager
    {
        public static ConfigEntry<bool> EnableHLog { get; private set; }
        public static ConfigEntry<HLog.LogLevel> HLogLevel { get; private set; }
        public static ConfigEntry<HLog.LogLevel> BepInExLogLevel { get; private set; }

        private const string SectionLog = "Log";

        /// <summary>
        /// 初始化插件独立日志和 BepInEx 日志同步等级配置。
        /// </summary>
        public static void InitializeLog()
        {
            try
            {
                Config.CreateTable(
                    SectionLog,
                    new Translator(chinese: "日志", english: "Log"),
                    new Translator(
                        chinese: "日志将生成在 BetterExperience\\logs 文件夹中。\n" +
                                 "日志和 BepInEx 日志的等级可分别设置。\n" +
                                 "若日志等级为 Info，将记录所有消息。\n" +
                                 "若日志等级为 Warning，仅记录警告和错误消息。\n" +
                                 "若日志等级为 Error，仅记录错误消息。",
                        english: "Logs will be generated in BetterExperience\\logs folder.\n" +
                                 "The log level of logs and BepInEx log can be set separately.\n" +
                                 "If the log level is set to Info, it will log all messages.\n" +
                                 "If the log level is set to Warning, it will only log warning and error messages.\n" +
                                 "If the log level is set to Error, it will only log error messages."
                    )
                );
                EnableHLog = Config.Bind(
                    SectionLog,
                    nameof(EnableHLog),
                    true,
                    new Translator(chinese: "启用日志", english: "Enable Log"),
                    new Translator(
                        chinese: "启用日志。将在 BetterExperience\\logs 文件夹中生成日志文件。",
                        english: "Enable log. It will generate a log file in BetterExperience\\logs folder."
                    )
                    );
                HLogLevel = Config.Bind(
                    SectionLog,
                    nameof(HLogLevel),
                    HLog.LogLevel.Info,
                    new Translator(chinese: "日志等级", english: "Log Level"),
                    new Translator(
                        chinese: "日志等级。默认值为 Info。",
                        english: "The log level. Default is Info."
                    )
                    );
                BepInExLogLevel = Config.Bind(
                    SectionLog,
                    nameof(BepInExLogLevel),
                    HLog.LogLevel.Warning,
                    new Translator(chinese: "BepInEx日志等级", english: "BepInEx Log Level"),
                    new Translator(
                        chinese: "BepInEx 日志等级。默认值为 Warning。",
                        english: "The log level of BepInEx log. Default is Warning."
                    )
                    );
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to initialize config manager for log.", ex);
            }
        }
    }
}
