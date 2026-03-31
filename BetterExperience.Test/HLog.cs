using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BetterExperience
{
    internal static class HLog
    {
        public static void Info(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Trace.WriteLine($"[INFO] {msg}");
        }

        public static void Warn(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Trace.WriteLine($"[WARN] {msg}");
        }

        public static void Error(string msg, Exception ex = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            Trace.WriteLine($"[ERROR] {msg}");
            if (ex != null)
                Trace.WriteLine(ex);
        }
    }
}
