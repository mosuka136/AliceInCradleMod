using BetterExperience.BConfigManager;
using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using BetterExperience.Patches;
using m2d;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BetterExperience.Test.Patches
{
    public class BasicPatchTests : IDisposable
    {
        private readonly Dictionary<PropertyInfo, object> _originalConfigEntries = new Dictionary<PropertyInfo, object>();

        [Theory]
        [InlineData(0, 0, true, 1)]
        [InlineData(0, 5, true, 32)]
        [InlineData(0xFFFF, 4, false, 0xFFEF)]
        [InlineData(0x20, 5, false, 0)]
        public void SetResetBit_WithSetOrReset_ReturnsExpectedValue(int value, int bit, bool set, int expected)
        {
            // Act
            var result = HPatches.SetWeatherPatch.SetResetBit(value, bit, set);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SetResetBit_WithHighestBit_SupportsSignBit()
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
        [InlineData(-2147483648, 31, true)]
        public void GetBit_WithValueAndBit_ReturnsExpectedResult(int value, int bit, bool expected)
        {
            // Act
            var result = HPatches.SetWeatherPatch.GetBit(value, bit);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(-0.1f, 0.75f, 0.75f)]
        [InlineData(0f, 0.75f, 0f)]
        [InlineData(0.35f, 0.75f, 0.35f)]
        [InlineData(1f, 0.75f, 1f)]
        [InlineData(1.01f, 0.75f, 0.75f)]
        public void SetReelSpeed_WithConfiguredValue_OnlyAppliesValuesWithinInclusiveRange(
            float configuredValue,
            float originalReduceLevel,
            float expectedReduceLevel)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.SetReelSpeed), configuredValue);
            var reduceLevel = originalReduceLevel;

            // Act
            HPatches.SetReelSpeedPatch.SetReelSpeed(ref reduceLevel);

            // Assert
            Assert.Equal(expectedReduceLevel, reduceLevel);
        }

        [Theory]
        [InlineData(-1f, 1, 2f, 2f)]
        [InlineData(0f, 1, 2f, 2f)]
        [InlineData(2.5f, 0, 2f, 2f)]
        [InlineData(2.5f, 1, 2f, 5f)]
        public void SetWalkSpeed_WithConfiguredMultiplier_AppliesOnlyWhenMultiplierPositiveAndMoving(
            float multiplier,
            int moveAimEx,
            float originalSpeed,
            float expectedSpeed)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.SetPlayerWalkSpeed), multiplier);
            var result = originalSpeed;

            // Act
            HPatches.SetWalkSpeedPatch.Postfix(moveAimEx, ref result);

            // Assert
            Assert.Equal(expectedSpeed, result);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void RemoveFallingToGroundPrefix_WithConfig_ReturnsWhetherOriginalShouldRun(
            bool enableFallingToGround,
            bool expected)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableFallingToGround), enableFallingToGround);

            // Act
            var result = HPatches.RemoveFallingToGroundPatch.Prefix();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        public void NoEpDamagePrefix_WithConfig_ControlsReturnValueAndResult(
            bool enableNoEpDamage,
            bool expectedShouldRunOriginal,
            bool expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableNoEpDamage), enableNoEpDamage);
            var resultValue = true;

            // Act
            var shouldRunOriginal = HPatches.NoEpDamagePatch.Prefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            Assert.Equal(expectedResult, resultValue);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void NoMpDamagePrefix_WithConfig_ReturnsWhetherOriginalShouldRun(
            bool enableNoMpDamage,
            bool expected)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableNoMpDamage), enableNoMpDamage);

            // Act
            var result = HPatches.NoMpDamagePatch.Prefix();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void NoMapDamagePrefix_WithConfig_ControlsMapDamageResult(
            bool enableMapDamage,
            bool expectedShouldRunOriginal,
            bool expectedKeepsOriginalAttackInfo)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableMapDamage), enableMapDamage);
            var originalAttackInfo = CreateUninitialized<AttackInfo>();
            var resultValue = originalAttackInfo;

            // Act
            var shouldRunOriginal = HPatches.NoMapDamagePatch.Prefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            if (expectedKeepsOriginalAttackInfo)
            {
                Assert.Same(originalAttackInfo, resultValue);
            }
            else
            {
                Assert.Null(resultValue);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void RemoveMosaicPostfix_WithConfig_ControlsMosaicResult(
            bool enableMosaic,
            bool expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableMosaic), enableMosaic);
            var result = true;

            // Act
            HPatches.RemoveMosaicPatch.Postfix(ref result);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void InvalidateWormTrapPrefix_WithConfig_ControlsCoveringResult(
            bool enableWormTrap,
            bool expectedShouldRunOriginal,
            bool expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableWormTrap), enableWormTrap);
            var resultValue = true;

            // Act
            var shouldRunOriginal = HPatches.InvalidateWormTrapPatch.Prefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            Assert.Equal(expectedResult, resultValue);
        }

        [Theory]
        [InlineData(true, true, 0.75f)]
        [InlineData(false, false, 0f)]
        public void RemoveHolyBurstFaintPrefix_WithConfig_ControlsFaintRatio(
            bool enableHolyBurstFaint,
            bool expectedShouldRunOriginal,
            float expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableHolyBurstFaint), enableHolyBurstFaint);
            var resultValue = 0.75f;

            // Act
            var shouldRunOriginal = HPatches.RemoveHolyBurstFaintPatch.Prefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            Assert.Equal(expectedResult, resultValue);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void RemovePressDamagePrefix_WithConfig_ReturnsWhetherOriginalShouldRun(
            bool enablePressDamage,
            bool expected)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnablePressDamage), enablePressDamage);

            // Act
            var result = HPatches.RemovePressDamagePatch.Prefix();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        public void RemoveWarehouseRegionRestrictionsPrefix_WithConfig_ControlsAccessResult(
            bool enableAccessAnywhere,
            bool expectedShouldRunOriginal,
            bool expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableAccessWarehouseAnywhere), enableAccessAnywhere);
            var resultValue = false;

            // Act
            var shouldRunOriginal = HPatches.RemoveWarehouseRegionRestrictionsPatch.CanAccesableToHouseInventoryPrefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            Assert.Equal(expectedResult, resultValue);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void UnableBeingAttackedPrefix_WithConfig_ReturnsWhetherOriginalShouldRun(
            bool enableBeingAttacked,
            bool expected)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableBeingAttacked), enableBeingAttacked);

            // Act
            var result = HPatches.UnableBeingAttackedPatch.Prefix();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        public void BetterSaveSiteCanSavePrefix_WithConfig_ControlsSaveResult(
            bool enableBetterSaveSite,
            bool expectedShouldRunOriginal,
            bool expectedResult)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.EnableBetterSaveSite), enableBetterSaveSite);
            var resultValue = false;

            // Act
            var shouldRunOriginal = HPatches.BetterSaveSitePatch.CanSavePrefix(ref resultValue);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
            Assert.Equal(expectedResult, resultValue);
        }

        [Theory]
        [InlineData(-1f, true)]
        [InlineData(0f, false)]
        public void SetLootDropRatioPrefix_WithNoOverrideOrZeroRatio_ReturnsExpectedControlFlow(
            float configuredRatio,
            bool expectedShouldRunOriginal)
        {
            // Arrange
            SetConfigEntry(nameof(ConfigManager.SetLootDropRatio), configuredRatio);

            // Act
            var shouldRunOriginal = HPatches.SetLootDropRatioPatch.Prefix(null);

            // Assert
            Assert.Equal(expectedShouldRunOriginal, shouldRunOriginal);
        }

        public void Dispose()
        {
            foreach (var pair in _originalConfigEntries)
            {
                pair.Key.SetValue(null, pair.Value);
            }
        }

        private void SetConfigEntry<T>(string propertyName, T value)
        {
            var property = typeof(ConfigManager).GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(property);

            if (!_originalConfigEntries.ContainsKey(property))
                _originalConfigEntries[property] = property.GetValue(null);

            property.SetValue(null, CreateConfigEntry(propertyName, value));
        }

        private static T CreateUninitialized<T>() where T : class
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        private static ConfigEntry<T> CreateConfigEntry<T>(string key, T value)
        {
            var encodedValue = ConfigFileEntry.EncodeValue(value);
            Assert.True(encodedValue.Success);

            var entry = new ConfigFileEntry
            {
                Key = key,
                Value = encodedValue.Value
            };

            return new ConfigEntry<T>(
                "Test",
                entry,
                value,
                new Translator(key, key),
                new Translator());
        }
    }
}
