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
        /// 手雷收纳槽由主背包中的 workbench_holder_bomb 数量生成，一个收纳器提供一个槽位。
        /// 修改物品数量后重建收纳行；序列化主背包时暂时去掉 Mod 增量，结束或异常后恢复。
        /// 计数覆盖状态机与空瓶收纳共用 <see cref="BottleHolderCountOverride"/>。
        /// </summary>
        [HarmonyPatch]
        public class SetBombHolderSlotCountPatch
        {
            private static bool _initialized;
            private static ItemStorage _inventory;
            private static BottleHolderCountOverride _countOverride;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    ClearOverride();
                    if (ConfigManager.SetBombHolderSlotCount.Value1)
                        SetBombHolderSlotCount(ConfigManager.SetBombHolderSlotCount.Value2);
                };

                _initialized = true;
                BLog.Debug("Bomb holder slot count patch initialized.");
            }

            public static void SetBombHolderSlotCount(int count)
            {
                try
                {
                    if (count < 0 || count > BottleHolderCountOverride.MaxCount)
                    {
                        BLog.Notice($"Ignored invalid bomb holder slot count: {count}");
                        return;
                    }

                    var inventory = GetInventory();
                    if (inventory?.getHid() == null)
                    {
                        BLog.Notice("Inventory not found while applying bomb holder slot count.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_holder_bomb");
                    if (item == null)
                    {
                        BLog.Notice("workbench_holder_bomb item not found while applying bomb holder slot count.");
                        return;
                    }

                    if (!ReferenceEquals(_inventory, inventory) || _countOverride == null)
                    {
                        _inventory = inventory;
                        _countOverride = new BottleHolderCountOverride(
                            () => inventory.getCount(item),
                            value => SetStoredBombHolderCount(inventory, item, value));
                    }

                    _countOverride.Apply(count);
                    BLog.Debug($"Bomb holder slot count applied. New count: {GetBombHolderSlotCount(inventory)}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetBombHolderSlotCount)}.", ex);
                }
            }

            internal static void SetStoredBombHolderCount(ItemStorage inventory, NelItem item, int count)
            {
                var current = inventory.getCount(item);
                if (current < count)
                    inventory.Add(item, count - current, 0);
                else if (current > count)
                {
                    // 被收纳手雷占用的行（WL_OUTER）不可直接扣减；先逐行解除关联，手雷仍保留在背包中。
                    // ver030g 删除了 removeWLink(NelItem) 重载，改为按物品收集行后逐行 removeWLink(IRow)。
                    using (var linkedRows = inventory.PopGetItemRowsFor(item))
                    {
                        foreach (var row in linkedRows)
                            inventory.removeWLink(row);
                    }
                    inventory.Reduce(item, current - count, -1, false);
                }

                // fineRows 会从实际物品数量重建 ItemHid 和收纳关联，不能只改数量。
                inventory.fineRows(true);
                if (inventory.getCount(item) != count)
                    throw new InvalidOperationException($"Could not apply bomb holder slot count: {count}.");
            }

            /// <summary>背包或收纳组件尚未加载时返回 -1。</summary>
            public static int GetBombHolderSlotCount()
            {
                return GetBombHolderSlotCount(GetInventory());
            }

            internal static int GetBombHolderSlotCount(ItemStorage inventory)
            {
                if (inventory == null)
                    return -1;
                var item = NelItem.GetById("workbench_holder_bomb");
                return item == null ? -1 : inventory.getCount(item);
            }

            // ItemStorage 在读档时会复用；必须在读取新数据前丢弃旧存档的原始数量。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.clearAllItems))]
            public static void ClearInventoryPrefix(ItemStorage __instance)
            {
                if (ReferenceEquals(_inventory, __instance))
                    ClearOverride();
            }

            private static void ClearOverride()
            {
                _inventory = null;
                _countOverride = null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.writeBinaryTo))]
            internal static void SaveInventoryPrefix(ItemStorage __instance, out BottleHolderCountOverride __state)
            {
                __state = ReferenceEquals(_inventory, __instance) ? _countOverride : null;
                // 还原失败时让保存中止，避免将临时数量写入存档；Finalizer 仍会尝试恢复运行时数量。
                __state?.SuspendForSave();
            }

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.writeBinaryTo))]
            internal static void SaveInventoryFinalizer(BottleHolderCountOverride __state)
            {
                try
                {
                    __state?.ResumeAfterSave();
                }
                catch (Exception ex)
                {
                    BLog.Error("Failed to restore bomb holder slot count after serialization.", ex);
                }
            }
        }
    }
}
