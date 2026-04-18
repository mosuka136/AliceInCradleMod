using BetterExperience.HConfigGUI;
using BetterExperience.HConfigFileSpace;
using Moq;
using Xunit;

namespace BetterExperience.Test
{
    public class UiMetadataHelperTests
    {
        [Fact]
        public void GetMetadata_WithNullEntry_ReturnsNull()
        {
            var result = UiMetadataHelper.GetMetadata(null);

            Assert.Null(result);
        }

        [Fact]
        public void GetMetadata_WithEntryWithSliderInfo_ReturnsUiSliderMetadata()
        {
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("SetLootDropRatio");

            var result = UiMetadataHelper.GetMetadata(mockEntry.Object);

            Assert.NotNull(result);
            Assert.IsType<UiSliderMetadata>(result);
            var sliderMetadata = (UiSliderMetadata)result;
            Assert.Equal(-1f, sliderMetadata.Min);
            Assert.Equal(20f, sliderMetadata.Max);
            Assert.Equal(0.1f, sliderMetadata.Step);
        }

        [Fact]
        public void GetMetadata_WithEntryWithoutSliderInfo_ReturnsNull()
        {
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("EnableBetterFishing");

            var result = UiMetadataHelper.GetMetadata(mockEntry.Object);

            Assert.Null(result);
        }
    }
}
