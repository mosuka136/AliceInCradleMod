using BetterExperience.HConfigGUI;

namespace BetterExperience.Test.HConfigGUI
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

        [Fact]
        public void ParseGeneric_ValidBoolInput_ReturnsSuccess()
        {
            // Arrange
            object input = "true";

            // Act
            var result = Parser.Parse<bool>(input);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Value);
        }

        [Fact]
        public void ParseGeneric_ValidDoubleInput_ReturnsSuccess()
        {
            // Arrange
            object input = "3.14";

            // Act
            var result = Parser.Parse<double>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3.14, result.Value, 2);
        }

        [Fact]
        public void ParseGeneric_InvalidBoolInput_ReturnsFail()
        {
            // Arrange
            object input = "not a bool";

            // Act
            var result = Parser.Parse<bool>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_ValidStringInput_ReturnsSuccess()
        {
            // Arrange
            object input = "hello";

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("hello", result.Value);
        }

        [Fact]
        public void ParseGeneric_ErrorPropagation_ContainsOriginalError()
        {
            // Arrange
            object input = "not a number";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Failed to parse input:", result.Errors[0]);
        }

        // -----------------------------------------------------------------------
        // Parse<T>(object) - Edge cases and boundary conditions
        // -----------------------------------------------------------------------

        [Fact]
        public void ParseGeneric_EmptyString_ReturnsFail()
        {
            // Arrange
            object input = "";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_IntMaxValue_ReturnsSuccess()
        {
            // Arrange
            object input = "2147483647";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(int.MaxValue, result.Value);
        }

        [Fact]
        public void ParseGeneric_IntMinValue_ReturnsSuccess()
        {
            // Arrange
            object input = "-2147483648";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(int.MinValue, result.Value);
        }

        [Fact]
        public void ParseGeneric_LongValue_ReturnsSuccess()
        {
            // Arrange
            object input = "9223372036854775807";

            // Act
            var result = Parser.Parse<long>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(long.MaxValue, result.Value);
        }

        [Fact]
        public void ParseGeneric_ShortValue_ReturnsSuccess()
        {
            // Arrange
            object input = "32767";

            // Act
            var result = Parser.Parse<short>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal((short)32767, result.Value);
        }

        [Fact]
        public void ParseGeneric_ByteValue_ReturnsSuccess()
        {
            // Arrange
            object input = "255";

            // Act
            var result = Parser.Parse<byte>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal((byte)255, result.Value);
        }

        [Fact]
        public void ParseGeneric_DecimalValue_ReturnsSuccess()
        {
            // Arrange
            object input = "123.456";

            // Act
            var result = Parser.Parse<decimal>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(123.456m, result.Value);
        }

        [Fact]
        public void ParseGeneric_FloatValue_ReturnsSuccess()
        {
            // Arrange
            object input = "123.456";

            // Act
            var result = Parser.Parse<float>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(123.456f, result.Value, 3);
        }

        [Fact]
        public void ParseGeneric_ZeroValue_ReturnsSuccess()
        {
            // Arrange
            object input = "0";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void ParseGeneric_NegativeValue_ReturnsSuccess()
        {
            // Arrange
            object input = "-42";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(-42, result.Value);
        }

        [Fact]
        public void ParseGeneric_IntObjectToInt_ReturnsSuccess()
        {
            // Arrange
            object input = 123;

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(123, result.Value);
        }

        [Fact]
        public void ParseGeneric_BoolObjectToBool_ReturnsSuccess()
        {
            // Arrange
            object input = true;

            // Act
            var result = Parser.Parse<bool>(input);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Value);
        }

        [Fact]
        public void ParseGeneric_FalseString_ReturnsSuccess()
        {
            // Arrange
            object input = "false";

            // Act
            var result = Parser.Parse<bool>(input);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Value);
        }

        [Fact]
        public void ParseGeneric_Whitespace_ReturnsFail()
        {
            // Arrange
            object input = "   ";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_DoubleObjectToDouble_ReturnsSuccess()
        {
            // Arrange
            object input = 3.14;

            // Act
            var result = Parser.Parse<double>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3.14, result.Value, 2);
        }

        [Fact]
        public void ParseGeneric_EnumNumericValue_ReturnsSuccess()
        {
            // Arrange
            object input = "1";

            // Act
            var result = Parser.Parse<TestEnum>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value2, result.Value);
        }

        [Fact]
        public void ParseGeneric_EnumMixedCase_ReturnsSuccess()
        {
            // Arrange
            object input = "VaLuE3";

            // Act
            var result = Parser.Parse<TestEnum>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value3, result.Value);
        }

        [Fact]
        public void ParseGeneric_IntOverflow_ReturnsFail()
        {
            // Arrange
            object input = "9999999999999999999";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_ByteOverflow_ReturnsFail()
        {
            // Arrange
            object input = "256";

            // Act
            var result = Parser.Parse<byte>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_ShortOverflow_ReturnsFail()
        {
            // Arrange
            object input = "32768";

            // Act
            var result = Parser.Parse<short>(input);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_StringObjectToString_ReturnsSuccess()
        {
            // Arrange
            object input = "test string";

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("test string", result.Value);
        }

        [Fact]
        public void ParseGeneric_SpecialCharactersInString_ReturnsSuccess()
        {
            // Arrange
            object input = "!@#$%^&*()";

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("!@#$%^&*()", result.Value);
        }

        [Fact]
        public void ParseGeneric_NumericStringToString_ReturnsSuccess()
        {
            // Arrange
            object input = "12345";

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("12345", result.Value);
        }

        [Fact]
        public void ParseGeneric_IntToLong_ReturnsSuccess()
        {
            // Arrange
            object input = 42;

            // Act
            var result = Parser.Parse<long>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(42L, result.Value);
        }

        [Fact]
        public void ParseGeneric_StringWithLeadingZeros_ReturnsSuccess()
        {
            // Arrange
            object input = "007";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(7, result.Value);
        }

        [Fact]
        public void ParseGeneric_DoubleWithExponent_ReturnsSuccess()
        {
            // Arrange
            object input = "1.5E+3";

            // Act
            var result = Parser.Parse<double>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1500.0, result.Value, 2);
        }

        [Fact]
        public void ParseGeneric_NegativeZero_ReturnsSuccess()
        {
            // Arrange
            object input = "-0";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void ParseGeneric_SuccessHasNoErrors()
        {
            // Arrange
            object input = "123";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ParseGeneric_FailureHasValue()
        {
            // Arrange
            object input = "not a number";

            // Act
            var result = Parser.Parse<int>(input);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(default(int), result.Value);
        }

        [Fact]
        public void ParseGeneric_CharToString_ReturnsSuccess()
        {
            // Arrange
            object input = 'A';

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("A", result.Value);
        }

        [Fact]
        public void ParseGeneric_EnumZeroValue_ReturnsSuccess()
        {
            // Arrange
            object input = "0";

            // Act
            var result = Parser.Parse<TestEnum>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(TestEnum.Value1, result.Value);
        }

        [Fact]
        public void ParseGeneric_DoublePrecision_ReturnsSuccess()
        {
            // Arrange
            object input = "3.141592653589793";

            // Act
            var result = Parser.Parse<double>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3.141592653589793, result.Value, 15);
        }

        [Fact]
        public void ParseGeneric_VeryLongString_ReturnsSuccess()
        {
            // Arrange
            object input = new string('a', 1000);

            // Act
            var result = Parser.Parse<string>(input);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1000, result.Value.Length);
        }

        [Fact]
        public void ParseGeneric_HexStringToInt_ReturnsFail()
        {
            // Arrange
            object input = "0xFF";

            // Act
            var result = Parser.Parse<int>(input);

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
