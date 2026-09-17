using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class ImmuneEnemyAbsorbLogicTests
    {
        [Theory]
        [InlineData(false, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        public void ShouldBlockEnemyAbsorb_OnlyCombatEnemies(
            bool immuneOn,
            bool isEnemy,
            bool isFarmAnimal,
            bool expected)
        {
            Assert.Equal(
                expected,
                ImmuneEnemyAbsorbLogic.ShouldBlockEnemyAbsorb(immuneOn, isEnemy, isFarmAnimal));
        }

        [Theory]
        [InlineData(false, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        public void ShouldFinishContainerRelease_RequiresEmptyAfterEnemyRelease(
            bool immuneOn,
            bool releasedAnyEnemy,
            bool anyActiveRemaining,
            bool expected)
        {
            Assert.Equal(
                expected,
                ImmuneEnemyAbsorbLogic.ShouldFinishContainerRelease(
                    immuneOn, releasedAnyEnemy, anyActiveRemaining));
        }

        [Fact]
        public void AssistFlag_IsPersistentConfigNotSessionControl()
        {
            var config = typeof(ConfigManager).GetProperty(
                nameof(ConfigManager.EnableImmuneEnemyAbsorb),
                BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);
            Assert.Null(typeof(ControlManager).GetProperty(
                "SetImmuneEnemyAbsorb",
                BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
