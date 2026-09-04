using BetterExperience.Patches;
using nel;
using System.Reflection;
using System.Security;
using UnityModBase.HControlSpace;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Test.Patches
{
    public class SetBottleHolderCountPatchTests
    {
        [Fact]
        public void Bind_BeforeInventoryLoads_InitializesBottleAndSubsequentControls()
        {
            using var controls = new ControlService();
            controls.CreateTable("Player", new Translator("Player", "Player"));

            var bottle = controls.Bind("Player", "Bottle",
                () => HPatches.SetBottleHolderCountPatch.GetBottleHolderCount(null),
                ControlUpdatePolicy.Never, new Translator("Bottle", "Bottle"));
            var next = controls.Bind("Player", "Next", () => 12,
                ControlUpdatePolicy.Never, new Translator("Next", "Next"));

            Assert.Equal(-1, bottle.Value);
            Assert.Equal(12, next.Value);
        }

        [Fact]
        public void GetBottleHolderCount_WithoutItemHid_ReturnsUnavailable()
        {
            var inventory = new ItemStorage("test", 12);

            Assert.Equal(-1, HPatches.SetBottleHolderCountPatch.GetBottleHolderCount(inventory));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        [InlineData(BottleHolderCountOverride.MaxCount)]
        public void GetBottleHolderCount_CurrentGame_ReadsItemHidProperty(int count)
        {
            var inventory = new ItemStorage("test", 12) { water_stockable = false };
            inventory.getHid().hide_bottle_max = count;

            Assert.Equal(count, HPatches.SetBottleHolderCountPatch.GetBottleHolderCount(inventory));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(12)]
        public void SetStoredBottleCount_CurrentGame_RebuildsCountBeforeUiRefresh(int target)
        {
            var inventory = new ItemStorage("test", 12) { water_stockable = false };
            var item = new NelItem("workbench_bottle", 0, 0, 1);
            var holders = (NelItem[])typeof(ItemHid).GetField("AHolderData", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var original = holders[0];
            var originalBottle = NelItem.Bottle;
            try
            {
                ItemHid.AssignItemEntry(item);
                NelItem.Bottle = new NelItem("empty_bottle", 0, 0, 1) { category = NelItem.CATEG.BOTTLE };
                RunUntilUiRefresh(() => HPatches.SetBottleHolderCountPatch.SetStoredBottleCount(inventory, item, 8));
                inventory.Add(NelItem.Bottle, 8, 0);
                Assert.Equal(8, inventory.getHid()[ItemHid.HOLDER.BOTTLE].use);
                RunUntilUiRefresh(() => HPatches.SetBottleHolderCountPatch.SetStoredBottleCount(inventory, item, target));
                RunUntilUiRefresh(() => inventory.fineRows(true));

                Assert.Equal(target, inventory.getCount(item));
                Assert.Equal(target, HPatches.SetBottleHolderCountPatch.GetBottleHolderCount(inventory));
                Assert.Equal(8, inventory.getCount(NelItem.Bottle));
            }
            finally
            {
                holders[0] = original;
                NelItem.Bottle = originalBottle;
            }
        }

        [Fact]
        public void SerializationAndReload_AffectOnlyTrackedInventory()
        {
            var owner = new ItemStorage("owner", 12);
            var other = new ItemStorage("other", 12);
            var count = 3;
            var state = new BottleHolderCountOverride(() => count, value => count = value);
            var patch = typeof(HPatches.SetBottleHolderCountPatch);
            var inventoryField = patch.GetField("_inventory", BindingFlags.Static | BindingFlags.NonPublic);
            var overrideField = patch.GetField("_countOverride", BindingFlags.Static | BindingFlags.NonPublic);
            var previousInventory = inventoryField.GetValue(null);
            var previousOverride = overrideField.GetValue(null);
            try
            {
                inventoryField.SetValue(null, owner);
                overrideField.SetValue(null, state);
                state.Apply(12);

                HPatches.SetBottleHolderCountPatch.SaveInventoryPrefix(other, out var unrelated);
                Assert.Null(unrelated);
                Assert.Equal(12, count);
                HPatches.SetBottleHolderCountPatch.ClearInventoryPrefix(other);
                HPatches.SetBottleHolderCountPatch.SaveInventoryPrefix(owner, out var saving);
                Assert.Same(state, saving);
                Assert.Equal(3, count);
                HPatches.SetBottleHolderCountPatch.SaveInventoryFinalizer(saving);
                Assert.Equal(12, count);

                HPatches.SetBottleHolderCountPatch.ClearInventoryPrefix(owner);
                HPatches.SetBottleHolderCountPatch.SaveInventoryPrefix(owner, out var afterReload);
                Assert.Null(afterReload);
                Assert.Equal(12, count);
            }
            finally
            {
                inventoryField.SetValue(null, previousInventory);
                overrideField.SetValue(null, previousOverride);
            }
        }

        private static void RunUntilUiRefresh(Action action)
        {
            try
            {
                action();
            }
            catch (SecurityException ex)
            {
                // net8 不能执行 Unity ECall。该 UI 方法位于计数和收纳行重建之后，
                // 只允许这个已知尾部调用失败；本测试不验证 Unity 界面刷新。
                Assert.Equal("fineRowButtonLink", ex.TargetSite?.Name);
            }
        }
    }
}
