using nel;
using static BetterExperience.Patches.HPatches;
using static BetterExperience.Patches.HPatches.BetterReelEffectPatch;

namespace BetterExperience.Test.Patches
{
    public class LuckyBagEffectTests
    {
        // 029j2 Data/reel 中的福袋内容；有意保留重复槽位验证索引和原始数组不变。
        private static string[] Contents() => new[]
        {
            "COUNT_MUL2", "GRADE1", "GRADE1", "COUNT_ADD1", "ADD_MONEY100",
            "GRADE1", "GRADE2", "COUNT_ADD3", "GRADE1", "COUNT_ADD2",
            "GRADE3", "COUNT_ADD1", "GRADE1", "COUNT_ADD1", "GRADE2"
        };

        [Theory]
        [InlineData(LuckyBagEffect.CountAdd1, "COUNT_ADD1")]
        [InlineData(LuckyBagEffect.CountAdd2, "COUNT_ADD2")]
        [InlineData(LuckyBagEffect.CountAdd3, "COUNT_ADD3")]
        [InlineData(LuckyBagEffect.GradeAdd1, "GRADE1")]
        [InlineData(LuckyBagEffect.GradeAdd2, "GRADE2")]
        [InlineData(LuckyBagEffect.GradeAdd3, "GRADE3")]
        [InlineData(LuckyBagEffect.CountMultiply2, "COUNT_MUL2")]
        [InlineData(LuckyBagEffect.MoneyAdd100, "ADD_MONEY100")]
        public void SpecifiedEffect_WorksIndependentlyAndOverridesBestEffect(LuckyBagEffect selection, string expected)
        {
            foreach (bool betterEffect in new[] { false, true })
            {
                var content = Contents();
                var original = (string[])content.Clone();

                Assert.True(BetterReelEffectPatch.TrySelectEffect(content, 7, ReelExecuter.ETYPE.RANDOM,
                    selection, betterEffect, out int index, out bool missing));

                Assert.Equal(expected, content[index]);
                Assert.False(missing);
                Assert.Equal(original, content);
            }
        }

        [Theory]
        [InlineData(false, 3)]
        [InlineData(true, 7)]
        public void Default_PreservesExistingBestEffectSetting(bool betterEffect, int expectedIndex)
        {
            bool changed = BetterReelEffectPatch.TrySelectEffect(Contents(), 3, ReelExecuter.ETYPE.RANDOM,
                LuckyBagEffect.Default, betterEffect, out int index, out bool missing);

            Assert.Equal(betterEffect, changed);
            Assert.Equal(expectedIndex, index);
            Assert.False(missing);
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 2)]
        public void OrdinaryReel_OnlyUsesExistingBestEffectSetting(bool betterEffect, int expectedIndex)
        {
            var content = new[] { "GRADE0", "GRADE1", "GRADE2" };
            BetterReelEffectPatch.TrySelectEffect(content, 1, ReelExecuter.ETYPE.GRADE1,
                LuckyBagEffect.CountAdd1, betterEffect, out int index, out bool missing);

            Assert.Equal(expectedIndex, index);
            Assert.False(missing);
        }

        [Fact]
        public void MissingSpecifiedEffect_KeepsOriginalInsteadOfChoosingAnotherEffect()
        {
            var content = new[] { "GRADE1", "GRADE3" };
            Assert.False(BetterReelEffectPatch.TrySelectEffect(content, 0, ReelExecuter.ETYPE.RANDOM,
                LuckyBagEffect.CountAdd1, true, out int index, out bool missing));
            Assert.Equal(0, index);
            Assert.True(missing);
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData(new string[0], 0)]
        [InlineData(new[] { "GRADE1" }, -1)]
        [InlineData(new[] { "invalid" }, 0)]
        public void InvalidReelContent_IsNotModified(string[] content, int initialIndex)
        {
            Assert.False(BetterReelEffectPatch.TrySelectEffect(content, initialIndex, ReelExecuter.ETYPE.RANDOM,
                LuckyBagEffect.Default, true, out int index, out bool missing));
            Assert.Equal(initialIndex, index);
            Assert.False(missing);
        }

        [Fact]
        public void WrappedIndexAndNullSlot_PreserveSameCategorySelection()
        {
            var content = new[] { null, "GRADE1", "GRADE3", "COUNT_ADD3" };
            Assert.True(BetterReelEffectPatch.TrySelectEffect(content, 5, ReelExecuter.ETYPE.RANDOM,
                LuckyBagEffect.Default, true, out int index, out _));
            Assert.Equal(2, index);
        }

        [Fact]
        public void InvalidEnumAndMixedCase_FailSafelyOrMatchCaseInsensitively()
        {
            var content = new[] { "count_add1", "COUNT_ADD3" };
            Assert.True(BetterReelEffectPatch.TrySelectEffect(content, 1, ReelExecuter.ETYPE.RANDOM,
                LuckyBagEffect.CountAdd1, true, out int index, out _));
            Assert.Equal(0, index);
            Assert.False(BetterReelEffectPatch.TrySelectEffect(content, 0, ReelExecuter.ETYPE.RANDOM,
                (LuckyBagEffect)999, false, out index, out _));
            Assert.Equal(0, index);
        }
    }
}
