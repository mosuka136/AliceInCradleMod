using BetterExperience.HEnumHelper;
using System.ComponentModel;

namespace BetterExperience.Test.HEnumHelper
{
    public class EnumHelperTests
    {
        // -----------------------------------------------------------------------
        // Test Enums
        // -----------------------------------------------------------------------

        private enum TestEnum
        {
            [Description("First Value")]
            [DisplayEnum(true)]
            First,

            [Description("Second Value")]
            [DisplayEnum(false)]
            Second,

            Third
        }

        // -----------------------------------------------------------------------
        // GetAttribute<TEnum, TAttribute> - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetAttribute_WithDescriptionAttribute_ReturnsAttribute()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var result = EnumHelper.GetAttribute<TestEnum, DescriptionAttribute>(enumValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("First Value", result.Description);
        }

        [Fact]
        public void GetAttribute_WithDisplayEnumAttribute_ReturnsAttribute()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var result = EnumHelper.GetAttribute<TestEnum, DisplayEnumAttribute>(enumValue);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDisplay);
        }

        [Fact]
        public void GetAttribute_WithNoAttribute_ReturnsNull()
        {
            // Arrange
            var enumValue = TestEnum.Third;

            // Act
            var result = EnumHelper.GetAttribute<TestEnum, DescriptionAttribute>(enumValue);

            // Assert
            Assert.Null(result);
        }

        // -----------------------------------------------------------------------
        // GetAttribute<TAttribute>(Type, Enum) - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetAttribute_WithTypeAndDescriptionAttribute_ReturnsAttribute()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.First;

            // Act
            var result = EnumHelper.GetAttribute<DescriptionAttribute>(enumType, enumValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("First Value", result.Description);
        }

        [Fact]
        public void GetAttribute_WithTypeAndDisplayEnumAttribute_ReturnsAttribute()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.Second;

            // Act
            var result = EnumHelper.GetAttribute<DisplayEnumAttribute>(enumType, enumValue);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsDisplay);
        }

        [Fact]
        public void GetAttribute_WithTypeAndNoAttribute_ReturnsNull()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.Third;

            // Act
            var result = EnumHelper.GetAttribute<DescriptionAttribute>(enumType, enumValue);

            // Assert
            Assert.Null(result);
        }

        // -----------------------------------------------------------------------
        // GetDescription<TEnum> - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetDescription_WithDescriptionAttribute_ReturnsDescription()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var result = EnumHelper.GetDescription(enumValue);

            // Assert
            Assert.Equal("First Value", result);
        }

        [Fact]
        public void GetDescription_WithoutDescriptionAttribute_ReturnsEnumName()
        {
            // Arrange
            var enumValue = TestEnum.Third;

            // Act
            var result = EnumHelper.GetDescription(enumValue);

            // Assert
            Assert.Equal("Third", result);
        }

        // -----------------------------------------------------------------------
        // GetDescription(Type, Enum) - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetDescription_WithTypeAndDescriptionAttribute_ReturnsDescription()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.Second;

            // Act
            var result = EnumHelper.GetDescription(enumType, enumValue);

            // Assert
            Assert.Equal("Second Value", result);
        }

        [Fact]
        public void GetDescription_WithTypeAndNoDescriptionAttribute_ReturnsEnumName()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.Third;

            // Act
            var result = EnumHelper.GetDescription(enumType, enumValue);

            // Assert
            Assert.Equal("Third", result);
        }

        // -----------------------------------------------------------------------
        // IsDisplay<TEnum> - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void IsDisplay_WithDisplayEnumAttributeTrue_ReturnsTrue()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var result = EnumHelper.IsDisplay(enumValue);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDisplay_WithDisplayEnumAttributeFalse_ReturnsFalse()
        {
            // Arrange
            var enumValue = TestEnum.Second;

            // Act
            var result = EnumHelper.IsDisplay(enumValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsDisplay_WithoutDisplayEnumAttribute_ReturnsTrue()
        {
            // Arrange
            var enumValue = TestEnum.Third;

            // Act
            var result = EnumHelper.IsDisplay(enumValue);

            // Assert
            Assert.True(result);
        }

        // -----------------------------------------------------------------------
        // Caching behavior tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetAttribute_CalledTwice_ReturnsSameAttributeInstance()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var result1 = EnumHelper.GetAttribute<TestEnum, DescriptionAttribute>(enumValue);
            var result2 = EnumHelper.GetAttribute<TestEnum, DescriptionAttribute>(enumValue);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        [Fact]
        public void GetAttribute_WithTypeCalled_Twice_ReturnsSameAttributeInstance()
        {
            // Arrange
            var enumType = typeof(TestEnum);
            Enum enumValue = TestEnum.Second;

            // Act
            var result1 = EnumHelper.GetAttribute<DisplayEnumAttribute>(enumType, enumValue);
            var result2 = EnumHelper.GetAttribute<DisplayEnumAttribute>(enumType, enumValue);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        [Fact]
        public void GetAttribute_DifferentAttributes_ReturnsDifferentInstances()
        {
            // Arrange
            var enumValue = TestEnum.First;

            // Act
            var descAttr = EnumHelper.GetAttribute<TestEnum, DescriptionAttribute>(enumValue);
            var displayAttr = EnumHelper.GetAttribute<TestEnum, DisplayEnumAttribute>(enumValue);

            // Assert
            Assert.NotNull(descAttr);
            Assert.NotNull(displayAttr);
            Assert.IsType<DescriptionAttribute>(descAttr);
            Assert.IsType<DisplayEnumAttribute>(displayAttr);
        }
    }
}
