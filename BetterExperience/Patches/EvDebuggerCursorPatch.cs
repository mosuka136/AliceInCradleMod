using BetterExperience.BLogSpace;
using evt;
using HarmonyLib;
using System;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 事件调试控制台（F7）激活时只注册了 IN.FlgUiUse，未向 CURS 注册光标，
        /// 软光标会被 CURS.fineMouse2 每帧隐藏，控制台内看不到鼠标。
        /// 激活时压入光标需求与默认贴图（与 M2FreezeCamera / MuseumViewer 的用法一致），
        /// 关闭或事件系统销毁时成对移除；按钮悬停自带的光标不受影响。
        /// </summary>
        [HarmonyPatch]
        public class EvDebuggerCursorPatch
        {
            private const string CursorKey = "EVDEBUGGER";
            private const string CursorCategory = "NORMAL";
            private const string CursorKind = "tl_move";

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EvDebugger), nameof(EvDebugger.changeActivate))]
            public static void SyncCursorOnActivate(EvDebugger __instance)
            {
                try
                {
                    if (__instance.isActive())
                    {
                        CURS.Active.Add(CursorKey);
                        CURS.Set(CursorCategory, CursorKind);
                    }
                    else
                    {
                        RemCursor();
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error("Unexpected error while syncing debugger cursor requirement.", ex);
                }
            }

            // 控制台开启状态下切图或读档时调试器会被直接销毁，这里清理光标避免鼠标常驻。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(EV), nameof(EV.destructItems))]
            public static void RemCursorOnDestruct()
            {
                try
                {
                    RemCursor();
                }
                catch (Exception ex)
                {
                    BLog.Error("Unexpected error while removing debugger cursor requirement.", ex);
                }
            }

            private static void RemCursor()
            {
                CURS.Rem(CursorCategory, CursorKind);
                CURS.Active.Rem(CursorKey);
            }
        }
    }
}
