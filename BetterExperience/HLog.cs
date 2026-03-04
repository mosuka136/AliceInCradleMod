using BepInEx.Logging;
using BetterExperience;
using BetterExperience.BepConfigManager;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterExperience
{
    internal sealed class HLog
    {
        private static StreamWriter _writer;
        private static object _lock = new object();

        private static LogLevel _logLevel;
        private static LogLevel _bepInExLogLevel;

        private static ManualLogSource _bepLog = null;
        private static int _seq = 0;

        private HLog()
        {

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize(
            string loggerPath,
            string loggerName,
            ManualLogSource bepLog = null,
            LogLevel logLevel = LogLevel.Info,
            LogLevel bepInExLogLevel = LogLevel.Warning)
        {
            _bepLog = bepLog;
            _logLevel = logLevel;
            _bepInExLogLevel = bepInExLogLevel;

            if (!ConfigManager.EnableHarmonyLog.Value)
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

            Application.quitting += Shutdown;
        }

        public static void Info(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => Write(LogLevel.Info, msg, null, member, file, line);
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
            if (!ConfigManager.EnableHarmonyLog.Value)
                return;

            if (_writer == null)
                return;

            lock (_lock)
            {
                _writer.WriteLine();
            }
        }

        private static void Write(
            LogLevel logLevel,
            string msg,
            Exception ex,
            string member,
            string file,
            int line)
        {
            if (!ConfigManager.EnableHarmonyLog.Value)
                return;

            if (_writer == null)
                return;

            var sb = new StringBuilder(256);

            int id = System.Threading.Interlocked.Increment(ref _seq);
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            int frame = Time.frameCount;
            string scene = SafeSceneName();

            sb.Append('[').Append(id).Append("] ")
              .Append(time).Append(" T").Append(threadId)
              .Append(" F").Append(frame)
              .Append(" S=").Append(scene)
              .Append(" ").Append(logLevel.ToString()).Append(" | ").Append(msg)
              .Append(" (").Append(Path.GetFileName(file))
              .Append(':').Append(line)
              .Append(" ").Append(member).Append(')');

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
                if (_bepLog == null)
                {
                    Debug.Log(final);
                }
                else
                {
                    switch (logLevel)
                    {
                        case LogLevel.Error:
                            _bepLog.LogError(final);
                            break;
                        case LogLevel.Warning:
                            _bepLog.LogWarning(final);
                            break;
                        case LogLevel.Info:
                            _bepLog.LogInfo(final);
                            break;
                        default:
                            _bepLog.LogInfo(final);
                            break;
                    }
                }
            }
        }

        private static string SafeSceneName()
        {
            try
            {
                string scene = SceneManager.GetActiveScene().name;
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

        private static void Shutdown()
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

        public enum LogLevel
        {
            Info = 0,
            Warning,
            Error
        }
    }
}