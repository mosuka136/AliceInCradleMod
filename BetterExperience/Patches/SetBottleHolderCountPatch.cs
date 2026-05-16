using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
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
                    if (ConfigManager.EnablePreloadBottleHolderCount.Value)
                    {
                        HLog.Debug($"Applying preloaded bottle holder count: {ConfigManager.SetBottleHolderCount.Value}");
                        SetBottleHolderCount(ConfigManager.SetBottleHolderCount.Value);
                    }
                };

                GameSaveProtectionManager.OnSavingActivated += RecoverBottleHolderCount;

                GameSaveProtectionManager.OnSavingCompleted += () =>
                {
                    HLog.Debug($"Restoring bottle holder count after save: {_originalBottleHolderCount}");
                    SetBottleHolderCount(_originalBottleHolderCount);
                };

                ConfigManager.SetBottleHolderCount.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Bottle holder count config changed: {e}");
                    SetBottleHolderCount(e);
                };

                _initialized = true;
                HLog.Debug("Bottle holder count patch initialized.");
            }

            public static void SetBottleHolderCount(int count)
            {
                try
                {
                    if (count < 0)
                    {
                        HLog.Debug($"Ignored invalid bottle holder count: {count}");
                        return;
                    }

                    var inventory = GetIMNG()?.getInventory();
                    if (inventory == null)
                    {
                        HLog.Notice("Inventory not found while applying bottle holder count.");
                        return;
                    }

                    inventory.hide_bottle_max = count;
                    inventory.fineRows(true);
                    HLog.Debug($"{nameof(SetBottleHolderCount)} applied. New count: {count}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetBottleHolderCount)}.", ex);
                }
            }

            public static void RecoverBottleHolderCount()
            {
                try
                {
                    var imng = GetIMNG();
                    if (imng == null)
                    {
                        HLog.Notice("Item manager not found while recovering bottle holder count.");
                        return;
                    }

                    var inventory = imng.getInventory();
                    if (inventory == null)
                    {
                        HLog.Notice("Inventory not found while recovering bottle holder count.");
                        return;
                    }

                    var item = NelItem.GetById("workbench_bottle");
                    if (item == null)
                    {
                        HLog.Notice("workbench_bottle item not found while recovering bottle holder count.");
                        return;
                    }

                    var count = imng.getInventoryPrecious().getCount(item);
                    count = Mathf.Max(count, 0);

                    _originalBottleHolderCount = inventory.hide_bottle_max;
                    inventory.hide_bottle_max = count;
                    HLog.Debug($"Recovered bottle holder count for save operation. TemporaryCount={count}, OriginalCount={_originalBottleHolderCount}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RecoverBottleHolderCount)}.", ex);
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

                return m2d.IMNG;
            }
        }
    }
}
