using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetEnhancerSlotCountPatch
        {
            private static bool _initialized = false;
            private static bool _isChanging = false;
            private static bool _hasLoggedOverrideForCurrentApply = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadEnhancerSlotCount.Value)
                    {
                        HLog.Debug("Applying preloaded enhancer slot count.");
                        SetEnhancerSlotCount();
                    }
                };

                ConfigManager.SetEnhancerSlotCount.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Enhancer slot count config changed: {e}");
                    SetEnhancerSlotCount();
                };

                _initialized = true;
                HLog.Debug("Enhancer slot count patch initialized.");
            }

            public static void SetEnhancerSlotCount()
            {
                try
                {
                    var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                    if (sg == null)
                    {
                        HLog.Notice("SceneGame not found while applying enhancer slot count.");
                        return;
                    }

                    var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                    if (m2d == null)
                    {
                        HLog.Notice("NelM2DBase not found while applying enhancer slot count.");
                        return;
                    }

                    if (m2d.IMNG == null)
                    {
                        HLog.Notice("Item manager not found while applying enhancer slot count.");
                        return;
                    }

                    var sp = Traverse.Create(m2d.IMNG).Field("StPrecious").GetValue<ItemStorage>();
                    if (sp == null)
                    {
                        HLog.Notice("Precious storage not found while applying enhancer slot count.");
                        return;
                    }

                    var se = Traverse.Create(m2d.IMNG).Field("StEnhancer").GetValue<ItemStorage>();
                    if (se == null)
                    {
                        HLog.Notice("Enhancer storage not found while applying enhancer slot count.");
                        return;
                    }

                    HLog.Debug($"Refresh enhancer slots. TargetCount={ConfigManager.SetEnhancerSlotCount.Value}");
                    _isChanging = true;
                    _hasLoggedOverrideForCurrentApply = false;
                    // ENHA.fineEnhancerStorage方法会调用ItemStorage.getCount方法
                    ENHA.fineEnhancerStorage(sp, se);
                    _isChanging = false;
                }
                catch (Exception ex)
                {
                    _isChanging = false;
                    HLog.Error($"Unexpected error in {nameof(SetEnhancerSlotCount)}.", ex);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int) })]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                try
                {
                    if (NelItem.GetById("enhancer_slot") != Data)
                        return true;

                    if (!_isChanging || ConfigManager.SetEnhancerSlotCount.Value < 0)
                        return true;

                    if (!_hasLoggedOverrideForCurrentApply)
                    {
                        HLog.Debug($"Enhancer slot count overridden to {ConfigManager.SetEnhancerSlotCount.Value}");
                        _hasLoggedOverrideForCurrentApply = true;
                    }

                    __result = ConfigManager.SetEnhancerSlotCount.Value;
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetEnhancerSlotCountPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
