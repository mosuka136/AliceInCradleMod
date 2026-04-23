using BetterExperience.HConfigGUI;

namespace BetterExperience.Test.HConfigGUI
{
    public class ParseResultTests
    {
        [Fact]
        public void Constructor_WithValueSuccessAndErrors_SetsProperties()
        {
            // Arrange
            var value = 42;
            var success = true;
            var errors = new List<string> { "error1", "error2" };

            // Act
            var result = new ParseResult<int>(value, success, errors);

            // Assert
            Assert.Equal(value, result.Value);
            Assert.Equal(success, result.Success);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Constructor_WithNullValue_SetsNullValue()
        {
            // Arrange
            string value = null;
            var success = false;
            var errors = new List<string> { "error" };

            // Act
            var result = new ParseResult<string>(value, success, errors);

            // Assert
            Assert.Null(result.Value);
            Assert.Equal(success, result.Success);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Constructor_WithFalseSuccess_SetsSuccessToFalse()
        {
            // Arrange
            var value = "test";
            var success = false;
            var errors = new List<string>();

            // Act
            var result = new ParseResult<string>(value, success, errors);

            // Assert
            Assert.Equal(value, result.Value);
            Assert.False(result.Success);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Constructor_WithEmptyErrorList_SetsEmptyErrors()
        {
            // Arrange
            var value = 100;
            var success = true;
            var errors = new List<string>();

            // Act
            var result = new ParseResult<int>(value, success, errors);

            // Assert
            Assert.Equal(value, result.Value);
            Assert.Equal(success, result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Ok_WithValue_ReturnsSuccessfulResult()
        {
            // Arrange
            var value = "success";

            // Act
            var result = ParseResult<string>.Ok(value);

            // Assert
            Assert.Equal(value, result.Value);
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Ok_WithNullValue_ReturnsSuccessfulResultWithNull()
        {
            // Arrange
            string value = null;

            // Act
            var result = ParseResult<string>.Ok(value);

            // Assert
            Assert.Null(result.Value);
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Ok_WithDefaultValue_ReturnsSuccessfulResult()
        {
            // Arrange
            var value = 0;

            // Act
            var result = ParseResult<int>.Ok(value);

            // Assert
            Assert.Equal(value, result.Value);
            Assert.True(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Fail_WithErrorList_ReturnsFailedResultWithErrors()
        {
            // Arrange
            var errors = new List<string> { "error1", "error2", "error3" };

            // Act
            var result = ParseResult<int>.Fail(errors);

            // Assert
            Assert.Equal(default(int), result.Value);
            Assert.False(result.Success);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Fail_WithEmptyErrorList_ReturnsFailedResultWithEmptyErrors()
        {
            // Arrange
            var errors = new List<string>();

            // Act
            var result = ParseResult<string>.Fail(errors);

            // Assert
            Assert.Null(result.Value);
            Assert.False(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Fail_WithSingleError_ReturnsFailedResultWithError()
        {
            // Arrange
            var errors = new List<string> { "single error" };

            // Act
            var result = ParseResult<double>.Fail(errors);

            // Assert
            Assert.Equal(default(double), result.Value);
            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal("single error", result.Errors[0]);
        }

        [Fact]
        public void Fail_WithParamsMultipleErrors_ReturnsFailedResultWithErrors()
        {
            // Arrange & Act
            var result = ParseResult<int>.Fail("error1", "error2", "error3");

            // Assert
            Assert.Equal(default(int), result.Value);
            Assert.False(result.Success);
            Assert.Equal(3, result.Errors.Count);
            Assert.Equal("error1", result.Errors[0]);
            Assert.Equal("error2", result.Errors[1]);
            Assert.Equal("error3", result.Errors[2]);
        }

        [Fact]
        public void Fail_WithParamsSingleError_ReturnsFailedResultWithSingleError()
        {
            // Arrange & Act
            var result = ParseResult<string>.Fail("single error");

            // Assert
            Assert.Null(result.Value);
            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal("single error", result.Errors[0]);
        }

        [Fact]
        public void Fail_WithParamsNoErrors_ReturnsFailedResultWithEmptyErrors()
        {
            // Arrange & Act
            var result = ParseResult<bool>.Fail();

            // Assert
            Assert.Equal(default(bool), result.Value);
            Assert.False(result.Success);
            Assert.Empty(result.Errors);
        }
    }
}
