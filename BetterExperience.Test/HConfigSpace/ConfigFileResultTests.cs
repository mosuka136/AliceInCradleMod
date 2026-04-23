using BetterExperience.HConfigSpace;

namespace BetterExperience.Test.HConfigSpace
{
    public class ConfigFileResultTests
    {
        [Fact]
        public void AddError_WithReadOnlyList_AddsErrorsToEmptyErrorsList()
        {
            // Arrange
            var result = new ConfigFileResult<string>();
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");
            var error2 = new ConfigFileError(ConfigFileErrorCode.InvalidType, "Error 2");
            var errorsToAdd = new List<ConfigFileError> { error1, error2 };

            // Act
            result.AddError(errorsToAdd);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        [Fact]
        public void AddError_WithReadOnlyList_AddsErrorsToExistingErrorsList()
        {
            // Arrange
            var initialError = new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Initial error");
            var result = new ConfigFileResult<string>(null, false, new List<ConfigFileError> { initialError });
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");
            var error2 = new ConfigFileError(ConfigFileErrorCode.InvalidType, "Error 2");
            var errorsToAdd = new List<ConfigFileError> { error1, error2 };

            // Act
            result.AddError(errorsToAdd);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(initialError, result.Errors);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        [Fact]
        public void AddError_WithReadOnlyList_HandlesNullErrorsProperty()
        {
            // Arrange
            var result = new ConfigFileResult<string>(null, false, null);
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");
            var errorsToAdd = new List<ConfigFileError> { error1 };

            // Act
            result.AddError(errorsToAdd);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains(error1, result.Errors);
        }

        [Fact]
        public void AddError_WithParamsArray_AddsErrorsToEmptyErrorsList()
        {
            // Arrange
            var result = new ConfigFileResult<string>();
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");
            var error2 = new ConfigFileError(ConfigFileErrorCode.InvalidType, "Error 2");

            // Act
            result.AddError(error1, error2);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        [Fact]
        public void AddError_WithParamsArray_AddsErrorsToExistingErrorsList()
        {
            // Arrange
            var initialError = new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Initial error");
            var result = new ConfigFileResult<string>(null, false, new List<ConfigFileError> { initialError });
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");
            var error2 = new ConfigFileError(ConfigFileErrorCode.InvalidType, "Error 2");

            // Act
            result.AddError(error1, error2);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(initialError, result.Errors);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        [Fact]
        public void AddError_WithParamsArray_HandlesNullErrorsProperty()
        {
            // Arrange
            var result = new ConfigFileResult<string>(null, false, null);
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Error 1");

            // Act
            result.AddError(error1);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains(error1, result.Errors);
        }

        [Fact]
        public void AddError_WithParamsArray_HandlesSingleError()
        {
            // Arrange
            var result = new ConfigFileResult<int>();
            var error = new ConfigFileError(ConfigFileErrorCode.EndOfContent, "End of content");

            // Act
            result.AddError(error);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void AddError_WithParamsArray_HandlesMultipleErrors()
        {
            // Arrange
            var result = new ConfigFileResult<int>();
            var error1 = new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, "Error 1");
            var error2 = new ConfigFileError(ConfigFileErrorCode.InvalidTableName, "Error 2");
            var error3 = new ConfigFileError(ConfigFileErrorCode.InvalidKeyValuePair, "Error 3");

            // Act
            result.AddError(error1, error2, error3);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
            Assert.Contains(error3, result.Errors);
        }

        [Fact]
        public void SetValue_WithNullErrors_InitializesErrorsToEmptyArray()
        {
            // Arrange
            var result = new ConfigFileResult<string>();
            var errorsProperty = typeof(ConfigFileResult<string>).GetProperty("Errors");
            errorsProperty.SetValue(result, null);

            // Act
            result.SetValue("test value");

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.Equal("test value", result.Value);
            Assert.True(result.Success);
        }

        [Fact]
        public void SetValue_WithNonNullErrors_PreservesErrors()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Test error");
            var result = new ConfigFileResult<int>(0, false, new List<ConfigFileError> { error });

            // Act
            result.SetValue(42);

            // Assert
            Assert.Equal(42, result.Value);
            Assert.True(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void SetValue_WithEmptyErrors_PreservesEmptyErrors()
        {
            // Arrange
            var result = new ConfigFileResult<string>();

            // Act
            result.SetValue("new value");

            // Assert
            Assert.Equal("new value", result.Value);
            Assert.True(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void SetValue_WithNullValue_SetsNullValue()
        {
            // Arrange
            var result = new ConfigFileResult<string>("initial", true, Array.Empty<ConfigFileError>());

            // Act
            result.SetValue(null);

            // Assert
            Assert.Null(result.Value);
            Assert.True(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public void SetValue_UpdatesSuccessToTrue()
        {
            // Arrange
            var result = new ConfigFileResult<int>(0, false, Array.Empty<ConfigFileError>());

            // Act
            result.SetValue(100);

            // Assert
            Assert.Equal(100, result.Value);
            Assert.True(result.Success);
        }
    }

    public class ConfigFileErrorTests
    {
        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Test message", "TestMethod");

            // Act
            var result = error.ToString();

            // Assert
            Assert.Equal("[InvalidValue] Test message (Caller: TestMethod)", result);
        }

        [Fact]
        public void ToString_HandlesEmptyCaller()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Test message", "");

            // Act
            var result = error.ToString();

            // Assert
            Assert.Equal("[UnsupportedType] Test message (Caller: )", result);
        }

        [Fact]
        public void ToString_HandlesDifferentErrorCodes()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.EntryNotFound, "Entry is missing", "GetEntry");

            // Act
            var result = error.ToString();

            // Assert
            Assert.Equal("[EntryNotFound] Entry is missing (Caller: GetEntry)", result);
        }

        [Fact]
        public void GetFullMessage_ReturnsFormattedString()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Test message", "TestMethod");

            // Act
            var result = error.GetFullMessage();

            // Assert
            Assert.Equal("[InvalidValue] Test message (Caller: TestMethod)", result);
        }

        [Fact]
        public void GetFullMessage_HandlesEmptyCaller()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.TableNotFound, "Table is missing", "");

            // Act
            var result = error.GetFullMessage();

            // Assert
            Assert.Equal("[TableNotFound] Table is missing (Caller: )", result);
        }

        [Fact]
        public void GetFullMessage_HandlesDifferentErrorCodes()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.InvalidTableHeader, "Invalid header", "ParseTable");

            // Act
            var result = error.GetFullMessage();

            // Assert
            Assert.Equal("[InvalidTableHeader] Invalid header (Caller: ParseTable)", result);
        }

        [Fact]
        public void GetFullMessage_MatchesToString()
        {
            // Arrange
            var error = new ConfigFileError(ConfigFileErrorCode.EndOfContent, "Reached end", "Reader");

            // Act
            var toStringResult = error.ToString();
            var getFullMessageResult = error.GetFullMessage();

            // Assert
            Assert.Equal(toStringResult, getFullMessageResult);
        }
    }
}
