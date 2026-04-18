using BepInEx.Logging;

namespace BetterExperience.HAdapter
{
    public class BepInExAdapter
    {
        public ManualLogSource Logger { get; }

        public BepInExAdapter(ManualLogSource logSource)
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
