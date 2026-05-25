using BetterExperience.HProvider;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.HLogSpace
{
    public static class BepInExHLog
    {
        public static LogLevel LogLevel { get; set; }
        public static UnityProvider UnityProvider { get; private set; }
        public static BepInExLoggerProvider BepInExLogger { get; private set; }

        public static void Initialize(LogLevel logLevel, UnityProvider unityProvider, BepInExLoggerProvider bepInExLogger)
        {
            LogLevel = logLevel;
            UnityProvider = unityProvider;
            BepInExLogger = bepInExLogger;
        }

        public static void Log(LogEntry logEntry)
        {
            try
            {
                if (logEntry.Level < LogLevel)
                    return;

                var msg = logEntry.ToString();

                if (BepInExLogger == null)
                {
                    UnityProvider?.DebugLog(msg);
                    return;
                }

                switch (logEntry.Level)
                {
                    case LogLevel.Error:
                        BepInExLogger.LogError(msg);
                        break;
                    case LogLevel.Warning:
                        BepInExLogger.LogWarning(msg);
                        break;
                    case LogLevel.Info:
                        BepInExLogger.LogInfo(msg);
                        break;
                    case LogLevel.Debug:
                        BepInExLogger.LogDebug(msg);
                        break;
                    default:
                        BepInExLogger.LogInfo(msg);
                        break;
                }
            }
            catch
            {
            }
        }
    }
}
