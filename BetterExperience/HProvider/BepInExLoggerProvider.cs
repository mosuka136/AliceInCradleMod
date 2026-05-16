using BepInEx.Logging;

namespace BetterExperience.HProvider
{
    /// <summary>
    /// BepInEx 日志器的薄封装。
    /// 该封装让 <see cref="HLog"/> 不直接依赖插件入口类中的 Logger 属性，便于在没有 BepInEx 日志器时回退到 Unity Debug 日志。
    /// </summary>
    public class BepInExLoggerProvider
    {
        /// <summary>
        /// 底层 BepInEx 日志源。
        /// </summary>
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

        public void LogDebug(object data)
        {
            Logger.LogDebug(data);
        }
    }
}
