using BetterExperience.HotkeyManager;
using Moq;

namespace BetterExperience.Test
{
    public class HotkeyTests
    {
        // -----------------------------------------------------------------------
        // Constructor(HotkeyChord[]) - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithHotkeyChordsArray_InitializesHotkeysList()
        {
            // Arrange
            var chord1 = new HotkeyChord();
            var chord2 = new HotkeyChord();
            var chords = new[] { chord1, chord2 };

            // Act
            var hotkey = new Hotkey(chords);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Contains(chord1, hotkey.Hotkeys);
            Assert.Contains(chord2, hotkey.Hotkeys);
        }

        [Fact]
        public void Constructor_WithEmptyHotkeyChordsArray_InitializesEmptyList()
        {
            // Arrange
            var chords = new HotkeyChord[0];

            // Act
            var hotkey = new Hotkey(chords);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // Constructor(IHotkeyTrigger, IHotkeyTrigger[]) - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithMainAndModifiers_CreatesHotkeyChord()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var mockMod1 = new Mock<IHotkeyTrigger>();
            var mockMod2 = new Mock<IHotkeyTrigger>();

            // Act
            var hotkey = new Hotkey(mockMain.Object, mockMod1.Object, mockMod2.Object);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Single(hotkey.Hotkeys);
            Assert.Equal(mockMain.Object, hotkey.Hotkeys[0].MainKey);
            Assert.Equal(2, hotkey.Hotkeys[0].Modifiers.Count);
        }

        [Fact]
        public void Constructor_WithMainOnly_CreatesHotkeyChordWithNoModifiers()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();

            // Act
            var hotkey = new Hotkey(mockMain.Object);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Single(hotkey.Hotkeys);
            Assert.Equal(mockMain.Object, hotkey.Hotkeys[0].MainKey);
            Assert.Empty(hotkey.Hotkeys[0].Modifiers);
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

        [Fact]
        public void WasPressedThisFrame_FirstChordPressed_ReturnsTrue()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            mockMain.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var chord = new HotkeyChord(mockMain.Object);
            var hotkey = new Hotkey(chord);

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WasPressedThisFrame_SecondChordPressed_ReturnsTrue()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            mockMain1.Setup(m => m.WasPressedThisFrame()).Returns(false);
            
            var mockMain2 = new Mock<IHotkeyTrigger>();
            mockMain2.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);
            var hotkey = new Hotkey(chord1, chord2);

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WasPressedThisFrame_NoChordsPressed_ReturnsFalse()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            mockMain1.Setup(m => m.WasPressedThisFrame()).Returns(false);
            
            var mockMain2 = new Mock<IHotkeyTrigger>();
            mockMain2.Setup(m => m.WasPressedThisFrame()).Returns(false);

            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);
            var hotkey = new Hotkey(chord1, chord2);

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
    }
}
