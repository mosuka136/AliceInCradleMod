using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class SetJumpHeightPatchTests
    {
        [Theory]
        [InlineData(1f, 1f, 1f)]
        [InlineData(0.5f, 2f, 1f)]
        [InlineData(1f, 0.1f, 0.1f)]
        [InlineData(1f, 5f, 5f)]
        [InlineData(0f, 3f, 0f)]
        public void Multiplier_PreservesStatusModifiers(float original, float multiplier, float expected)
        {
            Assert.Equal(expected, SetJumpHeightPatch.ApplyMultiplier(original, multiplier));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        [InlineData(0.09f)]
        [InlineData(5.01f)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void InvalidConfig_PreservesOriginal(float multiplier)
        {
            Assert.Equal(0.75f, SetJumpHeightPatch.ApplyMultiplier(0.75f, multiplier));
        }

        [Fact]
        public void Overflow_DoesNotIntroduceInvalidPhysicsValues()
        {
            Assert.Equal(float.MaxValue, SetJumpHeightPatch.ApplyMultiplier(float.MaxValue, 5f));
        }
    }
}
