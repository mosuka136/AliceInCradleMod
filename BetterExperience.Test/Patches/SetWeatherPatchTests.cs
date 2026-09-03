using BetterExperience.BConfigManager;
using BetterExperience.BControlManager;
using BetterExperience.Patches;
using System.Reflection;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;
using UnityModBase.HControlSpace;

namespace BetterExperience.Test.Patches
{
    public class SetWeatherPatchTests
    {
        [Theory]
        [InlineData(nameof(ConfigManager.SetWeatherWind))]
        [InlineData(nameof(ConfigManager.SetWeatherThunder))]
        [InlineData(nameof(ConfigManager.SetWeatherMist))]
        [InlineData(nameof(ConfigManager.SetWeatherDrought))]
        [InlineData(nameof(ConfigManager.SetWeatherDenseMist))]
        [InlineData(nameof(ConfigManager.SetWeatherPlague))]
        public void WeatherConfigProperty_UsesDualPreloadValue(string propertyName)
        {
            // Act
            var property = typeof(ConfigManager).GetProperty(propertyName);

            // Assert
            Assert.NotNull(property);
            Assert.Equal(typeof(ConfigEntry<bool, bool>), property.PropertyType);
            Assert.Equal(2, property.GetCustomAttribute<EntryGuiAttribute>()?.Count);
        }

        [Theory]
        [InlineData(nameof(ControlManager.SetWeatherWind))]
        [InlineData(nameof(ControlManager.SetWeatherThunder))]
        [InlineData(nameof(ControlManager.SetWeatherMist))]
        [InlineData(nameof(ControlManager.SetWeatherDrought))]
        [InlineData(nameof(ControlManager.SetWeatherDenseMist))]
        [InlineData(nameof(ControlManager.SetWeatherPlague))]
        public void WeatherControlProperty_UsesRuntimeBooleanValue(string propertyName)
        {
            // Act
            var property = typeof(ControlManager).GetProperty(
                propertyName,
                BindingFlags.NonPublic | BindingFlags.Static);

            // Assert
            Assert.NotNull(property);
            Assert.Equal(typeof(ControlEntry<bool>), property.PropertyType);
        }

        [Theory]
        [InlineData(0, 0, true, 1)]
        [InlineData(0, 5, true, 32)]
        [InlineData(0xFFFF, 4, false, 0xFFEF)]
        [InlineData(0x20, 5, false, 0)]
        [InlineData(0x21, 5, true, 0x21)]
        [InlineData(0x21, 4, false, 0x21)]
        public void SetResetBit_WithValidBit_PreservesUnrelatedBits(
            int value,
            int bit,
            bool set,
            int expected)
        {
            // Act
            var result = HPatches.SetWeatherPatch.SetResetBit(value, bit, set);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetResetBit_WithSignBit_SetsAndClearsBit31()
        {
            // Act
            var setResult = HPatches.SetWeatherPatch.SetResetBit(0, 31, true);
            var resetResult = HPatches.SetWeatherPatch.SetResetBit(setResult, 31, false);

            // Assert
            Assert.Equal(int.MinValue, setResult);
            Assert.Equal(0, resetResult);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, true)]
        [InlineData(0x20, 5, true)]
        [InlineData(0x20, 4, false)]
        [InlineData(int.MinValue, 31, true)]
        public void GetBit_WithValidBit_ReturnsWhetherBitIsSet(int value, int bit, bool expected)
        {
            // Act
            var result = HPatches.SetWeatherPatch.GetBit(value, bit);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
