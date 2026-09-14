using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using m2d;
using nel;
using nel.gm;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityModBase.HClassAttribute;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 放宽快速传送的地图、长椅以及夜间/雷暴限制。
        /// 游戏仍需要至少缓存过一次长椅对象，补丁会复用最近一次创建的 BenchChip。
        /// 剧情锁定的传送不放行。
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
                        BLog.Error("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var getter = AccessTools.PropertyGetter(type, "can_use_fasttravel");
                    if (getter == null)
                    {
                        BLog.Error("Failed to find getter: can_use_fasttravel");
                        yield break;
                    }

                    yield return getter;
                }

                [HarmonyPostfix]
                public static void Postfix(object __instance, ref bool __result)
                {
                    try
                    {
                        __result = RemoveFastTravelNightThunderPatch.ComputeLiveFastTravelAllowed(__instance);
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(RemoveFastTravelMapLimitPatch)}", ex);
                    }
                }
            }

            [HarmonyPatch]
            public class SyncMapFastTravelAppearPatch
            {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    var type = AccessTools.TypeByName("nel.gm.UiGMCMap");
                    if (type == null)
                    {
                        BLog.Error("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var method = AccessTools.Method(type, "initAppearMain");
                    if (method == null)
                    {
                        BLog.Error("Failed to find method: initAppearMain");
                        yield break;
                    }

                    yield return method;
                }

                [HarmonyPostfix]
                public static void Postfix(object __instance)
                {
                    try
                    {
                        bool allowed = RemoveFastTravelNightThunderPatch.ComputeLiveFastTravelAllowed(__instance);
                        RemoveFastTravelNightThunderPatch.SyncMapFastTravelState(__instance, allowed);
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(SyncMapFastTravelAppearPatch)}", ex);
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
                        BLog.Error("Failed to find type: nel.gm.UiGMCMap");
                        yield break;
                    }

                    var method = AccessTools.Method(type, "executeFastTravelConfirm");
                    if (method == null)
                    {
                        BLog.Error("Failed to find method: executeFastTravelConfirm");
                        yield break;
                    }

                    yield return method;
                }

                [HarmonyPrefix]
                public static bool Prefix(object __instance, ref bool __result)
                {
                    try
                    {
                        if (ConfigManager.EnableFastTravelAnywhere?.Value == true)
                        {
                            var gm = Traverse.Create(__instance).Field("GM").GetValue<UiGameMenu>();
                            if (gm == null)
                                BLog.Notice("UiGameMenu not found while removing fast travel bench restriction.");
                            else
                            {
                                gm.BenchChip = gm.BenchChip ?? _cachedBenchChip;
                                BLog.Debug($"{nameof(RemoveFastTravelBenchLimitPatch)} applied.");
                            }
                        }

                        if (RemoveFastTravelNightThunderPatch.IsLiveFastTravelBlocked())
                        {
                            __result = false;
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(RemoveFastTravelBenchLimitPatch)}", ex);
                    }

                    return true;
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
                        BLog.Debug($"Cached bench chip updated: {__instance.GetType().FullName}");
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(GetBenchChipPatch)}", ex);
                    }
                }
            }

            /// <summary>
            /// 原版 <see cref="NelM2DBase.cantFastTravel"/> 在夜间且战斗计数大于 0、或雷暴时返回
            /// <c>Alert_cannot_fast_travel</c>。随时快传或夜间椅子传送开启时去掉该提示。
            /// </summary>
            [HarmonyPatch]
            public class RemoveFastTravelNightThunderPatch
            {
                private static bool _initialized;

                [InitializeOnGameBoot]
                public static void Initialize()
                {
                    if (_initialized)
                        return;

                    if (ConfigManager.EnableAllowNightTravel != null)
                        ConfigManager.EnableAllowNightTravel.OnValueChanged += (_, __) => RefreshFastTravelUi();
                    if (ConfigManager.EnableFastTravelAnywhere != null)
                        ConfigManager.EnableFastTravelAnywhere.OnValueChanged += (_, __) => RefreshFastTravelUi();
                    GameSaveLoadManager.OnGameSaveLoadCompleted += RefreshFastTravelUi;

                    _initialized = true;
                    BLog.Debug($"{nameof(RemoveFastTravelNightThunderPatch)} initialized.");
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(NelM2DBase), nameof(NelM2DBase.cantFastTravel))]
                public static void Postfix(ref string __result)
                {
                    try
                    {
                        string next = WorldStateLock.ApplyNightTravelAssist(
                            __result,
                            IsAssistOn(),
                            CurrentVanillaNightThunderBlocks());
                        if (next != __result)
                            BLog.Debug($"{nameof(RemoveFastTravelNightThunderPatch)} adjusted cantFastTravel: {__result} -> {next}.");
                        __result = next;
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(RemoveFastTravelNightThunderPatch)}", ex);
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(SCN), nameof(SCN.isBenchCmdEnable))]
                public static void BenchCmdEnablePostfix(string key, ref bool __result)
                {
                    try
                    {
                        if (key != "fast_travel")
                            return;

                        var m2d = GetM2D();
                        if (m2d == null)
                            return;

                        if (m2d.cantFastTravel() != null)
                        {
                            __result = false;
                            return;
                        }

                        if (IsAssistOn())
                            __result = true;
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(BenchCmdEnablePostfix)}", ex);
                    }
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(UiGameMenu), nameof(UiGameMenu.activate))]
                public static void ActivatePostfix()
                {
                    RefreshFastTravelUi();
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(UiWmSkinController), nameof(UiWmSkinController.runEdit))]
                public static void RunEditPrefix(UiWmSkinController __instance)
                {
                    try
                    {
                        bool allowed = ComputeLiveFastTravelAllowed();
                        __instance.can_use_fasttravel = allowed;
                        if (!allowed)
                            SetSkinFastTravelActive(__instance, false);
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(RunEditPrefix)}", ex);
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(UiBenchMenu), nameof(UiBenchMenu.ExecuteFastTravel))]
                public static bool ExecuteFastTravelPrefix(ref bool __result)
                {
                    try
                    {
                        if (!IsLiveFastTravelBlocked())
                            return true;

                        __result = false;
                        UILog.Instance?.AddAlertTX(WorldStateLock.NightThunderAlert);
                        BLog.Debug($"{nameof(ExecuteFastTravelPrefix)} blocked travel after night-travel assist was turned off.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(ExecuteFastTravelPrefix)}", ex);
                        return true;
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(WholeMapItem), nameof(WholeMapItem.free_travel_analyzed), MethodType.Setter)]
                public static bool FreeTravelAnalyzedSetterPrefix(bool value)
                {
                    try
                    {
                        if (WorldStateLock.ShouldPersistFreeTravelAnalyzed(
                            value,
                            IsAssistOn(),
                            CurrentVanillaNightThunderBlocks(),
                            CurrentMapAlreadyUnlocked()))
                            return true;

                        BLog.Debug($"{nameof(FreeTravelAnalyzedSetterPrefix)} skipped persisting free-travel unlock.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(FreeTravelAnalyzedSetterPrefix)}", ex);
                        return true;
                    }
                }

                [HarmonyPrefix]
                [HarmonyPatch(typeof(UiWarpConfirm), nameof(UiWarpConfirm.checkUseConfirm))]
                public static void CheckUseConfirmPrefix(WholeMapItem NextWM, out bool __state)
                {
                    __state = NextWM != null && NextWM.free_travel_analyzed;
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(UiWarpConfirm), nameof(UiWarpConfirm.checkUseConfirm))]
                public static void CheckUseConfirmPostfix(WholeMapItem NextWM, bool __state)
                {
                    try
                    {
                        if (NextWM == null || __state)
                            return;
                        if (WorldStateLock.ShouldPersistFreeTravelAnalyzed(
                            true,
                            IsAssistOn(),
                            CurrentVanillaNightThunderBlocks(),
                            CurrentMapAlreadyUnlocked()))
                            return;

                        NextWM.free_travel_analyzed = false;
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(CheckUseConfirmPostfix)}", ex);
                    }
                }

                internal static bool IsLiveFastTravelBlocked()
                {
                    var m2d = GetM2D();
                    return m2d != null && m2d.cantFastTravel() != null;
                }

                internal static bool ComputeLiveFastTravelAllowed(object map = null)
                {
                    var gm = map != null
                        ? Traverse.Create(map).Field("GM").GetValue<UiGameMenu>()
                        : GetM2D()?.GM;
                    if (gm == null)
                        return false;

                    bool onBench = Traverse.Create(gm).Field("pr_on_bench").GetValue<bool>();
                    return WorldStateLock.ComputeMenuFastTravelAllowed(
                        onBench,
                        ConfigManager.EnableFastTravelAnywhere?.Value == true,
                        SCN.isBenchCmdEnable("fast_travel"),
                        Map2d.can_handle);
                }

                internal static void RefreshFastTravelUi()
                {
                    try
                    {
                        var gm = GetM2D()?.GM;
                        if (gm == null)
                            return;

                        UiBenchMenu.fineBenchCmdEnable();

                        var gmTraverse = Traverse.Create(gm);
                        bool allowed = ComputeLiveFastTravelAllowed();
                        gmTraverse.Field("can_use_fasttravel").SetValue(allowed);

                        var appear = gmTraverse.Field("AppearC").GetValue();
                        SyncMapFastTravelState(appear, allowed);

                        var bench = gmTraverse.Field("BenchMenu").GetValue();
                        if (bench != null)
                            Traverse.Create(bench).Method("setEnableBtns").GetValue();
                    }
                    catch (Exception ex)
                    {
                        BLog.Error($"Unexpected error in {nameof(RefreshFastTravelUi)}", ex);
                    }
                }

                internal static void SyncMapFastTravelState(object map, bool allowed)
                {
                    if (map == null)
                        return;

                    var appearTraverse = Traverse.Create(map);
                    var wmCtr = appearTraverse.Field("WmCtr").GetValue<UiWmSkinController>();
                    if (wmCtr != null)
                    {
                        wmCtr.can_use_fasttravel = allowed;
                        SetSkinFastTravelActive(wmCtr, allowed);
                    }
                    else
                    {
                        var wmSkin = appearTraverse.Field("WmSkin").GetValue();
                        if (wmSkin != null)
                            Traverse.Create(wmSkin).Property("fast_travel_active").SetValue(allowed);
                    }
                }

                private static void SetSkinFastTravelActive(UiWmSkinController controller, bool allowed)
                {
                    var wmSkin = Traverse.Create(controller).Field("WmSkin").GetValue();
                    if (wmSkin == null)
                        return;

                    if (!allowed)
                        Traverse.Create(wmSkin).Property("fast_travel_active").SetValue(false);
                    else
                        Traverse.Create(wmSkin).Property("fast_travel_active").SetValue(true);
                }

                private static bool IsAssistOn()
                {
                    return WorldStateLock.AllowsNightThunderTravel(
                        ConfigManager.EnableFastTravelAnywhere?.Value == true,
                        ConfigManager.EnableAllowNightTravel?.Value == true);
                }

                private static bool CurrentVanillaNightThunderBlocks()
                {
                    var night = GetNightController();
                    if (night == null)
                        return false;

                    return WorldStateLock.VanillaNightOrThunderBlocksTravel(
                        night.isNight(),
                        night.getBattleCount(),
                        night.hasWeather(WeatherItem.WEATHER.THUNDER));
                }

                private static bool CurrentMapAlreadyUnlocked()
                {
                    var cur = GetM2D()?.WM?.CurWM;
                    return cur != null && cur.free_travel_enable && cur.free_travel_analyzed;
                }
            }
        }
    }
}
