using BetterExperience.Patches;
using System;

namespace BetterExperience.Test.Patches
{
    public class StorePriceLogicTests
    {
        [Theory]
        [InlineData(-1f, 0f)]
        [InlineData(0f, 0f)]
        [InlineData(0.5f, 0.5f)]
        [InlineData(1f, 1f)]
        [InlineData(2f, 2f)]
        [InlineData(2.5f, 2f)]
        public void ClampRatio_LimitsToZeroThroughMax(float ratio, float expected)
        {
            // Act
            float actual = StorePriceLogic.ClampRatio(ratio, StorePriceLogic.BuyRatioMax);

            // Assert
            Assert.Equal(expected, actual, 5);
        }

        [Theory]
        [InlineData(-3f, 0f)]
        [InlineData(4.9f, 4.9f)]
        [InlineData(5f, 5f)]
        [InlineData(9f, 5f)]
        public void ClampRatio_RespectsProvidedMax(float ratio, float expected)
        {
            // Act
            float actual = StorePriceLogic.ClampRatio(ratio, StorePriceLogic.SellRatioMax);

            // Assert
            Assert.Equal(expected, actual, 5);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void ClampRatio_FallsBackToOneOnNonFinite(float ratio)
        {
            // Act
            float actual = StorePriceLogic.ClampRatio(ratio, StorePriceLogic.BuyRatioMax);

            // Assert
            Assert.Equal(StorePriceLogic.RatioDefault, actual, 5);
        }

        [Theory]
        [InlineData(100, 1f, 100)]
        [InlineData(100, 0f, 0)]
        [InlineData(100, 0.5f, 50)]
        [InlineData(99, 0.5f, 49)]
        [InlineData(100, 0.1f, 10)]
        [InlineData(7, 0.5f, 3)]
        public void ApplyRatio_TruncatesLikeVanilla(int price, float ratio, int expected)
        {
            // Act
            int actual = StorePriceLogic.ApplyRatio(price, ratio);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, 2f)]
        [InlineData(-5, 3f)]
        public void ApplyRatio_NonPositivePriceStaysZero(int price, float ratio)
        {
            // Act
            int actual = StorePriceLogic.ApplyRatio(price, ratio);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public void ApplyRatio_DoesNotOverflowToIntMax()
        {
            // Act
            int actual = StorePriceLogic.ApplyRatio(int.MaxValue, StorePriceLogic.SellRatioMax);

            // Assert
            Assert.Equal(int.MaxValue, actual);
        }
    }
}
