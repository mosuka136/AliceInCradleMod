using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using UnityEngine;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetBottleHolderCountPatch
        {
            private static bool _initialised = false;
            private static int _originalBottleHolderCount = -1;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialised)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadBottleHolderCount.Value)
                        SetBottleHolderCount(ConfigManager.SetBottleHolderCount.Value);
                };

                OnSiteProtectionManager.Instance.OnSiteProtectionActivated += RecoverBottleHolderCount;

                OnSiteProtectionManager.Instance.OnSiteProtectionCompleted += () =>
                {
                    SetBottleHolderCount(_originalBottleHolderCount);
                };

                ConfigManager.SetBottleHolderCount.SettingChanged += (s, e) =>
                {
                    SetBottleHolderCount(ConfigManager.SetBottleHolderCount.Value);
                };

                _initialised = true;
            }

            public static void SetBottleHolderCount(int count)
            {
                if (count < 0)
                    return;

                var inventory = GetIMNG()?.getInventory();
                if (inventory == null)
                    return;

                inventory.hide_bottle_max = count;
                inventory.fineRows(true);
            }

            private static void RecoverBottleHolderCount()
            {
                var imng = GetIMNG();
                if (imng == null)
                    return;

                var inventory = imng.getInventory();
                if (inventory == null)
                    return;

                var item = NelItem.GetById("workbench_bottle");
                if (item == null)
                    return;

                var count = imng.getInventoryPrecious().getCount(item);
                count = Mathf.Max(count, 0);

                _originalBottleHolderCount = inventory.hide_bottle_max;
                inventory.hide_bottle_max = count;
            }

            private static NelItemManager GetIMNG()
            {
                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return null;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return null;

                return m2d.IMNG;
            }
        }
    }
}
