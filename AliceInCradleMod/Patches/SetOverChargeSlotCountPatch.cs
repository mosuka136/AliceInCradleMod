using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetOverChargeSlotCountPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                ConfigManager.SetOverChargeSlotCount.Value = -1;

                ConfigManager.SetOverChargeSlotCount.SettingChanged += (s, e) =>
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

                oc.fineSlots();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int)})]
            private static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                if (NelItem.GetById("oc_slot") != Data)
                    return true;

                if (ConfigManager.SetOverChargeSlotCount.Value < 0)
                    return true;

                __result = ConfigManager.SetOverChargeSlotCount.Value;
                return false;
            }
        }
    }
}
