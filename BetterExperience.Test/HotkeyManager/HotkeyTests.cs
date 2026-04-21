using BetterExperience.HotkeyManager;
using Moq;
using System;
using Xunit;

namespace BetterExperience.Test
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
        // Constructor(string) - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidString_ParsesSuccessfully()
        {
            // Arrange
            var hotkeyString = "Ctrl+A";

            // Act
            var hotkey = new Hotkey(hotkeyString);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.NotEmpty(hotkey.Hotkeys);
        }

        [Fact]
        public void Constructor_WithInvalidString_ThrowsArgumentException()
        {
            // Arrange
            var hotkeyString = "InvalidKey";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Hotkey(hotkeyString));
            Assert.Contains("Invalid hotkey string", exception.Message);
            Assert.Contains(hotkeyString, exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyString_ThrowsArgumentException()
        {
            // Arrange
            var hotkeyString = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Hotkey(hotkeyString));
            Assert.Contains("Invalid hotkey string", exception.Message);
        }

        [Fact]
        public void Constructor_WithWhitespaceString_ThrowsArgumentException()
        {
            // Arrange
            var hotkeyString = "   ";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Hotkey(hotkeyString));
            Assert.Contains("Invalid hotkey string", exception.Message);
        }

        // -----------------------------------------------------------------------
        // Copy Constructor - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_CopyFromHotkey_CopiesAllChords()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            var mockMod1 = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain1.Object, mockMod1.Object);

            var mockMain2 = new Mock<IHotkeyTrigger>();
            var chord2 = new HotkeyChord(mockMain2.Object);

            var original = new Hotkey(chord1, chord2);

            // Act
            var copy = new Hotkey(original);

            // Assert
            Assert.NotNull(copy.Hotkeys);
            Assert.Equal(2, copy.Hotkeys.Count);
            Assert.NotSame(original.Hotkeys, copy.Hotkeys);
            Assert.NotSame(original.Hotkeys[0], copy.Hotkeys[0]);
            Assert.NotSame(original.Hotkeys[1], copy.Hotkeys[1]);
        }

        [Fact]
        public void Constructor_CopyFromEmptyHotkey_CreatesEmptyHotkey()
        {
            // Arrange
            var original = new Hotkey();

            // Act
            var copy = new Hotkey(original);

            // Assert
            Assert.NotNull(copy.Hotkeys);
            Assert.Empty(copy.Hotkeys);
        }

        [Fact]
        public void Constructor_CopyFromHotkey_ModifiersAreCopied()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var mockMod1 = new Mock<IHotkeyTrigger>();
            var mockMod2 = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mockMain.Object, mockMod1.Object, mockMod2.Object);
            var original = new Hotkey(chord);

            // Act
            var copy = new Hotkey(original);

            // Assert
            Assert.NotNull(copy.Hotkeys);
            Assert.Single(copy.Hotkeys);
            Assert.Equal(2, copy.Hotkeys[0].Modifiers.Count);
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

        [Fact]
        public void Count_WithMultipleChords_ReturnsCorrectCount()
        {
            // Arrange
            var chord1 = new HotkeyChord();
            var chord2 = new HotkeyChord();
            var chord3 = new HotkeyChord();
            var hotkey = new Hotkey(chord1, chord2, chord3);

            // Act
            var count = hotkey.Count;

            // Assert
            Assert.Equal(3, count);
        }

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

        [Fact]
        public void Constructor_WithSingleHotkeyChord_InitializesWithOneChord()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var hotkey = new Hotkey(chord);

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
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

        // -----------------------------------------------------------------------
        // WasPressedThisFrame() - Invalid state tests
        // -----------------------------------------------------------------------

        [Fact]
        public void WasPressedThisFrame_ValidFalse_ReturnsFalse()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            mockMain.Setup(m => m.WasPressedThisFrame()).Returns(true);
            var hotkey = new Hotkey(mockMain.Object);
            hotkey.Valid = false;

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // Add() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Add_ValidChordNotInList_AddsChord()
        {
            // Arrange
            var hotkey = new Hotkey();
            var mockMain = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mockMain.Object);

            // Act
            hotkey.Add(chord);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Contains(chord, hotkey.Hotkeys);
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
        public void Add_DuplicateChord_DoesNotAdd()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mockMain.Object);
            var hotkey = new Hotkey(chord);

            // Act
            hotkey.Add(chord);

            // Assert
            Assert.Single(hotkey.Hotkeys);
        }

        [Fact]
        public void Add_MultipleUniqueChords_AddsAll()
        {
            // Arrange
            var hotkey = new Hotkey();
            var mockMain1 = new Mock<IHotkeyTrigger>();
            var mockMain2 = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);

            // Act
            hotkey.Add(chord1);
            hotkey.Add(chord2);

            // Assert
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Contains(chord1, hotkey.Hotkeys);
            Assert.Contains(chord2, hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // Remove() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Remove_ExistingChord_RemovesChord()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mockMain.Object);
            var hotkey = new Hotkey(chord);

            // Act
            hotkey.Remove(chord);

            // Assert
            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void Remove_NullChord_DoesNothing()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mockMain.Object);
            var hotkey = new Hotkey(chord);

            // Act
            hotkey.Remove(null);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Contains(chord, hotkey.Hotkeys);
        }

        [Fact]
        public void Remove_NonExistentChord_DoesNothing()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            var mockMain2 = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);
            var hotkey = new Hotkey(chord1);

            // Act
            hotkey.Remove(chord2);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Contains(chord1, hotkey.Hotkeys);
        }

        [Fact]
        public void Remove_FromMultipleChords_RemovesOnlySpecified()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            var mockMain2 = new Mock<IHotkeyTrigger>();
            var mockMain3 = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);
            var chord3 = new HotkeyChord(mockMain3.Object);
            var hotkey = new Hotkey(chord1, chord2, chord3);

            // Act
            hotkey.Remove(chord2);

            // Assert
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Contains(chord1, hotkey.Hotkeys);
            Assert.DoesNotContain(chord2, hotkey.Hotkeys);
            Assert.Contains(chord3, hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // RemoveInvalidHotkey() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void RemoveInvalidHotkey_WithInvalidChords_RemovesThem()
        {
            // Arrange
            var mockMain = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain.Object);
            var chord2 = new HotkeyChord(); // Invalid - no MainKey set
            var hotkey = new Hotkey(chord1, chord2);

            // Act
            hotkey.RemoveInvalidHotkey();

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Contains(chord1, hotkey.Hotkeys);
            Assert.DoesNotContain(chord2, hotkey.Hotkeys);
        }

        [Fact]
        public void RemoveInvalidHotkey_AllValidChords_RemovesNone()
        {
            // Arrange
            var mockMain1 = new Mock<IHotkeyTrigger>();
            var mockMain2 = new Mock<IHotkeyTrigger>();
            var chord1 = new HotkeyChord(mockMain1.Object);
            var chord2 = new HotkeyChord(mockMain2.Object);
            var hotkey = new Hotkey(chord1, chord2);

            // Act
            hotkey.RemoveInvalidHotkey();

            // Assert
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Contains(chord1, hotkey.Hotkeys);
            Assert.Contains(chord2, hotkey.Hotkeys);
        }

        [Fact]
        public void RemoveInvalidHotkey_AllInvalidChords_RemovesAll()
        {
            // Arrange
            var chord1 = new HotkeyChord(); // Invalid - no MainKey set
            var chord2 = new HotkeyChord(); // Invalid - no MainKey set
            var hotkey = new Hotkey(chord1, chord2);

            // Act
            hotkey.RemoveInvalidHotkey();

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

        [Fact]
        public void HasSameHotkey_SameChords_ReturnsTrue()
        {
            // Arrange
            var hotkey1 = new Hotkey("Ctrl+A");
            var hotkey2 = new Hotkey("Ctrl+A");

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasSameHotkey_DifferentChords_ReturnsFalse()
        {
            // Arrange
            var hotkey1 = new Hotkey("Ctrl+A");
            var hotkey2 = new Hotkey("Ctrl+B");

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasSameHotkey_SameChordsInDifferentOrder_ReturnsTrue()
        {
            // Arrange
            var hotkey1 = new Hotkey("Ctrl+A,Shift+B");
            var hotkey2 = new Hotkey("Shift+B,Ctrl+A");

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasSameHotkey_DifferentNumberOfChords_ReturnsFalse()
        {
            // Arrange
            var hotkey1 = new Hotkey("Ctrl+A");
            var hotkey2 = new Hotkey("Ctrl+A,Shift+B");

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.False(result);
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
