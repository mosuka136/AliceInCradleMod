using BetterExperience.Patches;
using nel;

namespace BetterExperience.Test.Patches
{
    public class WanderingNpcForceLogicTests
    {
        [Theory]
        [InlineData(SummonWanderingNpcKind.Nightingale, WanderingManager.TYPE.NIG)]
        [InlineData(SummonWanderingNpcKind.CoffeeMaker, WanderingManager.TYPE.COF)]
        [InlineData(SummonWanderingNpcKind.Tilde, WanderingManager.TYPE.TLD)]
        [InlineData(SummonWanderingNpcKind.Puppet, WanderingManager.TYPE.PUP)]
        public void ToGameType_MapsEachKind(SummonWanderingNpcKind kind, WanderingManager.TYPE expected)
        {
            // Act
            WanderingManager.TYPE actual = WanderingNpcForceLogic.ToGameType(kind);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToGameType_OutOfRangeFallsBackToNightingale()
        {
            // Arrange
            var outOfRange = (SummonWanderingNpcKind)99;

            // Act
            WanderingManager.TYPE actual = WanderingNpcForceLogic.ToGameType(outOfRange);

            // Assert
            Assert.Equal(WanderingManager.TYPE.NIG, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldSummon_FollowsWorldReadiness(bool worldReady)
        {
            // Act
            bool actual = WanderingNpcForceLogic.ShouldSummon(worldReady);

            // Assert
            Assert.Equal(worldReady, actual);
        }
    }
}
