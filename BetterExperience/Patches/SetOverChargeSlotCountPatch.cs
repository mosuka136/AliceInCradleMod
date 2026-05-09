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
        public class SetOverChargeSlotCountPatch
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
                    if (ConfigManager.EnablePreloadOverChargeSlotCount.Value)
                    {
                        HLog.Debug("Applying preloaded overcharge slot count.");
                        SetOverChargeSlotCount();
                    }
                };

                ConfigManager.SetOverChargeSlotCount.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Overcharge slot count config changed: {e}");
                    SetOverChargeSlotCount();
                };

                _initialized = true;
                HLog.Debug("Overcharge slot count patch initialized.");
            }

            public static void SetOverChargeSlotCount()
            {
                try
                {
                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                    if (pr == null)
                    {
                        HLog.Notice("Player instance not found while applying overcharge slot count.");
                        return;
                    }

                    if (pr.Skill == null)
                    {
                        HLog.Notice("Player skill data not found while applying overcharge slot count.");
                        return;
                    }

                    var oc = Traverse.Create(pr.Skill).Field("OcSlots").GetValue<M2PrOverChargeSlot>();
                    if (oc == null)
                    {
                        HLog.Notice("Overcharge slot component not found while applying overcharge slot count.");
                        return;
                    }

                    HLog.Debug($"Refresh overcharge slots. TargetCount={ConfigManager.SetOverChargeSlotCount.Value}");

                    _isChanging = true;
                    _hasLoggedOverrideForCurrentApply = false;
                    oc.fineSlots(); // fineSlots方法会调用ItemStorage.getCount方法
                    _isChanging = false;
                }
                catch (Exception ex)
                {
                    _isChanging = false;
                    HLog.Error($"Unexpected error in {nameof(SetOverChargeSlotCount)}.", ex);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int)})]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                try
                {
                    if (NelItem.GetById("oc_slot") != Data)
                        return true;

                    if (!_isChanging || ConfigManager.SetOverChargeSlotCount.Value < 0)
                        return true;

                    if (!_hasLoggedOverrideForCurrentApply)
                    {
                        HLog.Debug($"{nameof(SetOverChargeSlotCountPatch)} applied.");
                        _hasLoggedOverrideForCurrentApply = true;
                    }

                    __result = ConfigManager.SetOverChargeSlotCount.Value;
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetOverChargeSlotCountPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
