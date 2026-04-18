using System;
using BetterExperience.HConfigGUI;
using Xunit;

namespace BetterExperience.Test
{
    public class ParserTests
    {
        // -----------------------------------------------------------------------
        // Parse(Type, object) - Null input tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Parse_NullInput_ReturnsFail()
        {
            // Arrange
            var targetType = typeof(int);
            object input = null;

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Input is null.", result.Errors);
        }

        // -----------------------------------------------------------------------
        // Parse(Type, object) - Null target type tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Parse_NullTargetType_ReturnsFail()
        {
            // Arrange
            Type targetType = null;
            object input = "test";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Target type is null.", result.Errors);
        }

        // -----------------------------------------------------------------------
        // Parse(Type, object) - Enum tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Parse_ValidEnumInput_ReturnsSuccess()
        {
            // Arrange
            var targetType = typeof(TestEnum);
            object input = "Value1";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value1, result.Value);
        }

        [Fact]
        public void Parse_ValidEnumInputCaseInsensitive_ReturnsSuccess()
        {
            // Arrange
            var targetType = typeof(TestEnum);
            object input = "value2";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value2, result.Value);
        }

        [Fact]
        public void Parse_InvalidEnumInput_ReturnsFail()
        {
            // Arrange
            var targetType = typeof(TestEnum);
            object input = "InvalidValue";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Failed to parse enum:", result.Errors[0]);
        }

        // -----------------------------------------------------------------------
        // Parse(Type, object) - Non-enum type conversion tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Parse_ValidIntInput_ReturnsSuccess()
        {
            // Arrange
            var targetType = typeof(int);
            object input = "42";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Parse_ValidDoubleInput_ReturnsSuccess()
        {
            // Arrange
            var targetType = typeof(double);
            object input = "3.14";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3.14, (double)result.Value, 2);
        }

        [Fact]
        public void Parse_ValidBoolInput_ReturnsSuccess()
        {
            // Arrange
            var targetType = typeof(bool);
            object input = "true";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(true, result.Value);
        }

        [Fact]
        public void Parse_InvalidIntInput_ReturnsFail()
        {
            // Arrange
            var targetType = typeof(int);
            object input = "not a number";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Failed to parse input:", result.Errors[0]);
        }

        [Fact]
        public void Parse_InvalidBoolInput_ReturnsFail()
        {
            // Arrange
            var targetType = typeof(bool);
            object input = "not a bool";

            // Act
            var result = Parser.Parse(targetType, input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Failed to parse input:", result.Errors[0]);
        }

        // -----------------------------------------------------------------------
        // Parse<T>(object) - Generic method tests
        // -----------------------------------------------------------------------

        [Fact]
        public void ParseGeneric_ValidInput_ReturnsSuccess()
        {
            // Arrange
            object input = "123";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(123, result.Value);
        }

        [Fact]
        public void ParseGeneric_InvalidInput_ReturnsFail()
        {
            // Arrange
            object input = "not a number";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_NullInput_ReturnsFail()
        {
            // Arrange
            object input = null;

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Input is null.", result.Errors);
        }

        [Fact]
        public void ParseGeneric_ValidEnumInput_ReturnsSuccess()
        {
            // Arrange
            object input = "Value1";

            // Act
            var result = Parser.Parse<TestEnum>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value1, result.Value);
        }

        [Fact]
        public void ParseGeneric_InvalidEnumInput_ReturnsFail()
        {
            // Arrange
            object input = "InvalidValue";

            // Act
            var result = Parser.Parse<TestEnum>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        // -----------------------------------------------------------------------
        // Test helper enum
        // -----------------------------------------------------------------------

        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
}
