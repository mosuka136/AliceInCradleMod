using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetBackpackCapacityPatch
        {
            private static bool _initialised = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialised)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadBackpackCapacity.Value)
                        SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                OnSiteProtectionManager.Instance.OnSiteProtectionActivated += RecoverBackpackCapacity;
                OnSiteProtectionManager.Instance.OnSiteProtectionCompleted += () =>
                {
                    SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                ConfigManager.SetBackpackCapacity.SettingChanged += (s, e) =>
                {
                    SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                _initialised = true;
            }

            public static void SetBackpackCapacity(int count)
            {
                if (count <= 0)
                    return;

                var imng = GetIMNG();
                if (imng == null)
                    return;

                var inventory = Traverse.Create(imng).Field("StInventory").GetValue<ItemStorage>();
                if (inventory == null)
                    return;

                inventory.row_max = count;
            }

            private static NelItemManager GetIMNG()
            {
                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return null;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return null;

                if (m2d.IMNG == null)
                    return null;

                return m2d.IMNG;
            }

            private static void RecoverBackpackCapacity()
            {
                var imng = GetIMNG();
                if (imng == null)
                    return;

                var inventory = Traverse.Create(imng).Field("StInventory").GetValue<ItemStorage>();
                if (inventory == null)
                    return;

                var item = NelItem.GetById("workbench_capacity");
                if (item == null)
                    return;

                var count = imng.getInventoryPrecious().getCount(item);
                count = Math.Max(count, 0);
                inventory.row_max = count + 12;
            }
        }
    }
}
