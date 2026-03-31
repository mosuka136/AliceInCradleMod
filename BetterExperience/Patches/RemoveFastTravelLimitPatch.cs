using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using nel.gm;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveFastTravelLimitPatch
        {
            private static NelChipBench _cachedBenchChip;

            [HarmonyPatch]
            public class RemoveFastTravelMapLimitPatch
            {
                private static IEnumerable<MethodBase> TargetMethods()
                {
                    var type = AccessTools.TypeByName("nel.gm.UiGMCMap");
                    if (type == null)
                    {
                        Debug.LogError("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var getter = AccessTools.PropertyGetter(type, "can_use_fasttravel");
                    if (getter == null)
                    {
                        Debug.LogError("Failed to find getter: can_use_fasttravel");
                        yield break;
                    }

                    yield return getter;
                }

                [HarmonyPostfix]
                public static void Postfix(object __instance, ref bool __result)
                {
                    if (!ConfigManager.EnableFastTravelAnywhere.Value)
                        return;

                    __result = true;
                }
            }

            [HarmonyPatch]
            public class RemoveFastTravelBenchLimitPatch
            {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    var type = AccessTools.TypeByName("nel.gm.UiGMCMap");
                    if (type == null)
                    {
                        Debug.LogError("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var method = AccessTools.Method(type, "executeFastTravelConfirm");
                    if (method == null)
                    {
                        Debug.LogError("Failed to find method: executeFastTravelConfirm");
                        yield break;
                    }

                    yield return method;
                }

                [HarmonyPrefix]
                public static void Prefix(object __instance, ref bool __result)
                {
                    if (!ConfigManager.EnableFastTravelAnywhere.Value)
                        return;

                    var gm = Traverse.Create(__instance).Field("GM").GetValue<UiGameMenu>();
                    if (gm == null)
                        return;

                    gm.BenchChip = gm.BenchChip ?? _cachedBenchChip;
                }
            }

            [HarmonyPatch]
            public class GetBenchChipPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(PR), nameof(PR.getNearBench))]
                public static void Postfix(ref NelChipBench __result)
                {
                    if (!ConfigManager.EnableFastTravelAnywhere.Value)
                        return;

                    if (__result != null)
                        _cachedBenchChip = __result;
                }
            }
        }
    }
}
