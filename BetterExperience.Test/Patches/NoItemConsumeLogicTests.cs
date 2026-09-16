using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class NoItemConsumeLogicTests
    {
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(20, 20)]
        [InlineData(21, 20)]
        public void ClampThreshold_LimitsToZeroThroughTwenty(int threshold, int expected)
        {
            // Act
            int actual = NoItemConsumeLogic.ClampThreshold(threshold);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new[] { 9, 9, 3, 0, 0 }, 9, 2)]
        [InlineData(new[] { 9, 8, 0, 0, 0 }, 9, 1)]
        [InlineData(new[] { 10, 10, 10, 10, 10 }, 9, 5)]
        [InlineData(new[] { 0, 0, 0, 0, 0 }, 9, 0)]
        [InlineData(new[] { 9, 9, 9, 9, 9 }, 0, 0)]
        public void CountFullRows_CountsRowsAtOrAboveStockable(int[] countsPerGrade, int stockable, int expected)
        {
            // Act
            int actual = NoItemConsumeLogic.CountFullRows(countsPerGrade, stockable);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountFullRows_ToleratesNullCounts()
        {
            // Act
            int actual = NoItemConsumeLogic.CountFullRows(null, 9);

            // Assert
            Assert.Equal(0, actual);
        }

        [Theory]
        [InlineData(2, 25, 2, true)]
        [InlineData(1, 25, 2, false)]
        [InlineData(2, 25, 3, false)]
        [InlineData(0, 1, 0, true)]
        [InlineData(0, 0, 0, false)]
        [InlineData(5, 5, -1, false)]
        public void ShouldKeep_WhenFullRowsReachThreshold(
            int fullRows,
            int totalCount,
            int threshold,
            bool expected)
        {
            // Act
            bool actual = NoItemConsumeLogic.ShouldKeep(fullRows, totalCount, threshold);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IconEffectConstants_AreSane()
        {
            // Assert
            Assert.InRange(NoItemConsumeLogic.IconEffectPeriodFrames, 1, 600);
            Assert.InRange(
                NoItemConsumeLogic.IconEffectOverlayAlpha,
                0f,
                1f);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HueFromKey_EmptyKeyIsRedHue(string key)
        {
            // Act
            float hue = NoItemConsumeLogic.HueFromKey(key);

            // Assert
            Assert.Equal(0f, hue);
        }

        [Fact]
        public void HueFromKey_IsStablePerKey()
        {
            // Act
            float first = NoItemConsumeLogic.HueFromKey("potion_hp");
            float second = NoItemConsumeLogic.HueFromKey("potion_hp");

            // Assert
            Assert.Equal(first, second);
            Assert.InRange(first, 0f, 1f);
        }

        [Fact]
        public void HueFromKey_DistinctKeysUsuallyGetDistinctHues()
        {
            // Act
            float a = NoItemConsumeLogic.HueFromKey("potion_hp");
            float b = NoItemConsumeLogic.HueFromKey("potion_mp");

            // Assert
            Assert.NotEqual(a, b);
        }

        [Theory]
        [InlineData(0f, 1f, 0f, 0f)]
        [InlineData(1f / 6f, 1f, 1f, 0f)]
        [InlineData(2f / 6f, 0f, 1f, 0f)]
        [InlineData(3f / 6f, 0f, 1f, 1f)]
        [InlineData(4f / 6f, 0f, 0f, 1f)]
        [InlineData(5f / 6f, 1f, 0f, 1f)]
        [InlineData(1f, 1f, 0f, 0f)]
        public void HueToRgb_MapsPrimaryAndSecondaryHues(
            float hue,
            float expectedR,
            float expectedG,
            float expectedB)
        {
            // Act
            NoItemConsumeLogic.HueToRgb(hue, out float r, out float g, out float b);

            // Assert
            Assert.Equal(expectedR, r, 5);
            Assert.Equal(expectedG, g, 5);
            Assert.Equal(expectedB, b, 5);
        }

        [Theory]
        [InlineData(-1f / 6f, 1f, 0f, 1f)]
        [InlineData(7f / 6f, 1f, 1f, 0f)]
        public void HueToRgb_WrapsOutOfRangeHues(
            float hue,
            float expectedR,
            float expectedG,
            float expectedB)
        {
            // Act
            NoItemConsumeLogic.HueToRgb(hue, out float r, out float g, out float b);

            // Assert
            Assert.Equal(expectedR, r, 5);
            Assert.Equal(expectedG, g, 5);
            Assert.Equal(expectedB, b, 5);
        }

        [Fact]
        public void HueToRgb_NanHueFallsBackToWhite()
        {
            // Act
            NoItemConsumeLogic.HueToRgb(float.NaN, out float r, out float g, out float b);

            // Assert
            Assert.Equal(1f, r);
            Assert.Equal(1f, g);
            Assert.Equal(1f, b);
        }

        [Fact]
        public void HueToRgb_InterpolatesWithinSector()
        {
            // Act
            NoItemConsumeLogic.HueToRgb(0.5f / 6f, out float r, out float g, out float b);

            // Assert
            Assert.Equal(1f, r);
            Assert.Equal(0.5f, g, 5);
            Assert.Equal(0f, b);
        }
    }
}
