using System;
using System.Diagnostics;

namespace BetterExperience
{
    internal static class HLog
    {
        public static void Info(string msg, string member = "", string file = "", int line = 0)
        {
            Trace.WriteLine($"[INFO] {msg}");
        }

        public static void Error(string msg, Exception ex = null, string member = "", string file = "", int line = 0)
        {
            Trace.WriteLine($"[ERROR] {msg}");
            if (ex != null)
                Trace.WriteLine(ex);
        }
    }
}
