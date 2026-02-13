using BepInEx.Logging;
using BetterExperience;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

internal sealed class HLog
{
    private static LogLevel _logLevel;
    private static LogLevel _bepinexLogLevel;

    private static ManualLogSource _bepLog = null;
    private static int _seq = 0;

    public static void Initialize(
        string loggerPath,
        string loggerName,
        ManualLogSource bepLog = null,
        LogLevel logLevel = LogLevel.Info,
        LogLevel bepinexLogLevel = LogLevel.Warning)
    {
        _bepLog = bepLog;
        _logLevel = logLevel;
        _bepinexLogLevel = bepinexLogLevel;

        if(!ConfigManager.EnableHarmonyLog.Value)
            return;

        if (!System.IO.Directory.Exists(loggerPath))
        {
            System.IO.Directory.CreateDirectory(loggerPath);
        }
        FileLog.logPath = System.IO.Path.Combine(loggerPath, loggerName);
        FileLog.Reset();
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
        [CallerLineNumber] int line = 0) => Write(LogLevel.Error, msg, ex , member, file, line);
    public static void WriteLine()
    {
        if (!ConfigManager.EnableHarmonyLog.Value)
            return;
        FileLog.Log("");
    }

    private static void Write(
        LogLevel logLevel,
        string msg,
        Exception ex,
        string member,
        string file,
        int line)
    {
        if(!ConfigManager.EnableHarmonyLog.Value)
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
          .Append(" (").Append(System.IO.Path.GetFileName(file))
          .Append(':').Append(line)
          .Append(" ").Append(member).Append(')');

        if (ex != null)
        {
            sb.AppendLine();
            sb.Append(ex);
        }

        string final = sb.ToString();

        if (logLevel >= _logLevel)
            FileLog.Log(final);

        if (logLevel >= _bepinexLogLevel)
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

    public enum LogLevel
    {
        Info = 0,
        Warning,
        Error
    }
}
