using System;
using BetterExperience.HConfigGUI;
using Xunit;

namespace BetterExperience.Test
{
    public class UiMetadataTests
    {
        [Fact]
        public void MetadataType_WhenAccessed_ReturnsUiSliderMetadataType()
        {
            // Arrange
            var metadata = new UiSliderMetadata(0f, 1f, 0.5f);

            // Act
            var result = metadata.MetadataType;

            // Assert
            Assert.Equal(typeof(UiSliderMetadata), result);
            Assert.Same(typeof(UiSliderMetadata), result);
        }

        [Fact]
        public void UiSliderMetadata_WhenConstructed_SetsMinMaxAndStepProperties()
        {
            // Arrange
            const float min = -10.5f;
            const float max = 20.25f;
            const float step = 0.75f;

            // Act
            var metadata = new UiSliderMetadata(min, max, step);

            // Assert
            Assert.Equal(min, metadata.Min);
            Assert.Equal(max, metadata.Max);
            Assert.Equal(step, metadata.Step);
        }

        [Fact]
        public void UiSliderMetadata_WhenConstructedWithSpecialFloatValues_PreservesProvidedValues()
        {
            // Arrange
            var min = float.NaN;
            var max = float.PositiveInfinity;
            var step = float.NegativeInfinity;

            // Act
            var metadata = new UiSliderMetadata(min, max, step);

            // Assert
            Assert.True(float.IsNaN(metadata.Min));
            Assert.Equal(max, metadata.Max);
            Assert.Equal(step, metadata.Step);
        }
    }
}
