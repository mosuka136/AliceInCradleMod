using BetterExperience.BConfigManager;
using HarmonyLib;
using m2d;
using nel;
using nel.gm;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 放宽快速传送的地图和长椅限制。
        /// 游戏仍需要至少缓存过一次长椅对象，补丁会复用最近一次创建的 BenchChip。
        /// </summary>
        [HarmonyPatch]
        public class RemoveFastTravelLimitPatch
        {
            // 远离长椅打开地图时游戏菜单仍需要一个 BenchChip，本字段保存最近一次正常获取到的对象。
            private static NelChipBench _cachedBenchChip;

            [HarmonyPatch]
            public class RemoveFastTravelMapLimitPatch
            {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    var type = AccessTools.TypeByName("nel.gm.UiGMCMap");
                    if (type == null)
                    {
                        HLog.Error("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var getter = AccessTools.PropertyGetter(type, "can_use_fasttravel");
                    if (getter == null)
                    {
                        HLog.Error("Failed to find getter: can_use_fasttravel");
                        yield break;
                    }

                    yield return getter;
                }

                [HarmonyPostfix]
                public static void Postfix(object __instance, ref bool __result)
                {
                    try
                    {
                        if (!ConfigManager.EnableFastTravelAnywhere.Value)
                            return;

                        __result = true;

                        HLog.Debug($"{nameof(RemoveFastTravelMapLimitPatch)} applied.");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(RemoveFastTravelMapLimitPatch)}", ex);
                    }
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
                        HLog.Error("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var method = AccessTools.Method(type, "executeFastTravelConfirm");
                    if (method == null)
                    {
                        HLog.Error("Failed to find method: executeFastTravelConfirm");
                        yield break;
                    }

                    yield return method;
                }

                [HarmonyPrefix]
                public static void Prefix(object __instance, ref bool __result)
                {
                    try
                    {
                        if (!ConfigManager.EnableFastTravelAnywhere.Value)
                            return;

                        var gm = Traverse.Create(__instance).Field("GM").GetValue<UiGameMenu>();
                        if (gm == null)
                        {
                            HLog.Notice("UiGameMenu not found while removing fast travel bench restriction.");
                            return;
                        }

                        gm.BenchChip = gm.BenchChip ?? _cachedBenchChip;

                        HLog.Debug($"{nameof(RemoveFastTravelBenchLimitPatch)} applied.");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(RemoveFastTravelBenchLimitPatch)}", ex);
                    }
                }
            }

            [HarmonyPatch]
            public class GetBenchChipPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(NelChipBench), MethodType.Constructor)]
                [HarmonyPatch(new Type[] { typeof(M2MapLayer), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(M2ChipImage) })]
                public static void Postfix(NelChipBench __instance)
                {
                    try
                    {
                        _cachedBenchChip = __instance;
                        HLog.Debug($"Cached bench chip updated: {__instance.GetType().FullName}");
                    }
                    catch (Exception ex)
                    {
                        HLog.Error($"Unexpected error in {nameof(GetBenchChipPatch)}", ex);
                    }
                }
            }
        }
    }
}
