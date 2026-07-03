using UnityModBase.HLogSpace;
using UnityModBase.HProvider;

namespace BetterExperience.BLogSpace
{
    public static class BepInExLog
    {
        public static bool Enable { get; set; } = true;
        public static LogLevel Level { get; set; }
        public static UnityProvider UnityProvider { get; private set; }
        public static BepInExLoggerProvider BepInExLogger { get; private set; }

        public static void Initialize(LogLevel logLevel, UnityProvider unityProvider, BepInExLoggerProvider bepInExLogger)
        {
            Level = logLevel;
            UnityProvider = unityProvider;
            BepInExLogger = bepInExLogger;
        }

        public static void Log(LogEntry logEntry)
        {
            try
            {
                if (!Enable)
                    return;

                if (logEntry.Level < Level)
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
