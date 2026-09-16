using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class ManaWeedRechargeLogicTests
    {
        [Theory]
        [InlineData(-1f, -1f)]
        [InlineData(-5f, -1f)]
        [InlineData(-0.5f, -1f)]
        [InlineData(0f, 0f)]
        [InlineData(0.5f, 30f)]
        [InlineData(1f, 60f)]
        [InlineData(30f, 1800f)]
        [InlineData(90f, 5400f)]
        public void ResolveRechargeFrames_ConvertsSecondsToFrames(float seconds, float expected)
        {
            // Act
            float actual = ManaWeedRechargeLogic.ResolveRechargeFrames(seconds);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(2400f, -1f, 2400f)]
        [InlineData(6000f, -1f, 6000f)]
        [InlineData(3000f, 1800f, 1800f)]
        [InlineData(6000f, 1800f, 1800f)]
        [InlineData(1200f, 1800f, 1200f)]
        [InlineData(1800f, 1800f, 1800f)]
        [InlineData(0f, 0f, 0f)]
        public void CapRechargeTime_OnlyLowersToCap(float currentFrames, float capFrames, float expected)
        {
            // Act
            float actual = ManaWeedRechargeLogic.CapRechargeTime(currentFrames, capFrames);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
