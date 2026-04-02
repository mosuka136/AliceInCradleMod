using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetBackpackCapacityPatch
        {
            private static bool _initialized = false;
            private static int _currentCapacity = -1;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadBackpackCapacity.Value)
                        SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                OnSiteProtectionManager.Instance.OnSiteProtectionActivated += RecoverBackpackCapacity;
                OnSiteProtectionManager.Instance.OnSiteProtectionCompleted += () =>
                {
                    SetBackpackCapacity(_currentCapacity);
                };

                ConfigManager.SetBackpackCapacity.OnValueChanged += (s, e) =>
                {
                    SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                _initialized = true;
            }

            public static void SetBackpackCapacity(int count)
            {
                if (count <= 0)
                    return;

                var imng = GetIMNG();
                if (imng == null)
                    return;

                var inventory = imng.getInventory();
                if (inventory == null)
                    return;

                inventory.row_max = count;
            }

            public static NelItemManager GetIMNG()
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

            public static void RecoverBackpackCapacity()
            {
                var imng = GetIMNG();
                if (imng == null)
                    return;

                var inventory = imng.getInventory();
                if (inventory == null)
                    return;

                var item = NelItem.GetById("workbench_capacity");
                if (item == null)
                    return;

                var count = imng.getInventoryPrecious().getCount(item);
                count = Math.Max(count, 0);

                _currentCapacity = inventory.row_max;
                inventory.row_max = count + 12;
            }
        }
    }
}
