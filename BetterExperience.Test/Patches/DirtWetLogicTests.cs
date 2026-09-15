using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using nel;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class DirtWetLogicTests
    {
        [Theory]
        [InlineData(false, BetoInfo.TYPE.STAIN, false)]
        [InlineData(true, BetoInfo.TYPE.STAIN, true)]
        [InlineData(true, BetoInfo.TYPE.LIQUID, true)]
        [InlineData(true, BetoInfo.TYPE.SMOKE, true)]
        [InlineData(true, BetoInfo.TYPE.CUTTED, true)]
        [InlineData(true, BetoInfo.TYPE.FROZEN, false)]
        [InlineData(true, BetoInfo.TYPE.STONE_WHOLE, false)]
        [InlineData(true, BetoInfo.TYPE.WEB_TRAPPED, false)]
        public void ShouldSkipDirt_BlocksStainsButKeepsSpecialOverlays(bool noDirt, BetoInfo.TYPE type, bool expected)
        {
            Assert.Equal(expected, DirtWetLogic.ShouldSkipDirt(noDirt, type));
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        public void ShouldSkipWetten_OnlyBlocksBecomingWet(bool noWet, bool becomingWet, bool expected)
        {
            Assert.Equal(expected, DirtWetLogic.ShouldSkipWetten(noWet, becomingWet));
        }

        [Theory]
        [InlineData(nameof(ConfigManager.EnableNoDirtStains))]
        [InlineData(nameof(ConfigManager.EnableNoWetten))]
        public void AssistFlags_ArePersistentConfigNotSessionControls(string name)
        {
            var config = typeof(ConfigManager).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);
            Assert.Null(typeof(ControlManager).GetProperty(
                "Set" + name.Substring("Enable".Length),
                BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
