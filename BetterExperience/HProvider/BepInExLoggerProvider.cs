using BepInEx.Logging;

namespace BetterExperience.HProvider
{
    public class BepInExLoggerProvider
    {
        public ManualLogSource Logger { get; }

        public BepInExLoggerProvider(ManualLogSource logSource)
        {
            Logger = logSource;
        }

        public void LogError(object data)
        {
            Logger.LogError(data);
        }

        public void LogWarning(object data)
        {
            Logger.LogWarning(data);
        }

        public void LogInfo(object data)
        {
            Logger.LogInfo(data);
        }
    }
}
