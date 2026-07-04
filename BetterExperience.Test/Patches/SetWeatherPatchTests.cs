using BetterExperience.Patches;

namespace BetterExperience.Test.Patches
{
    public class SetWeatherPatchTests
    {
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
