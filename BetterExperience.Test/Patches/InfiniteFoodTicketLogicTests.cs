using BetterExperience.Patches;
using System.Collections.Generic;

namespace BetterExperience.Test.Patches
{
    public class InfiniteFoodTicketLogicTests
    {
        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, true, false)]
        [InlineData(false, true, true, false)]
        public void ShouldRestore_RequiresAllConditions(
            bool featureEnabled,
            bool hadTicketsBeforeEat,
            bool eatSucceeded,
            bool expected)
        {
            // Act
            bool actual = InfiniteFoodTicketLogic.ShouldRestore(featureEnabled, hadTicketsBeforeEat, eatSucceeded);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RestoreDiscount_InsertsConsumedByteAtHead()
        {
            // Arrange
            var adiscount = new List<byte> { 30, 30, 10 };

            // Act
            InfiniteFoodTicketLogic.RestoreDiscount(adiscount, 50);

            // Assert
            Assert.Equal(new byte[] { 50, 30, 30, 10 }, adiscount.ToArray());
        }

        [Fact]
        public void RestoreDiscount_KeepsDescendingOrderWhenRestoringMax()
        {
            // Arrange
            var adiscount = new List<byte> { 30, 10 };

            // Act
            InfiniteFoodTicketLogic.RestoreDiscount(adiscount, 30);

            // Assert
            Assert.Equal(new byte[] { 30, 30, 10 }, adiscount.ToArray());
            Assert.True(IsDescending(adiscount));
        }

        [Fact]
        public void RestoreDiscount_ToleratesNullList()
        {
            // Act
            InfiniteFoodTicketLogic.RestoreDiscount(null, 50);
        }

        [Theory]
        [InlineData(true, 3, 3)]
        [InlineData(false, 3, 0)]
        [InlineData(true, 0, 0)]
        public void RestoreGrade_UsesTopGradeOnlyWhenInfoExists(bool hasTicketInfo, int topGrade, int expected)
        {
            // Act
            int actual = InfiniteFoodTicketLogic.RestoreGrade(hasTicketInfo, topGrade);

            // Assert
            Assert.Equal(expected, actual);
        }

        private static bool IsDescending(List<byte> values)
        {
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i - 1] < values[i])
                    return false;
            }

            return true;
        }
    }
}
