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
         [HarmonyPatch]
         public class RemoveLimitInBenchMenuPatch
         {
            static IEnumerable<MethodBase> TargetMethods()
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

            static void Prefix(string _key, ref Func<PR, bool> _FnCanUse, bool _can_set_auto, ref bool _only_in_safearea)
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
        }

        [HarmonyPatch]
        public class RemoveLimitInBenchMenuButtonPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiBenchMenu), "setEnableBtns")]
            public static void SetEnableBtnsPostfix(UiBenchMenu __instance)
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
        }
    }
}
