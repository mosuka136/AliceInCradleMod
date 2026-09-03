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
            // 本次刷新要应用的插槽数量，由调用方传入：同一数值有“读档预加载”和“实时控制”两个来源，不能再固定读取某个配置项。
            private static int _targetCount = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetEnhancerSlotCount.Value1)
                    {
                        BLog.Debug("Applying preloaded enhancer slot count.");
                        SetEnhancerSlotCount(ConfigManager.SetEnhancerSlotCount.Value2);
                    }
                };

                _initialized = true;
                BLog.Debug("Enhancer slot count patch initialized.");
            }

            public static void SetEnhancerSlotCount(int count)
            {
                try
                {
                    if (count < 0)
                    {
                        BLog.Debug($"Ignored invalid enhancer slot count: {count}");
                        return;
                    }

                    var imng = GetIMNG();
                    if (imng == null)
                    {
                        BLog.Notice("Item manager not found while applying enhancer slot count.");
                        return;
                    }

                    var sp = GetPreciousInventory();
                    if (sp == null)
                    {
                        BLog.Notice("Precious storage not found while applying enhancer slot count.");
                        return;
                    }

                    var se = Traverse.Create(imng).Field("StEnhancer").GetValue<ItemStorage>();
                    if (se == null)
                    {
                        BLog.Notice("Enhancer storage not found while applying enhancer slot count.");
                        return;
                    }

                    BLog.Debug($"Refresh enhancer slots. TargetCount={count}");
                    _targetCount = count;
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

            /// <summary>
            /// 读取当前强化插槽数量，供实时控制界面显示；物品管理器未加载时返回 -1 占位。
            /// max_slot 是 ENHA 的静态字段，保存最近一次 fineEnhancerStorage 计算出的插槽数。
            /// </summary>
            public static int GetEnhancerSlotCount()
            {
                if (GetIMNG() == null)
                    return -1;

                return Traverse.Create(typeof(ENHA)).Field("max_slot").GetValue<int>();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int) })]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                try
                {
                    if (NelItem.GetById("enhancer_slot") != Data)
                        return true;

                    if (!_isChanging || _targetCount < 0)
                        return true;

                    if (!_hasLoggedOverrideForCurrentApply)
                    {
                        BLog.Debug($"Enhancer slot count overridden to {_targetCount}");
                        _hasLoggedOverrideForCurrentApply = true;
                    }

                    __result = _targetCount;
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
