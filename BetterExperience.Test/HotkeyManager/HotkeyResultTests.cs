using BetterExperience.HotkeyManager;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyResultTests
    {
        [Fact]
        public void Constructor_Default_InitializesEmptyErrors()
        {
            // Arrange & Act
            var result = new HotkeyResult<string>();

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.False(result.Success);
            Assert.Null(result.Value);
        }

        [Fact]
        public void Fail_WithErrorList_ReturnsFailedResult()
        {
            // Arrange
            IReadOnlyList<string> errors = new List<string> { "error1", "error2" };

            // Act
            var result = HotkeyResult<int>.Fail(errors);

            // Assert
            Assert.Equal(default(int), result.Value);
            Assert.False(result.Success);
            Assert.Same(errors, result.Errors);
        }

        [Fact]
        public void Fail_WithParamsErrors_ReturnsFailedResult()
        {
            // Arrange & Act
            var result = HotkeyResult<string>.Fail("error1", "error2");

            // Assert
            Assert.Null(result.Value);
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("error1", result.Errors[0]);
            Assert.Equal("error2", result.Errors[1]);
        }

        [Fact]
        public void ToString_WhenValueIsNull_ReturnsEmptyString()
        {
            // Arrange
            var result = new HotkeyResult<string>();

            // Act
            var text = result.ToString();

            // Assert
            Assert.Equal(string.Empty, text);
        }

        [Fact]
        public void ToString_WhenValueHasContent_ReturnsValueString()
        {
            // Arrange
            var result = new HotkeyResult<string>("Ctrl+A", true, new List<string>());

            // Act
            var text = result.ToString();

            // Assert
            Assert.Equal("Ctrl+A", text);
        }

    }
}
