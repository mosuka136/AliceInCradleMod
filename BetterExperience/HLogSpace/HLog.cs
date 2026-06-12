using BetterExperience.HProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BetterExperience.HLogSpace
{
    /// <summary>
    /// 插件内部日志工具。
    /// 它可以同时写入独立文件和 BepInEx 日志，但是否启用由配置控制；日志方法会吞掉自身异常，避免日志失败影响补丁逻辑。
    /// </summary>
    public static class HLog
    {
        public const int MaxEarlyLogs = 200;

        private static readonly TimeSpan WriteInterval = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan LongestDuration = TimeSpan.FromSeconds(5);

        private static bool _initialized = false;
        private static int _seq = 0;
        private static Timer _timer;
        private static LogEntry _lastLog;
        private static StreamWriter _writer;
        private static readonly object _lock = new object();
        private static readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();

        public static event Action<LogEntry> OnLog;
        public static event Action<LogEntry> OnLogAdd;

        public static bool EnableLog { get; set; } = true;
        public static string LogDirectory { get; set; }
        public static string LogFileName { get; set; }
        public static LogLevel HLogLevel { get; set; } = LogLevel.Info;
        public static List<LogEntry> EarlyLogs { get; } = new List<LogEntry>();
        public static UnityProvider UnityProvider { get; set; }

        /// <summary>
        /// 初始化日志输出。
        /// 启用文件日志时会按小时追加到同一个文件，并在 Unity 退出时关闭写入器。
        /// </summary>
        /// <param name="logDirectory">日志目录。</param>
        /// <param name="logFileName">日志文件名前缀。</param>
        /// <param name="logLevel">写入独立日志文件的最低等级。</param>
        /// <param name="unity">Unity 访问适配器。</param>
        public static void Initialize(string logDirectory, string logFileName, LogLevel logLevel, UnityProvider unity)
        {
            LogDirectory = logDirectory;
            LogFileName = logFileName;
            HLogLevel = logLevel;
            UnityProvider = unity;

            if (_initialized)
                return;

            GameQuitManager.OnGameQuit += Dispose;
            _initialized = true;

            if (EnableLog)
                InitializeWriter();
        }

        /// <summary>
        /// 初始化日志写入器，可重复调用，重复调用会关闭之前的写入器并创建新的写入器。
        /// </summary>
        public static void InitializeWriter()
        {
            try
            {
                lock (_lock)
                {
                    if (_writer != null)
                        DisposeWriter();

                    _timer?.Dispose();

                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);

                    var fullPath = Path.Combine(LogDirectory, $"{Path.GetFileNameWithoutExtension(LogFileName)}-{DateTime.Now:yyyy-MM-dd-HH}.log");
                    var fs = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    _writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };

                    _writer.WriteLine($"{new string('-', 50)}LOG-START-{DateTime.Now}{new string('-', 50)}");

                    _timer = new Timer(s => FlushQueue(), null, WriteInterval, WriteInterval);
                }
            }
            catch
            {
            }
        }

        public static void Debug(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => WriteQueue(LogLevel.Debug, msg, null, member, file, line);

        public static void Info(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => WriteQueue(LogLevel.Info, msg, null, member, file, line);

        public static void Notice(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => WriteQueue(LogLevel.Notice, msg, null, member, file, line);

        public static void Warn(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => WriteQueue(LogLevel.Warning, msg, null, member, file, line);
 
        public static void Error(string msg, Exception ex = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => WriteQueue(LogLevel.Error, msg, ex, member, file, line);

        /// <summary>
        /// 该方法将日志写入队列，然后调用<see cref="FlushQueue"/>写入日志同时触发日志添加事件。
        /// </summary>
        /// <param name="logLevel">日志等级。</param>
        /// <param name="msg">日志消息。</param>
        /// <param name="ex">异常对象，可为 <c>null</c>。</param>
        /// <param name="member">调用该方法的成员名称，可为 <c>null</c>。</param>
        /// <param name="file">调用该方法的文件路径，可为 <c>null</c>。</param>
        /// <param name="line">调用该方法的行号。</param>
        public static void WriteQueue(LogLevel logLevel, string msg, Exception ex, string member, string file, int line)
        {
            try
            {
                int id = Interlocked.Increment(ref _seq);
                DateTime timestamp = DateTime.Now;
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int frame = UnityProvider?.FrameCount ?? 0;
                string scene = UnityProvider?.ActiveScene.name;
                scene = string.IsNullOrEmpty(scene) ? "?" : scene;

                var entry = new LogEntry(id, timestamp, threadId, frame, scene, logLevel, msg, file, line, member, ex);

                if (EarlyLogs.Count < MaxEarlyLogs)
                    EarlyLogs.Add(entry);

                foreach (var handler in (OnLog?.GetInvocationList() ?? Array.Empty<Delegate>()).Cast<Action<LogEntry>>())
                {
                    try { handler?.Invoke(entry); }
                    catch { }
                }

                if (entry.Equals(_lastLog))
                {
                    _lastLog.UpdateRepeat(timestamp);
                }
                else
                {
                    if (_lastLog?.IsRepeated == true)
                        _logEntries.Enqueue(_lastLog);
                    _logEntries.Enqueue(entry);
                    _lastLog = entry;
                    FlushQueue();
                }
            }
            catch
            {
            }
        }

        public static void WriteLog(LogEntry entry)
        {
            if (!EnableLog || entry == null)
                return;

            if (entry.Level < HLogLevel)
                return;

            _writer?.WriteLine(entry.ToString());

            foreach (var handler in (OnLogAdd?.GetInvocationList() ?? Array.Empty<Delegate>()).Cast<Action<LogEntry>>())
            {
                try { handler?.Invoke(entry); }
                catch { }
            }
        }

        /// <summary>
        /// 将日志队列中的日志写入文件，并触发日志添加事件。
        /// </summary>
        public static void FlushQueue()
        {
            if (!_initialized)
                return;

            if (EnableLog && _writer == null)
                InitializeWriter();

            try
            {
                lock (_lock)
                {
                    while (!_logEntries.IsEmpty)
                    {
                        if (_logEntries.TryDequeue(out var entry))
                            WriteLog(entry);
                    }

                    if (_lastLog?.IsRepeated == true &&
                        (DateTime.Now - _lastLog.LastRepeatTime >= WriteInterval ||
                        _lastLog.LastRepeatTime - _lastLog.Timestamp >= LongestDuration))
                    {
                        WriteLog(_lastLog);
                        _lastLog = null;
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 释放日志资源，关闭文件写入器和定时器。
        /// </summary>
        public static void Dispose()
        {
            DisposeWriter();
            _timer?.Dispose();
            _timer = null;
            GameQuitManager.OnGameQuit -= Dispose;
            _initialized = false;
        }

        /// <summary>
        /// 关闭文件日志写入器。
        /// 该方法可重复调用，Unity 退出事件和异常清理路径都可以安全使用。
        /// </summary>
        public static void DisposeWriter()
        {
            if (_writer == null)
                return;

            try
            {
                lock (_lock)
                {
                    FlushQueue();
                    if (_lastLog?.IsRepeated == true)
                    {
                        WriteLog(_lastLog);
                        _lastLog = null;
                    }

                    _writer.WriteLine($"{new string('-', 50)}LOG-END-{DateTime.Now}{new string('-', 50)}");
                    _writer.WriteLine();
                    _writer.WriteLine();

                    _writer.Flush();
                    _writer.Dispose();
                    _writer = null;
                }
            }
            catch
            {
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
