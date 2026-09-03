using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using UnityModBase.HClassAttribute;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置背包行数上限。
        /// 保存前会临时恢复为游戏可由贵重品推导出的容量，保存后再应用配置容量，避免把运行时扩容直接固化到存档。
        /// </summary>
        [HarmonyPatch]
        public class SetBackpackCapacityPatch
        {
            private static bool _initialized = false;
            // 保存前记录当前运行时容量，保存完成后用于恢复配置效果。
            private static int _currentCapacity = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    // 恢复值属于上一个存档的运行时状态，读档后必须丢弃，避免之后的保存完成回调把旧容量写入新存档。
                    _currentCapacity = -1;

                    if (ConfigManager.SetBackpackCapacity.Value1)
                    {
                        BLog.Debug($"Applying preloaded backpack capacity: {ConfigManager.SetBackpackCapacity.Value2}");
                        SetBackpackCapacity(ConfigManager.SetBackpackCapacity.Value2);
                    }
                };

                GameSaveProtectionManager.OnSavingActivated += RecoverBackpackCapacity;
                GameSaveProtectionManager.OnSavingCompleted += () =>
                {
                    BLog.Debug($"Restoring backpack capacity after save: {_currentCapacity}");
                    SetBackpackCapacity(_currentCapacity);
                };

                _initialized = true;
                BLog.Debug("Backpack capacity patch initialized.");
            }

            public static void SetBackpackCapacity(int count)
            {
                try
                {
                    if (count <= 0)
                    {
                        BLog.Debug($"Ignored invalid backpack capacity: {count}");
                        return;
                    }

                    var inventory = GetInventory();
                    if (inventory == null)
                    {
                        BLog.Notice("Inventory not found while applying backpack capacity.");
                        return;
                    }

                    inventory.row_max = count;
                    BLog.Debug($"{nameof(SetBackpackCapacity)} applied. New capacity: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetBackpackCapacity)}.", ex);
                }
            }

            /// <summary>
            /// 读取当前背包行数上限，供实时控制界面显示；背包未加载时返回 -1 占位。
            /// </summary>
            public static int GetBackpackCapacity()
            {
                var inventory = GetInventory();
                return inventory == null ? -1 : inventory.row_max;
            }

            public static void RecoverBackpackCapacity()
            {
                try
                {
                    var inventory = GetInventory();
                    if (inventory == null)
                    {
                        BLog.Notice("Inventory not found while recovering backpack capacity.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_capacity");
                    if (item == null)
                    {
                        BLog.Notice("workbench_capacity item not found while recovering backpack capacity.");
                        return;
                    }

                    var preciousInventory = GetPreciousInventory();
                    if (preciousInventory == null)
                    {
                        BLog.Notice("Precious inventory not found while recovering backpack capacity.");
                        return;
                    }

                    var count = preciousInventory.getCount(item);
                    count = Math.Max(count, 0);

                    _currentCapacity = inventory.row_max;
                    // 原游戏背包基础容量为 12，workbench_capacity 记录额外容量。
                    inventory.row_max = count + 12;
                    BLog.Debug($"Recovered backpack capacity for save operation. TemporaryCapacity={inventory.row_max}, CurrentCapacity={_currentCapacity}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(RecoverBackpackCapacity)}.", ex);
                }
            }
        }
    }
}
