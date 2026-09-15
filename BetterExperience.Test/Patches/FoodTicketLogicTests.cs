using BetterExperience.Patches;
using System.Collections.Generic;

namespace BetterExperience.Test.Patches
{
    public class FoodTicketLogicTests
    {
        [Theory]
        [InlineData(-5, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(50, 50)]
        [InlineData(99, 99)]
        [InlineData(100, 99)]
        public void ClampCount_LimitsToZeroThroughNinetyNine(int count, int expected)
        {
            // Act
            int actual = FoodTicketLogic.ClampCount(count);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NewTicketDiscount_UsesMaxOfExisting()
        {
            // Arrange
            var adiscount = new List<byte> { 40, 50, 20, 50 };

            // Act
            byte actual = FoodTicketLogic.NewTicketDiscount(adiscount);

            // Assert
            Assert.Equal((byte)50, actual);
        }

        [Fact]
        public void NewTicketDiscount_FallsBackToDefaultWhenEmpty()
        {
            // Act
            byte actual = FoodTicketLogic.NewTicketDiscount(new List<byte>());

            // Assert
            Assert.Equal(FoodTicketLogic.DefaultDiscount, actual);
        }

        [Fact]
        public void NewTicketDiscount_ToleratesNullList()
        {
            // Act
            byte actual = FoodTicketLogic.NewTicketDiscount(null);

            // Assert
            Assert.Equal(FoodTicketLogic.DefaultDiscount, actual);
        }

        [Theory]
        [InlineData(0, 5, 5, 0)]
        [InlineData(3, 8, 5, 0)]
        [InlineData(5, 5, 0, 0)]
        [InlineData(9, 4, 0, 5)]
        [InlineData(4, 0, 0, 4)]
        public void ComputeDelta_ProducesAddOrRemove_Exclusively(
            int current,
            int target,
            int expectedAdd,
            int expectedRemove)
        {
            // Act
            FoodTicketLogic.ComputeDelta(current, target, out int add, out int remove);

            // Assert
            Assert.Equal(expectedAdd, add);
            Assert.Equal(expectedRemove, remove);
        }
    }
}
