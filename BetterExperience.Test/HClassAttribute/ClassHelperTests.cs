using BetterExperience.HClassAttribute;

namespace BetterExperience.Test.HClassAttribute
{
    public class ClassHelperTests
    {
        // Test helper class with properties for testing
        private class TestClass
        {
            [ConfigSlider(0f, 100f, 1f)]
            public float SliderProperty { get; set; }

            [ConfigSlider(10f, 50f, 0.5f)]
            public float AnotherSliderProperty { get; set; }

            [Obsolete("This is obsolete")]
            public string ObsoleteProperty { get; set; }

            public string PropertyWithoutAttribute { get; set; }
        }

        [Fact]
        public void GetAttribute_WhenPropertyHasAttribute_ReturnsAttribute()
        {
            // Arrange
            var propertyName = "SliderProperty";

            // Act
            var result = ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0f, result.Min);
            Assert.Equal(100f, result.Max);
            Assert.Equal(1f, result.Step);
        }

        [Fact]
        public void GetAttribute_WhenPropertyDoesNotHaveAttribute_ReturnsNull()
        {
            // Arrange
            var propertyName = "PropertyWithoutAttribute";

            // Act
            var result = ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAttribute_WhenPropertyDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var propertyName = "NonExistentProperty";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName));
            Assert.Contains("Property 'NonExistentProperty' not found", exception.Message);
            Assert.Contains("BetterExperience.Test.HClassAttribute.ClassHelperTests+TestClass", exception.Message);
        }

        [Fact]
        public void GetAttribute_WhenCalledTwiceForSameProperty_ReturnsCachedValue()
        {
            // Arrange
            var propertyName = "SliderProperty";

            // Act
            var result1 = ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName);
            var result2 = ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName);

            // Assert
            Assert.Same(result1, result2);
        }

        [Fact]
        public void GetAttribute_WhenPropertyHasDifferentAttributeType_ReturnsCorrectAttribute()
        {
            // Arrange
            var propertyName = "ObsoleteProperty";

            // Act
            var result = ClassHelper.GetAttribute<TestClass, ObsoleteAttribute>(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("This is obsolete", result.Message);
        }

        [Fact]
        public void GetAttribute_WhenPropertyHasAttributeButRequestingDifferentType_ReturnsNull()
        {
            // Arrange
            var propertyName = "SliderProperty";

            // Act
            var result = ClassHelper.GetAttribute<TestClass, ObsoleteAttribute>(propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAttribute_WhenPropertyNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string propertyName = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName));
            Assert.Contains("name", exception.Message);
        }

        [Fact]
        public void GetAttribute_WhenPropertyNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var propertyName = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ClassHelper.GetAttribute<TestClass, ConfigSliderAttribute>(propertyName));
            Assert.Contains("Property '' not found", exception.Message);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyHasSliderAttribute_ReturnsSliderInfo()
        {
            // Arrange
            var propertyName = "SliderProperty";

            // Act
            var result = ClassHelper.GetSliderInfo<TestClass>(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0f, result.Value.Min);
            Assert.Equal(100f, result.Value.Max);
            Assert.Equal(1f, result.Value.Step);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyHasDifferentSliderValues_ReturnsCorrectValues()
        {
            // Arrange
            var propertyName = "AnotherSliderProperty";

            // Act
            var result = ClassHelper.GetSliderInfo<TestClass>(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10f, result.Value.Min);
            Assert.Equal(50f, result.Value.Max);
            Assert.Equal(0.5f, result.Value.Step);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyDoesNotHaveSliderAttribute_ReturnsNull()
        {
            // Arrange
            var propertyName = "PropertyWithoutAttribute";

            // Act
            var result = ClassHelper.GetSliderInfo<TestClass>(propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyHasDifferentAttributeType_ReturnsNull()
        {
            // Arrange
            var propertyName = "ObsoleteProperty";

            // Act
            var result = ClassHelper.GetSliderInfo<TestClass>(propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            var propertyName = "NonExistentProperty";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ClassHelper.GetSliderInfo<TestClass>(propertyName));
            Assert.Contains("Property 'NonExistentProperty' not found", exception.Message);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string propertyName = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ClassHelper.GetSliderInfo<TestClass>(propertyName));
            Assert.Contains("name", exception.Message);
        }

        [Fact]
        public void GetSliderInfo_WhenPropertyNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var propertyName = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ClassHelper.GetSliderInfo<TestClass>(propertyName));
            Assert.Contains("not found", exception.Message);
        }
    }
}
