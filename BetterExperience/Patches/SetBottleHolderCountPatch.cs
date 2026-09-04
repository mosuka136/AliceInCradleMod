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
        /// 新版空瓶槽位由主背包中的 workbench_bottle 数量生成。
        /// 修改物品数量后重建收纳行；序列化主背包时暂时去掉 Mod 增量，结束或异常后恢复。
        /// </summary>
        [HarmonyPatch]
        public class SetBottleHolderCountPatch
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
                    if (ConfigManager.SetBottleHolderCount.Value1)
                        SetBottleHolderCount(ConfigManager.SetBottleHolderCount.Value2);
                };

                _initialized = true;
                BLog.Debug("Bottle holder count patch initialized.");
            }

            public static void SetBottleHolderCount(int count)
            {
                try
                {
                    if (count < 0 || count > BottleHolderCountOverride.MaxCount)
                    {
                        BLog.Notice($"Ignored invalid bottle holder count: {count}");
                        return;
                    }

                    var inventory = GetInventory();
                    if (inventory?.getHid() == null)
                    {
                        BLog.Notice("Inventory not found while applying bottle holder count.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_bottle");
                    if (item == null)
                    {
                        BLog.Notice("workbench_bottle item not found while applying bottle holder count.");
                        return;
                    }

                    if (!ReferenceEquals(_inventory, inventory) || _countOverride == null)
                    {
                        _inventory = inventory;
                        _countOverride = new BottleHolderCountOverride(
                            () => inventory.getCount(item),
                            value => SetStoredBottleCount(inventory, item, value));
                    }

                    _countOverride.Apply(count);
                    BLog.Debug($"Bottle holder count applied. New count: {GetBottleHolderCount(inventory)}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetBottleHolderCount)}.", ex);
                }
            }

            internal static void SetStoredBottleCount(ItemStorage inventory, NelItem item, int count)
            {
                var current = inventory.getCount(item);
                if (current < count)
                    inventory.Add(item, count - current, 0);
                else if (current > count)
                {
                    // 被空瓶占用的收纳行不能直接 Reduce；先解除关联，瓶子仍保留在背包中。
                    inventory.removeWLink(item);
                    inventory.Reduce(item, current - count, -1, false);
                }

                // fineRows 会从实际物品数量重建 ItemHid 和空瓶关联，不能只写 hide_bottle_max 属性。
                inventory.fineRows(true);
                if (inventory.getCount(item) != count)
                    throw new InvalidOperationException($"Could not apply bottle holder count: {count}.");
            }

            /// <summary>背包或收纳组件尚未加载时返回 -1。</summary>
            public static int GetBottleHolderCount()
            {
                return GetBottleHolderCount(GetInventory());
            }

            internal static int GetBottleHolderCount(ItemStorage inventory)
            {
                return inventory?.getHid()?.hide_bottle_max ?? -1;
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
                    BLog.Error("Failed to restore bottle holder count after serialization.", ex);
                }
            }
        }
    }
}
