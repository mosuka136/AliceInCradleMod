using HarmonyLib;
using nel;
using nel.gm;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
         [HarmonyPatch]
         private class RemoveLimitInBenchMenu
         {
            static IEnumerable<MethodBase> TargetMethods()
            {
                var t = typeof(UiBenchMenu).GetNestedType("BenchCmd", BindingFlags.NonPublic);
                if (t == null)
                    yield break;

                var ctor = AccessTools.Constructor(t, new Type[]{
                    typeof(string),
                    typeof(Func<PR, bool>),
                    typeof(bool),
                    typeof(bool)});

                if (ctor != null)
                    yield return ctor;
            }

            static void Prefix(string _key, ref Func<PR, bool> _FnCanUse)
            {
                if (!ConfigManager.EnableRemoveLimitInBenchMenu.Value)
                    return;

                _FnCanUse = (pr) => true;
            }
        }
    }
}
