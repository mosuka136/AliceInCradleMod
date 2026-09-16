using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class StoreRefreshButtonLogicTests
    {
        private static readonly string[] NativeTitles =
            { "&&cmd_buy", "&&cmd_sell", "&&cmd_cart", "&&cmd_checkout" };

        [Fact]
        public void ExpandTitles_AppendsRefreshKeyToNativeLayout()
        {
            // Act
            var expanded = StoreRefreshButtonLogic.ExpandTitles(NativeTitles, "be_store_refresh");

            // Assert
            Assert.Equal(new[] { "&&cmd_buy", "&&cmd_sell", "&&cmd_cart", "&&cmd_checkout", "be_store_refresh" }, expanded);
        }

        [Fact]
        public void ExpandTitles_DoesNotMutateOriginalArray()
        {
            // Act
            StoreRefreshButtonLogic.ExpandTitles(NativeTitles, "be_store_refresh");

            // Assert
            Assert.Equal(new[] { "&&cmd_buy", "&&cmd_sell", "&&cmd_cart", "&&cmd_checkout" }, NativeTitles);
        }

        [Fact]
        public void ExpandTitles_ReturnsSameInstanceOnNull()
        {
            // Act
            var expanded = StoreRefreshButtonLogic.ExpandTitles(null, "be_store_refresh");

            // Assert
            Assert.Null(expanded);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(5)]
        public void ExpandTitles_ReturnsSameInstanceOnUnexpectedLength(int length)
        {
            // Arrange
            var titles = Enumerable.Range(0, length).Select(i => "&&cmd_" + i).ToArray();

            // Act
            var expanded = StoreRefreshButtonLogic.ExpandTitles(titles, "be_store_refresh");

            // Assert
            Assert.Same(titles, expanded);
        }

        [Fact]
        public void ExpandTitles_ReturnsSameInstanceWhenCheckoutIsMissing()
        {
            // Arrange
            var titles = new[] { "&&cmd_buy", "&&cmd_sell", "&&cmd_cart", "&&cmd_other" };

            // Act
            var expanded = StoreRefreshButtonLogic.ExpandTitles(titles, "be_store_refresh");

            // Assert
            Assert.Same(titles, expanded);
        }
    }
}
