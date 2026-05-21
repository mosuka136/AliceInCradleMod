using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using nel.gm;
using System;
using System.Collections.Generic;
using System.Reflection;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
         /// <summary>
         /// 放宽长椅菜单命令的安全区域和可用性限制。
         /// 排尿选项保留原限制，因为该选项会影响正常坐下椅子的流程。
         /// </summary>
         [HarmonyPatch]
         public class RemoveLimitInBenchMenuPatch
         {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                var t = typeof(UiBenchMenu).GetNestedType("BenchCmd", BindingFlags.NonPublic);
                if (t == null)
                {
                    HLog.Error("Failed to find nested type: UiBenchMenu.BenchCmd");
                    yield break;
                }

                var ctor = AccessTools.Constructor(t, new Type[]{
                    typeof(string),
                    typeof(Func<PR, bool>),
                    typeof(bool),
                    typeof(bool)});

                if (ctor != null)
                    yield return ctor;
                else
                    HLog.Error("Failed to find constructor for UiBenchMenu.BenchCmd");
            }

            public static void Prefix(string _key, ref Func<PR, bool> _FnCanUse, bool _can_set_auto, ref bool _only_in_safearea)
            {
                try
                {
                    if (!ConfigManager.EnableRemoveLimitInBenchMenu.Value)
                        return;

                    if (_key == "pee")
                    {
                        HLog.Debug("Skip modifying the pee option.");
                        return;
                    }

                    _only_in_safearea = false;
                    _FnCanUse = (pr) => true;

                    HLog.Debug($"{nameof(RemoveLimitInBenchMenuPatch)} applied. Key={_key}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInBenchMenuPatch)}", ex);
                }
            }
        }
        /// <summary>
        /// 刷新长椠菜单按钮状态，解除已经被 UI 锁定的按钮。
        /// </summary>
        [HarmonyPatch]
        public class RemoveLimitInBenchMenuButtonPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiBenchMenu), "setEnableBtns")]
            public static void SetEnableBtnsPostfix(UiBenchMenu __instance)
            {
                try
                {
                    if (!ConfigManager.EnableRemoveLimitInBenchMenu.Value)
                        return;

                    var buttons = Traverse.Create(__instance).Field("Btns").GetValue<BtnContainerRadio<aBtn>>();
                    if (buttons == null)
                    {
                        HLog.Notice("Bench menu buttons not found while removing bench menu restrictions.");
                        return;
                    }

                    var num = buttons.Length - 1;
                    for (var i = 0; i < num; i++)
                    {
                        var btn = buttons.Get(i);
                        var cmd = Traverse.Create<UiBenchMenu>().Method("GetCmd", btn.title).GetValue();
                        if (cmd == null)
                            continue;

                        Traverse.Create(cmd).Field("currennt_useable").SetValue(true);

                        btn.SetLocked(false, no_change_binding: true);
                    }

                    HLog.Debug($"{nameof(RemoveLimitInBenchMenuButtonPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveLimitInBenchMenuButtonPatch)}", ex);
                }
            }
        }
    }
}
