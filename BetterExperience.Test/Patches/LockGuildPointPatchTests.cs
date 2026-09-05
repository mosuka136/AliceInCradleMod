using BetterExperience.Patches;
using HarmonyLib;
using nel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static BetterExperience.Patches.HPatches.LockGuildPointPatch;

namespace BetterExperience.Test.Patches
{
    public class LockGuildPointPatchTests
    {
        private static GuildManager CreateGuild(int point)
        {
            var guild = (GuildManager)RuntimeHelpers.GetUninitializedObject(typeof(GuildManager));
            guild.gq_point = point;
            return guild;
        }

        [Theory]
        [InlineData(true, 100, 50)]
        [InlineData(true, 0, 50)]
        [InlineData(false, 100, 100)]
        [InlineData(false, 0, 0)]
        public void AnimatedWrite_OnlyChangesPointsWhenUnlocked(bool locked, int requested, int expected)
        {
            var guild = CreateGuild(50);
            WriteAnimatedPoint(guild, requested, locked);
            Assert.Equal(expected, guild.gq_point);
        }

        [Fact]
        public void Lock_DoesNotPreventManualSettingOrCarryPointsAcrossGuildInstances()
        {
            var first = CreateGuild(50);
            var second = CreateGuild(20);
            Assert.Equal(HPatches.SetGuildPointPatch.ApplyResult.Applied,
                HPatches.SetGuildPointPatch.Apply(first, 115, () => false));
            WriteAnimatedPoint(first, 150, true);
            WriteAnimatedPoint(second, 30, true);
            Assert.Equal(115, first.gq_point);
            Assert.Equal(20, second.gq_point);
            WriteAnimatedPoint(first, 100, false);
            Assert.Equal(100, first.gq_point);
        }

        [Fact]
        public void Lock_UninitializedOrMissingGuild_DoesNotFreezeInitialization()
        {
            var guild = CreateGuild(-1);
            WriteAnimatedPoint(guild, 0, true);
            Assert.Equal(0, guild.gq_point);
            WriteAnimatedPoint(null, 100, true);
            SynchronizeShopBalance(null, null, true);
        }

        [Theory]
        [InlineData(true, 50, 50)]
        [InlineData(false, 50, 30)]
        [InlineData(true, -1, 30)]
        public void Checkout_RestoresTemporaryBalanceOnlyForInitializedLockedGuild(bool locked, int points, uint expected)
        {
            var entry = new CoinEntry(CoinStorage.CTYPE._TEMPORARY, "guild_rankstar");
            entry.Set(30);
            SynchronizeShopBalance(CreateGuild(points), entry, locked);
            Assert.Equal(expected, entry.Get());
        }

        [Fact]
        public void Animation_LockingMidway_ClearsPendingDeltaAndUsesActualPoints()
        {
            var block = (FillBlockGQPointBox)RuntimeHelpers.GetUninitializedObject(typeof(FillBlockGQPointBox));
            block.stack_obtained = 10;
            block.stack_obtaining = 20;
            float initialPoint = 30;

            SynchronizeAnimation(block, CreateGuild(50), ref initialPoint, true);

            Assert.Equal(50, initialPoint);
            Assert.Equal(50, block.getCurPoint());
            Assert.Equal(0, block.stack_obtained);
            Assert.Equal(0, block.stack_obtaining);
        }

        [Fact]
        public void Animation_OrdinaryExperienceOrUnlockedGuild_IsUnchanged()
        {
            foreach (var block in new[]
            {
                (FillImageBlockExperience)RuntimeHelpers.GetUninitializedObject(typeof(FillImageBlockExperience)),
                (FillImageBlockExperience)RuntimeHelpers.GetUninitializedObject(typeof(FillBlockGQPointBox))
            })
            {
                block.stack_obtaining = 20;
                float initialPoint = 30;
                SynchronizeAnimation(block, CreateGuild(50), ref initialPoint, !(block is FillBlockGQPointBox));
                Assert.Equal(30, initialPoint);
                Assert.Equal(20, block.stack_obtaining);
            }
        }

        [Fact]
        public void Transpiler_CurrentGame_ReplacesOnlyPointWritesInBothAnimationMethods()
        {
            var field = typeof(GuildManager).GetField(nameof(GuildManager.gq_point));
            var setter = typeof(HPatches.LockGuildPointPatch).GetMethod(nameof(WriteAnimatedPoint), BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(GuildManager), typeof(int) }, null);
            foreach (var method in PointWritePatch.TargetMethods())
            {
                var original = CookingRandomEffectPatchTests.ReadInstructions(method);
                var patched = PointWritePatch.Transpiler(original).ToList();
                Assert.Equal(original.Count, patched.Count);
                Assert.Single(original.Where(instruction => instruction.opcode == OpCodes.Stfld && Equals(instruction.operand, field)));
                Assert.DoesNotContain(patched, instruction => instruction.opcode == OpCodes.Stfld && Equals(instruction.operand, field));
                Assert.Single(patched.Where(instruction => instruction.opcode == OpCodes.Call && Equals(instruction.operand, setter)));
                for (int index = 0; index < original.Count; index++)
                {
                    Assert.Equal(original[index].labels, patched[index].labels);
                    Assert.Equal(original[index].blocks, patched[index].blocks);
                    if (original[index].opcode == OpCodes.Stfld && Equals(original[index].operand, field))
                        continue;
                    Assert.Equal(original[index].opcode, patched[index].opcode);
                    Assert.Equal(original[index].operand, patched[index].operand);
                }
            }
        }

        [Fact]
        public void Transpiler_MissingWrite_RejectsIncompatibleGameMethod()
        {
            Assert.Throws<InvalidOperationException>(() => PointWritePatch.Transpiler(new[] { new CodeInstruction(OpCodes.Ret) }));
        }
    }
}
