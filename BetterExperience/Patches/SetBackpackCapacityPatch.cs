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
        public class SetBackpackCapacityPatch
        {
            private static bool _initialized = false;
            private static int _currentCapacity = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadBackpackCapacity.Value)
                    {
                        HLog.Debug($"Applying preloaded backpack capacity: {ConfigManager.SetBackpackCapacity.Value}");
                        SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                    }
                };

                GameSaveProtectionManager.OnSavingActivated += RecoverBackpackCapacity;
                GameSaveProtectionManager.OnSavingCompleted += () =>
                {
                    HLog.Debug($"Restoring backpack capacity after save: {_currentCapacity}");
                    SetBackpackCapacity(_currentCapacity);
                };

                ConfigManager.SetBackpackCapacity.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Backpack capacity config changed: {e}");
                    SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value);
                };

                _initialized = true;
                HLog.Debug("Backpack capacity patch initialized.");
            }

            public static void SetBackpackCapacity(int count)
            {
                try
                {
                    if (count <= 0)
                    {
                        HLog.Debug($"Ignored invalid backpack capacity: {count}");
                        return;
                    }

                    var imng = GetIMNG();
                    if (imng == null)
                    {
                        HLog.Notice("Item manager not found while applying backpack capacity.");
                        return;
                    }

                    var inventory = imng.getInventory();
                    if (inventory == null)
                    {
                        HLog.Notice("Inventory not found while applying backpack capacity.");
                        return;
                    }

                    inventory.row_max = count;
                    HLog.Debug($"{nameof(SetBackpackCapacity)} applied. New capacity: {count}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetBackpackCapacity)}.", ex);
                }
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
                try
                {
                    var imng = GetIMNG();
                    if (imng == null)
                    {
                        HLog.Notice("Item manager not found while recovering backpack capacity.");
                        return;
                    }

                    var inventory = imng.getInventory();
                    if (inventory == null)
                    {
                        HLog.Notice("Inventory not found while recovering backpack capacity.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_capacity");
                    if (item == null)
                    {
                        HLog.Notice("workbench_capacity item not found while recovering backpack capacity.");
                        return;
                    }

                    var count = imng.getInventoryPrecious().getCount(item);
                    count = Math.Max(count, 0);

                    _currentCapacity = inventory.row_max;
                    inventory.row_max = count + 12;
                    HLog.Debug($"Recovered backpack capacity for save operation. TemporaryCapacity={inventory.row_max}, CurrentCapacity={_currentCapacity}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RecoverBackpackCapacity)}.", ex);
                }
            }
        }
    }
}
