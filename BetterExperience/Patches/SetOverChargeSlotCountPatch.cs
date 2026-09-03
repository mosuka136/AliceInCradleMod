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
        /// 设置过充插槽数量。
        /// 游戏刷新插槽时会读取贵重品数量，补丁只在刷新调用期间临时覆盖对应物品计数。
        /// </summary>
        [HarmonyPatch]
        public class SetOverChargeSlotCountPatch
        {
            private static bool _initialized = false;
            // 仅在主动调用 fineSlots 期间拦截 ItemStorage.getCount，避免影响其他物品计数。
            private static bool _isChanging = false;
            private static bool _hasLoggedOverrideForCurrentApply = false;
            private static int _targetCount = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetOverChargeSlotCount.Value1)
                    {
                        BLog.Debug("Applying preloaded overcharge slot count.");
                        SetOverChargeSlotCount(ConfigManager.SetOverChargeSlotCount.Value2);
                    }
                };

                _initialized = true;
                BLog.Debug("Overcharge slot count patch initialized.");
            }

            public static void SetOverChargeSlotCount(int count)
            {
                try
                {
                    if (count < 0)
                    {
                        BLog.Debug($"Ignored invalid overcharge slot count: {count}");
                        return;
                    }

                    var skill = GetPRSkillTraverse();
                    if (skill == null)
                    {
                        BLog.Notice("Player skill data not found while applying overcharge slot count.");
                        return;
                    }

                    var oc = skill.Field("OcSlots").GetValue<M2PrOverChargeSlot>();
                    if (oc == null)
                    {
                        BLog.Notice("Overcharge slot component not found while applying overcharge slot count.");
                        return;
                    }

                    BLog.Debug($"Refresh overcharge slots. TargetCount={count}");

                    _targetCount = count;
                    _isChanging = true;
                    _hasLoggedOverrideForCurrentApply = false;
                    // fineSlots 会读取 oc_slot 数量，下面的 Harmony Prefix 只在此窗口期返回配置值。
                    oc.fineSlots();
                    _isChanging = false;
                }
                catch (Exception ex)
                {
                    _isChanging = false;
                    BLog.Error($"Unexpected error in {nameof(SetOverChargeSlotCount)}.", ex);
                }
            }

            public static int GetOverChargeSlotCount()
            {
                var skill = GetPRSkillTraverse();
                if (skill == null)
                    return -1;

                var slots = skill.Field("OcSlots").GetValue<M2PrOverChargeSlot>();
                return slots == null
                    ? -1
                    : Traverse.Create(slots).Field("max_slot").GetValue<int>();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int)})]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                try
                {
                    if (NelItem.GetById("oc_slot") != Data)
                        return true;

                    if (!_isChanging || _targetCount < 0)
                        return true;

                    if (!_hasLoggedOverrideForCurrentApply)
                    {
                        BLog.Debug($"{nameof(SetOverChargeSlotCountPatch)} applied.");
                        _hasLoggedOverrideForCurrentApply = true;
                    }

                    __result = _targetCount;
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetOverChargeSlotCountPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
