using BetterExperience.Patches;
using System.Collections.Generic;

namespace BetterExperience.Test.Patches
{
    public class WanderingNpcPresenceLogicTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void ShouldShowOnMap_RequiresFeatureAndStoryUnlock(
            bool featureEnabled,
            bool npcEnabled,
            bool expected)
        {
            // Act
            bool actual = WanderingNpcPresenceLogic.ShouldShowOnMap(featureEnabled, npcEnabled);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CircleRadiusCells_IsUnifiedHalfCell()
        {
            // Act & Assert
            Assert.Equal(0.5f, WanderingNpcPresenceLogic.CircleRadiusCells, 5);
        }

        [Fact]
        public void CirclePointCount_IsFour()
        {
            // Act & Assert
            Assert.Equal(4, WanderingNpcPresenceLogic.CirclePointCount);
        }

        [Fact]
        public void CircleColor_IsDistinctPerMerchant()
        {
            // Arrange
            var colors = new Dictionary<SummonWanderingNpcKind, (float R, float G, float B)>
            {
                [SummonWanderingNpcKind.Nightingale] = WanderingNpcPresenceLogic.CircleColor(SummonWanderingNpcKind.Nightingale),
                [SummonWanderingNpcKind.CoffeeMaker] = WanderingNpcPresenceLogic.CircleColor(SummonWanderingNpcKind.CoffeeMaker),
                [SummonWanderingNpcKind.Tilde] = WanderingNpcPresenceLogic.CircleColor(SummonWanderingNpcKind.Tilde),
                [SummonWanderingNpcKind.Puppet] = WanderingNpcPresenceLogic.CircleColor(SummonWanderingNpcKind.Puppet)
            };

            // Act & Assert
            var seen = new HashSet<(float, float, float)>();
            foreach (var color in colors.Values)
            {
                Assert.InRange(color.R, 0f, 1f);
                Assert.InRange(color.G, 0f, 1f);
                Assert.InRange(color.B, 0f, 1f);
                Assert.True(seen.Add((color.R, color.G, color.B)), $"Duplicated circle color for a merchant.");
            }
        }
    }
}
