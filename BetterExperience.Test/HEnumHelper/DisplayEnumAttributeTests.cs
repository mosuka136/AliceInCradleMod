using BetterExperience.HEnumHelper;

namespace BetterExperience.Test.HEnumHelper
{
    public class DisplayEnumAttributeTests
    {
        [Fact]
        public void Constructor_WithoutParameter_DefaultsToDisplayed()
        {
            // Act
            var attribute = new DisplayEnumAttribute();

            // Assert
            Assert.True(attribute.IsDisplay);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_WithValue_StoresDisplayFlag(bool isDisplay)
        {
            // Act
            var attribute = new DisplayEnumAttribute(isDisplay);

            // Assert
            Assert.Equal(isDisplay, attribute.IsDisplay);
        }

        [Fact]
        public void AttributeUsage_TargetsEnumFieldsAndDoesNotAllowMultiple()
        {
            // Act
            var usage = Attribute.GetCustomAttribute(
                typeof(DisplayEnumAttribute),
                typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Field, usage.ValidOn);
            Assert.False(usage.Inherited);
            Assert.False(usage.AllowMultiple);
        }
    }
}
