using BetterExperience.BConfigManager;
using HarmonyLib;
using System;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在游戏基础工具初始化时同步调试标记。
        /// 只操作游戏全局调试开关，不影响插件自身日志等级。
        /// </summary>
        [HarmonyPatch]
        public class SwitchDebugPatch
        {
            private static readonly Traverse<bool> DebugAnnounceField = Traverse.Create(typeof(X)).Field<bool>("DEBUGANNOUNCE");
            private static readonly Traverse<bool> DebugTimestampField = Traverse.Create(typeof(X)).Field<bool>("DEBUGTIMESTAMP");

            [HarmonyPrefix]
            [HarmonyPatch(typeof(X), nameof(X.init1))]
            public static void Prefix()
            {
                if (DebugAnnounceField == null || DebugTimestampField == null)
                {
                    HLog.Error($"Failed to access debug fields in {nameof(SwitchDebugPatch)}.");
                    return;
                }

                try
                {
                    if (ConfigManager.EnableDebugMode.Value)
                    {
                        DebugAnnounceField.Value = true;
                        DebugTimestampField.Value = true;
                    }
                    else
                    {
                        DebugTimestampField.Value = false;
                    }

                    HLog.Debug(ConfigManager.EnableDebugMode.Value ? "Debug mode enabled." : "Debug mode disabled.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SwitchDebugPatch)}", ex);
                }
            }
        }
    }
}
