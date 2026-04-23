using BetterExperience.HotkeyManager;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyTests
    {
        // -----------------------------------------------------------------------
        // Default Constructor - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_Default_InitializesEmptyHotkeysList()
        {
            // Arrange & Act
            var hotkey = new Hotkey();

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // Count Property - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Count_WithEmptyHotkeys_ReturnsZero()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var count = hotkey.Count;

            // Assert
            Assert.Equal(0, count);
        }

        // -----------------------------------------------------------------------
        // WasPressedThisFrame() - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void WasPressedThisFrame_NoHotkeys_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // TryParse(string) - Whitespace handling tests
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParse_WithWhitespaceChord_SkipsWhitespaceChord()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("Ctrl+A,   ,Shift+B");

            // Assert
            Assert.True(result);
            Assert.Equal(2, hotkey.Hotkeys.Count);
        }

        [Fact]
        public void TryParse_InvalidChord_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("InvalidKey");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Add_NullChord_DoesNotAdd()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            hotkey.Add(null);

            // Assert
            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void RemoveInvalidHotkey_EmptyList_DoesNothing()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            hotkey.RemoveInvalidHotkey();

            // Assert
            Assert.Empty(hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // HasSameHotkey() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void HasSameHotkey_OtherIsNull_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.HasSameHotkey(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasSameHotkey_BothEmpty_ReturnsTrue()
        {
            // Arrange
            var hotkey1 = new Hotkey();
            var hotkey2 = new Hotkey();

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.True(result);
        }

        // -----------------------------------------------------------------------
        // TryParse(string) - Additional tests for uncovered lines
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParse_NullString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_EmptyString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhitespaceString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("   ");

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // Decode() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Decode_ValidContent_ReturnsSuccessWithSelf()
        {
            // Arrange
            var hotkey = new Hotkey();
            var content = "Ctrl+A";

            // Act
            var result = hotkey.Decode(content);

            // Assert
            Assert.True(result.Success);
            Assert.Same(hotkey, result.Value);
        }

        [Fact]
        public void Decode_InvalidContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();
            var content = "InvalidKey";

            // Act
            var result = hotkey.Decode(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Decode_NullContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.Decode(null);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Decode_EmptyContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.Decode("");

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }
    }
}
