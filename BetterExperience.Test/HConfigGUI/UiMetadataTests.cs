using BetterExperience.HConfigGUI;

namespace BetterExperience.Test.HConfigGUI
{
    public class UiMetadataTests
    {
        [Fact]
        public void Constructor_WithValidParameters_SetsMinProperty()
        {
            float min = 1.5f;
            float max = 10.0f;
            float step = 0.5f;

            var metadata = new UiSliderMetadata(min, max, step);

            Assert.Equal(min, metadata.Min);
        }

        [Fact]
        public void Constructor_WithValidParameters_SetsMaxProperty()
        {
            float min = 1.5f;
            float max = 10.0f;
            float step = 0.5f;

            var metadata = new UiSliderMetadata(min, max, step);

            Assert.Equal(max, metadata.Max);
        }

        [Fact]
        public void Constructor_WithValidParameters_SetsStepProperty()
        {
            float min = 1.5f;
            float max = 10.0f;
            float step = 0.5f;

            var metadata = new UiSliderMetadata(min, max, step);

            Assert.Equal(step, metadata.Step);
        }

        [Fact]
        public void Constructor_WithZeroValues_SetsAllPropertiesCorrectly()
        {
            float min = 0.0f;
            float max = 0.0f;
            float step = 0.0f;

            var metadata = new UiSliderMetadata(min, max, step);

            Assert.Equal(min, metadata.Min);
            Assert.Equal(max, metadata.Max);
            Assert.Equal(step, metadata.Step);
        }

        [Fact]
        public void Constructor_WithNegativeValues_SetsAllPropertiesCorrectly()
        {
            float min = -5.0f;
            float max = -1.0f;
            float step = -0.1f;

            var metadata = new UiSliderMetadata(min, max, step);

            Assert.Equal(min, metadata.Min);
            Assert.Equal(max, metadata.Max);
            Assert.Equal(step, metadata.Step);
        }

        [Fact]
        public void MetadataType_WhenAccessed_ReturnsUiSliderMetadataType()
        {
            var metadata = new UiSliderMetadata(0.0f, 100.0f, 1.0f);

            var result = metadata.MetadataType;

            Assert.Equal(typeof(UiSliderMetadata), result);
        }

        [Fact]
        public void MetadataType_WhenAccessedMultipleTimes_ReturnsSameType()
        {
            var metadata = new UiSliderMetadata(0.0f, 100.0f, 1.0f);

            var result1 = metadata.MetadataType;
            var result2 = metadata.MetadataType;

            Assert.Equal(result1, result2);
            Assert.Same(result1, result2);
        }
    }
}
