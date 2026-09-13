using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Test.Patches
{
    public class BetterBunAssistTests
    {
        [Theory]
        [InlineData(nameof(ConfigManager.EnableBunNoBodyTouch))]
        [InlineData(nameof(ConfigManager.EnableBunClearVision))]
        [InlineData(nameof(ConfigManager.EnableBunEatWhileCarrying))]
        public void AssistFlags_ArePersistentConfigNotSessionControls(string name)
        {
            var config = typeof(ConfigManager).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(config);
            Assert.Equal(typeof(ConfigEntry<bool>), config.PropertyType);

            Assert.Null(typeof(ControlManager).GetProperty("Set" + name.Substring("Enable".Length), BindingFlags.NonPublic | BindingFlags.Static));
            Assert.Null(typeof(ConfigManager).GetProperty("Toggle" + name.Substring("Enable".Length) + "Hotkey"));
        }
    }
}
