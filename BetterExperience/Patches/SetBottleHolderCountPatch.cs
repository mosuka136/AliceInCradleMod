using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using UnityEngine;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置空瓶收纳槽位数量。
        /// 和背包容量类似，保存前恢复到游戏贵重品记录对应的数量，保存后再恢复运行时配置值。
        /// </summary>
        [HarmonyPatch]
        public class SetBottleHolderCountPatch
        {
            private static bool _initialized = false;
            // 保存期间暂存运行时配置数量，保存完成后恢复。
            private static int _originalBottleHolderCount = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    _originalBottleHolderCount = -1;

                    if (ConfigManager.SetBottleHolderCount.Value1)
                    {
                        BLog.Debug($"Applying preloaded bottle holder count: {ConfigManager.SetBottleHolderCount.Value2}");
                        SetBottleHolderCount(ConfigManager.SetBottleHolderCount.Value2);
                    }
                };

                GameSaveProtectionManager.OnSavingActivated += RecoverBottleHolderCount;

                GameSaveProtectionManager.OnSavingCompleted += () =>
                {
                    BLog.Debug($"Restoring bottle holder count after save: {_originalBottleHolderCount}");
                    SetBottleHolderCount(_originalBottleHolderCount);
                };

                _initialized = true;
                BLog.Debug("Bottle holder count patch initialized.");
            }

            public static void SetBottleHolderCount(int count)
            {
                try
                {
                    if (count < 0)
                    {
                        BLog.Debug($"Ignored invalid bottle holder count: {count}");
                        return;
                    }

                    var inventory = GetInventory();
                    if (inventory == null)
                    {
                        BLog.Notice("Inventory not found while applying bottle holder count.");
                        return;
                    }

                    inventory.hide_bottle_max = count;
                    inventory.fineRows(true);
                    BLog.Debug($"{nameof(SetBottleHolderCount)} applied. New count: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetBottleHolderCount)}.", ex);
                }
            }

            public static void RecoverBottleHolderCount()
            {
                try
                {
                    var inventory = GetInventory();
                    if (inventory == null)
                    {
                        BLog.Notice("Inventory not found while recovering bottle holder count.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_bottle");
                    if (item == null)
                    {
                        BLog.Notice("workbench_bottle item not found while recovering bottle holder count.");
                        return;
                    }

                    var preciousInventory = GetPreciousInventory();
                    if (preciousInventory == null)
                    {
                        BLog.Notice("Precious inventory not found while recovering bottle holder count.");
                        return;
                    }

                    var count = preciousInventory.getCount(item);
                    count = Mathf.Max(count, 0);

                    _originalBottleHolderCount = inventory.hide_bottle_max;
                    inventory.hide_bottle_max = count;
                    BLog.Debug($"Recovered bottle holder count for save operation. TemporaryCount={count}, OriginalCount={_originalBottleHolderCount}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(RecoverBottleHolderCount)}.", ex);
                }
            }

            public static int GetBottleHolderCount()
            {
                var inventory = GetInventory();
                return inventory == null ? -1 : inventory.hide_bottle_max;
            }
        }
    }
}
