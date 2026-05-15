using BetterExperience.BConfigManager;
using BetterExperience.HProvider;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BetterExperience
{
    /// <summary>
    /// 插件内部日志工具。
    /// 它可以同时写入独立文件和 BepInEx 日志，但是否启用由配置控制；日志方法会吞掉自身异常，避免日志失败影响补丁逻辑。
    /// </summary>
    public static class HLog
    {
        private static StreamWriter _writer;
        // 文件写入需要串行化；Unity 主线程之外的补丁也可能调用日志。
        private static readonly object _lock = new object();

        private static LogLevel _logLevel;
        private static LogLevel _bepInExLogLevel;

        private static UnityProvider _unityProvider;
        private static BepInExLoggerProvider _bepLog = null;
        private static int _seq = 0;

        /// <summary>
        /// 初始化日志输出。
        /// 启用文件日志时会按小时追加到同一个文件，并在 Unity 退出时关闭写入器。
        /// </summary>
        /// <param name="loggerPath">日志目录。</param>
        /// <param name="loggerName">日志文件名前缀。</param>
        /// <param name="unity">Unity 访问适配器。</param>
        /// <param name="bepLog">可选 BepInEx 日志适配器；为空时回退到 Unity Debug 日志。</param>
        /// <param name="logLevel">写入独立日志文件的最低等级。</param>
        /// <param name="bepInExLogLevel">同步到 BepInEx/Unity 日志的最低等级。</param>
        public static void Initialize(
            string loggerPath,
            string loggerName,
            UnityProvider unity,
            BepInExLoggerProvider bepLog = null,
            LogLevel logLevel = LogLevel.Info,
            LogLevel bepInExLogLevel = LogLevel.Warning)
        {
            _unityProvider = unity;
            _bepLog = bepLog;
            _logLevel = logLevel;
            _bepInExLogLevel = bepInExLogLevel;

            if (ConfigManager.EnableHarmonyLog == null || !ConfigManager.EnableHarmonyLog.Value)
                return;

            if (!Directory.Exists(loggerPath))
            {
                Directory.CreateDirectory(loggerPath);
            }

            lock (_lock)
            {
                var fullPath = Path.Combine(loggerPath, $"{Path.GetFileNameWithoutExtension(loggerName)}-{DateTime.Now:yyyy-MM-dd-HH}.log");
                var fs = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };

                _writer.WriteLine($"{new string('-', 50)}LOG-START-{DateTime.Now}{new string('-', 50)}");
            }

            _unityProvider.UnityQuitting += Shutdown;
        }

        public static void Debug(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Debug, msg, null, member, file, line);

        public static void Info(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Info, msg, null, member, file, line);

        public static void Notice(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Notice, msg, null, member, file, line);

        public static void Warn(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Warning, msg, null, member, file, line);
        public static void Error(string msg, Exception ex = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Error, msg, ex, member, file, line);
        public static void WriteLine()
        {
            if (ConfigManager.EnableHarmonyLog == null || !ConfigManager.EnableHarmonyLog.Value)
                return;

            if (_writer == null)
                return;

            lock (_lock)
            {
                try
                {
                    _writer.WriteLine();
                }
                catch { }
            }
        }

        public static void Write(
            LogLevel logLevel,
            string msg,
            Exception ex,
            string member,
            string file,
            int line)
        {
            try
            {
                if (ConfigManager.EnableHarmonyLog == null || !ConfigManager.EnableHarmonyLog.Value)
                    return;

                if (_writer == null)
                    return;

                var sb = new StringBuilder(256);

                int id = System.Threading.Interlocked.Increment(ref _seq);
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                int frame = _unityProvider.FrameCount;
                string scene = SafeSceneName();

                // 序号、线程、帧和场景同时写入，便于排查 Harmony 补丁在不同回调和帧中的执行顺序。
                sb.Append('[').Append(id).Append("] ")
                  .Append(time).Append(" T").Append(threadId)
                  .Append(" F").Append(frame)
                  .Append(" S=").Append(scene)
                  .Append(" ").Append(logLevel.ToString()).Append(" | ").Append(msg);

                if (!string.IsNullOrEmpty(member) && !string.IsNullOrEmpty(Path.GetFileName(file)) && line > 0)
                {
                    sb.Append(" (").Append(Path.GetFileName(file))
                      .Append(':').Append(line)
                      .Append(" ").Append(member).Append(')');
                }

                if (ex != null)
                {
                    sb.AppendLine();
                    sb.Append(ex);
                }

                string final = sb.ToString();

                lock (_lock)
                {
                    if (logLevel >= _logLevel)
                        _writer.WriteLine(final);
                }

                if (logLevel >= _bepInExLogLevel)
                {
                    AdditionalLog(logLevel, final);
                }
            }
            catch { }
        }

        /// <summary>
        /// 将日志同步到 BepInEx 日志器或 Unity Debug 日志。
        /// </summary>
        public static void AdditionalLog(LogLevel level, string msg)
        {
            if (_bepLog == null)
            {
                _unityProvider.DebugLog(msg);
            }
            else
            {
                switch (level)
                {
                    case LogLevel.Error:
                        _bepLog.LogError(msg);
                        break;
                    case LogLevel.Warning:
                        _bepLog.LogWarning(msg);
                        break;
                    case LogLevel.Info:
                        _bepLog.LogInfo(msg);
                        break;
                    case LogLevel.Debug:
                        _bepLog.LogDebug(msg);
                        break;
                    default:
                        _bepLog.LogInfo(msg);
                        break;
                }
            }
        }

        /// <summary>
        /// 安全获取当前场景名。
        /// 场景系统尚未可用或场景名为空时返回 <c>?</c>。
        /// </summary>
        public static string SafeSceneName()
        {
            try
            {
                string scene = _unityProvider.ActiveScene.name;
                if (string.IsNullOrWhiteSpace(scene))
                {
                    return "?";
                }
                else
                {
                    return scene;
                }
            }
            catch { return "?"; }
        }

        /// <summary>
        /// 关闭文件日志写入器。
        /// 该方法可重复调用，Unity 退出事件和异常清理路径都可以安全使用。
        /// </summary>
        public static void Shutdown()
        {
            if (_writer != null)
            {
                lock (_lock)
                {
                    try
                    {
                        _writer.WriteLine($"{new string('-', 50)}LOG-END-{DateTime.Now}{new string('-', 50)}");
                        _writer.WriteLine();
                        _writer.WriteLine();

                        _writer.Flush();
                        _writer.Dispose();
                        _writer = null;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 日志等级。数值越大表示越严重。
        /// </summary>
        public enum LogLevel
        {
            Debug = 0,
            Info,
            Notice,
            Warning,
            Error
        }
    }
}
