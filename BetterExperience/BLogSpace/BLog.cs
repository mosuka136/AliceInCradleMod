using System;
using System.Runtime.CompilerServices;
using UnityModBase.HLogSpace;

namespace BetterExperience.BLogSpace
{
    /// <summary>
    /// 插件内部日志工具。
    /// 它可以同时写入独立文件和 BepInEx 日志，但是否启用由配置控制；日志方法会吞掉自身异常，避免日志失败影响补丁逻辑。
    /// </summary>
    public static class BLog
    {
        public static LogDatabase LogDatabase => BService.LogDatabase;

        public static void Debug(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => LogDatabase.Debug(msg, member, file, line);

        public static void Info(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => LogDatabase.Info(msg, member, file, line);

        public static void Notice(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => LogDatabase.Notice(msg, member, file, line);

        public static void Warn(string msg,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => LogDatabase.Warn(msg, member, file, line);
 
        public static void Error(string msg, Exception ex = null,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0) => LogDatabase.Error(msg, ex, member, file, line);
    }
}
