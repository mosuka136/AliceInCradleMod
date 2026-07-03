using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置强化插槽数量。
        /// 游戏刷新插槽时会读取贵重品数量，补丁只在刷新调用期间临时覆盖对应物品计数。
        /// </summary>
        [HarmonyPatch]
        public class SetEnhancerSlotCountPatch
        {
            private static bool _initialized = false;
            // 仅在主动调用 fineEnhancerStorage 期间拦截 ItemStorage.getCount，避免影响其他物品计数。
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
                        BLog.Debug("Applying preloaded enhancer slot count.");
                        SetEnhancerSlotCount();
                    }
                };

                ConfigManager.SetEnhancerSlotCount.OnValueChanged += (s, e) =>
                {
                    BLog.Debug($"Enhancer slot count config changed: {e}");
                    SetEnhancerSlotCount();
                };

                _initialized = true;
                BLog.Debug("Enhancer slot count patch initialized.");
            }

            public static void SetEnhancerSlotCount()
            {
                try
                {
                    var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                    if (sg == null)
                    {
                        BLog.Notice("SceneGame not found while applying enhancer slot count.");
                        return;
                    }

                    var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                    if (m2d == null)
                    {
                        BLog.Notice("NelM2DBase not found while applying enhancer slot count.");
                        return;
                    }

                    if (m2d.IMNG == null)
                    {
                        BLog.Notice("Item manager not found while applying enhancer slot count.");
                        return;
                    }

                    var sp = Traverse.Create(m2d.IMNG).Field("StPrecious").GetValue<ItemStorage>();
                    if (sp == null)
                    {
                        BLog.Notice("Precious storage not found while applying enhancer slot count.");
                        return;
                    }

                    var se = Traverse.Create(m2d.IMNG).Field("StEnhancer").GetValue<ItemStorage>();
                    if (se == null)
                    {
                        BLog.Notice("Enhancer storage not found while applying enhancer slot count.");
                        return;
                    }

                    BLog.Debug($"Refresh enhancer slots. TargetCount={ConfigManager.SetEnhancerSlotCount.Value}");
                    _isChanging = true;
                    _hasLoggedOverrideForCurrentApply = false;
                    // fineEnhancerStorage 会读取 enhancer_slot 数量，下面的 Harmony 前缀只在此窗口期返回配置值。
                    ENHA.fineEnhancerStorage(sp, se);
                    _isChanging = false;
                }
                catch (Exception ex)
                {
                    _isChanging = false;
                    BLog.Error($"Unexpected error in {nameof(SetEnhancerSlotCount)}.", ex);
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
                        BLog.Debug($"Enhancer slot count overridden to {ConfigManager.SetEnhancerSlotCount.Value}");
                        _hasLoggedOverrideForCurrentApply = true;
                    }

                    __result = ConfigManager.SetEnhancerSlotCount.Value;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetEnhancerSlotCountPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
