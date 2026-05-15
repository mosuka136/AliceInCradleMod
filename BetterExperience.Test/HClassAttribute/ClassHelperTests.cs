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

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        private sealed class TestMarkerAttribute : Attribute
        {
        }

        [TestMarker]
        private class MarkedClassA
        {
        }

        [TestMarker]
        private class MarkedClassB
        {
        }

        private class UnmarkedClass
        {
        }

        [RegisterOnGameBoot]
        private class RegisterOnGameBootMarkedClass
        {
        }

        private class MethodAttributeContainer
        {
            [TestMarker]
            public void MarkedPublicInstanceMethod()
            {
            }

            [TestMarker]
            private void MarkedPrivateInstanceMethod()
            {
            }

            [TestMarker]
            public static void MarkedPublicStaticMethod()
            {
            }

            [TestMarker]
            private static void MarkedPrivateStaticMethod()
            {
            }

            public void UnmarkedMethod()
            {
            }
        }

        [Fact]
        public void GetClasses_WhenAssemblyContainsMarkedClasses_ReturnsOnlyMarkedClasses()
        {
            // Arrange
            var assembly = typeof(ClassHelperTests).Assembly;

            // Act
            var result = ClassHelper.GetClasses<TestMarkerAttribute>(assembly);

            // Assert
            Assert.Contains(typeof(MarkedClassA), result);
            Assert.Contains(typeof(MarkedClassB), result);
            Assert.DoesNotContain(typeof(UnmarkedClass), result);
        }

        [Fact]
        public void GetClasses_WhenAssemblyHasNoClassesWithAttribute_ReturnsEmptyArray()
        {
            // Arrange
            var assembly = typeof(string).Assembly;

            // Act
            var result = ClassHelper.GetClasses<RegisterOnGameBootAttribute>(assembly);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetMethods_WhenAssemblyContainsMarkedMethods_ReturnsMethodsWithAllSupportedBindingFlags()
        {
            // Arrange
            var assembly = typeof(ClassHelperTests).Assembly;

            // Act
            var result = ClassHelper.GetMethods<TestMarkerAttribute>(assembly);

            // Assert
            Assert.Contains(result, method => method.DeclaringType == typeof(MethodAttributeContainer) && method.Name == nameof(MethodAttributeContainer.MarkedPublicInstanceMethod));
            Assert.Contains(result, method => method.DeclaringType == typeof(MethodAttributeContainer) && method.Name == "MarkedPrivateInstanceMethod");
            Assert.Contains(result, method => method.DeclaringType == typeof(MethodAttributeContainer) && method.Name == nameof(MethodAttributeContainer.MarkedPublicStaticMethod));
            Assert.Contains(result, method => method.DeclaringType == typeof(MethodAttributeContainer) && method.Name == "MarkedPrivateStaticMethod");
            Assert.DoesNotContain(result, method => method.DeclaringType == typeof(MethodAttributeContainer) && method.Name == nameof(MethodAttributeContainer.UnmarkedMethod));
        }

        [Fact]
        public void GetMethods_WhenAssemblyHasNoMethodsWithAttribute_ReturnsEmptyArray()
        {
            // Arrange
            var assembly = typeof(string).Assembly;

            // Act
            var result = ClassHelper.GetMethods<RegisterOnGameBootAttribute>(assembly);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetRegisterOnGameBootClasses_WhenAssemblyContainsMarkedClasses_ReturnsMarkedClasses()
        {
            // Arrange
            var assembly = typeof(ClassHelperTests).Assembly;

            // Act
            var result = ClassHelper.GetRegisterOnGameBootClasses(assembly);

            // Assert
            Assert.Contains(result, type => type == typeof(RegisterOnGameBootMarkedClass));
            Assert.DoesNotContain(result, type => type == typeof(UnmarkedClass));
        }

        private class InitializeOnGameBootMethodContainer
        {
            [InitializeOnGameBoot]
            public void MarkedPublicInstanceMethod()
            {
            }

            [InitializeOnGameBoot]
            private static void MarkedPrivateStaticMethod()
            {
            }

            public void UnmarkedMethod()
            {
            }
        }

        [Fact]
        public void GetInitializeOnGameBootMethods_WhenAssemblyContainsMarkedMethods_ReturnsOnlyMarkedMethods()
        {
            // Arrange
            var assembly = typeof(ClassHelperTests).Assembly;

            // Act
            var result = ClassHelper.GetInitializeOnGameBootMethods(assembly);

            // Assert
            Assert.Contains(result, method => method.DeclaringType == typeof(InitializeOnGameBootMethodContainer) && method.Name == nameof(InitializeOnGameBootMethodContainer.MarkedPublicInstanceMethod));
            Assert.Contains(result, method => method.DeclaringType == typeof(InitializeOnGameBootMethodContainer) && method.Name == "MarkedPrivateStaticMethod");
            Assert.DoesNotContain(result, method => method.DeclaringType == typeof(InitializeOnGameBootMethodContainer) && method.Name == nameof(InitializeOnGameBootMethodContainer.UnmarkedMethod));
        }
        [Fact]
        public void GetInitializeOnGameBootMethods_WhenAssemblyHasNoMarkedMethods_ReturnsEmptyArray()
        {
            // Arrange
            var assembly = typeof(string).Assembly;

            // Act
            var result = ClassHelper.GetInitializeOnGameBootMethods(assembly);

            // Assert
            Assert.Empty(result);
        }




    }
}
