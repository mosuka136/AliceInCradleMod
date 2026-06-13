using BetterExperience.HClassAttribute;

namespace BetterExperience.Test.HClassAttribute
{
    public class ConfigSliderAttributeTests
    {
        [Fact]
        public void Constructor_WithExplicitStep_StoresRangeAndStep()
        {
            // Act
            var attribute = new ConfigSliderAttribute(-1f, 10f, 0.5f);

            // Assert
            Assert.Equal(-1f, attribute.Min);
            Assert.Equal(10f, attribute.Max);
            Assert.Equal(0.5f, attribute.Step);
        }

        [Fact]
        public void Constructor_WithoutStep_UsesNegativeStepSentinel()
        {
            // Act
            var attribute = new ConfigSliderAttribute(0f, 100f);

            // Assert
            Assert.Equal(0f, attribute.Min);
            Assert.Equal(100f, attribute.Max);
            Assert.Equal(-1f, attribute.Step);
        }

        [Fact]
        public void AttributeUsage_TargetsPropertiesAndDoesNotAllowMultiple()
        {
            // Act
            var usage = Attribute.GetCustomAttribute(
                typeof(ConfigSliderAttribute),
                typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;

            // Assert
            Assert.NotNull(usage);
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Property));
            Assert.True(usage.Inherited);
            Assert.False(usage.AllowMultiple);
        }
    }
}
