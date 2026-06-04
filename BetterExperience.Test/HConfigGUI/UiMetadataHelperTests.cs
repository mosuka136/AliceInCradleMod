using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigSpace;
using Moq;
using Xunit;

namespace BetterExperience.Test
{
    public class UiMetadataHelperTests
    {
        [Fact]
        public void GetMetadata_WhenEntryIsNull_ReturnsNull()
        {
            // Act
            var result = UiMetadataHelper.GetMetadata(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetMetadata_WhenEntryKeyHasSliderAttribute_ReturnsSliderMetadata()
        {
            // Arrange
            var entryMock = new Mock<IConfigEntry>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(nameof(ConfigManager.SetLootDropRatio));

            // Act
            var result = UiMetadataHelper.GetMetadata(entryMock.Object);

            // Assert
            var metadata = Assert.IsType<UiSliderMetadata>(result);
            Assert.Equal(-1f, metadata.Min);
            Assert.Equal(20f, metadata.Max);
            Assert.Equal(0.1f, metadata.Step);
            entryMock.VerifyGet(x => x.Key, Times.Once);
        }

        [Fact]
        public void GetMetadata_WhenEntryKeyDoesNotHaveSliderAttribute_ReturnsNull()
        {
            // Arrange
            var entryMock = new Mock<IConfigEntry>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(nameof(ConfigManager.EnableDebugMode));

            // Act
            var result = UiMetadataHelper.GetMetadata(entryMock.Object);

            // Assert
            Assert.Null(result);
            entryMock.VerifyGet(x => x.Key, Times.Once);
        }
    }
}
