using BetterExperience.BConfigManager;
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadOverChargeSlotCount.Value)
                        SetOverChargeSlotCount();
                };

                ConfigManager.SetOverChargeSlotCount.OnValueChanged += (s, e) =>
                {
                    SetOverChargeSlotCount();
                };

                _initialized = true;
            }

            public static void SetOverChargeSlotCount()
            {
                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                if(pr.Skill == null)
                    return;

                var oc = Traverse.Create(pr.Skill).Field("OcSlots").GetValue<M2PrOverChargeSlot>();
                if (oc == null)
                    return;

                _isChanging = true;
                oc.fineSlots(); // fineSlots方法会调用ItemStorage.getCount方法
                _isChanging = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int)})]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                if (NelItem.GetById("oc_slot") != Data)
                    return true;

                if (!_isChanging || ConfigManager.SetOverChargeSlotCount.Value < 0)
                    return true;

                __result = ConfigManager.SetOverChargeSlotCount.Value;
                return false;
            }
        }
    }
}
