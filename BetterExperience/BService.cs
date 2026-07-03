using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using System;
using System.IO;
using UnityModBase;
using UnityModBase.HConfigSpace;
using UnityModBase.HLogSpace;
using UnityModBase.HProvider;

namespace BetterExperience
{
    internal static class BService
    {
        public static ServiceRegistry Service { get; private set; }
        public static LogDatabase LogDatabase => Service.LogDatabase;
        public static LogWriter LogWriter => Service.LogWriter;
        public static ConfigService Config => Service.Config;

        public static void Initialize(string baseDirectory, BepInExLoggerProvider logger)
        {
            try
            {
                Service = ServiceRegistry.Register(nameof(BetterExperience), nameof(BetterExperience));

                if (!Directory.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory);

                var configPath = Path.Combine(baseDirectory, $"{nameof(BetterExperience)}.cfg");
                Service.RegisterConfig(typeof(ConfigManager), configPath);
                ConfigManager.Initialize();

                Service.RegisterLog(Path.Combine(baseDirectory, "logs"), nameof(BetterExperience), ConfigManager.LogLevel.Value);
                ConfigManager.EnableLog.OnValueChanged += (s, e) => LogWriter.Enable = e;
                ConfigManager.LogLevel.OnValueChanged += (s, e) => LogWriter.Level = e;
                LogWriter.Enable = ConfigManager.EnableLog.Value;

                BepInExLog.Initialize(ConfigManager.BepInExLogLevel.Value, UnityProvider.Instance, logger);
                ConfigManager.EnableLog.OnValueChanged += (s, e) => BepInExLog.Enable = e;
                ConfigManager.BepInExLogLevel.OnValueChanged += (s, e) => BepInExLog.Level = ConfigManager.BepInExLogLevel.Value;
                LogDatabase.OnLogAdded += (entry) => BepInExLog.Log(entry);
                LogDatabase.OnLogRepeated += (entry) => BepInExLog.Log(entry);
                BepInExLog.Enable = ConfigManager.EnableLog.Value;
            }
            catch (Exception ex)
            {
                LogDatabase?.Error($"Failed to initialize {nameof(BService)}", ex, nameof(BService), null, 0);
            }
        }

        public static void Dispose()
        {
            try
            {
                Service?.Dispose();
                Service = null;
            }
            catch (Exception ex)
            {
                LogDatabase?.Error($"Failed to dispose {nameof(BService)}", ex, nameof(BService), null, 0);
            }
        }
    }
}
