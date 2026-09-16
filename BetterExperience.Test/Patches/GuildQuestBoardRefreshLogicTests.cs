using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class GuildQuestBoardRefreshLogicTests
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void ShowInGuildUi_HidesInDigestingView(bool digesting, bool expected)
        {
            // Act
            bool actual = GuildQuestBoardRefreshLogic.ShowInGuildUi(digesting);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        public void IsRemovable_OnlyUnacceptedUndestructedEntries(
            bool hasQuestTracker,
            bool destructed,
            bool expected)
        {
            // Act
            bool actual = GuildQuestBoardRefreshLogic.IsRemovable(hasQuestTracker, destructed);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
