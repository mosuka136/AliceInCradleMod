using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class SetReelObtainableRatioLogicTests
    {
        [Theory]
        [InlineData(3, -1f, 3)]
        [InlineData(3, -0.5f, 3)]
        [InlineData(0, 2f, 0)]
        [InlineData(3, 1f, 3)]
        [InlineData(3, 2f, 6)]
        [InlineData(3, 1.5f, 5)]
        [InlineData(3, 0.5f, 2)]
        [InlineData(3, 0f, 0)]
        [InlineData(30, 10f, 255)]
        [InlineData(255, 2f, 255)]
        public void ApplyRatio_ScalesAndClampsToByteRange(int count, float ratio, int expected)
        {
            // Act
            int actual = SetReelObtainableRatioLogic.ApplyRatio(count, ratio);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
