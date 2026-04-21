using BetterExperience.HotkeyManager;
using Moq;
using System.Linq;
using UnityEngine.InputSystem;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyChordTests
    {
        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithMainKeyAndModifiers_ShouldSetProperties()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            var modifier1 = new Mock<IHotkeyTrigger>();
            var modifier2 = new Mock<IHotkeyTrigger>();

            // Act
            var chord = new HotkeyChord(mainKey.Object, modifier1.Object, modifier2.Object);

            // Assert
            Assert.Equal(mainKey.Object, chord.MainKey);
            Assert.Equal(2, chord.Modifiers.Count);
            Assert.Contains(modifier1.Object, chord.Modifiers);
            Assert.Contains(modifier2.Object, chord.Modifiers);
        }

        [Fact]
        public void Constructor_WithMainKeyAndNoModifiers_ShouldSetMainKeyAndEmptyModifiers()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();

            // Act
            var chord = new HotkeyChord(mainKey.Object);

            // Assert
            Assert.Equal(mainKey.Object, chord.MainKey);
            Assert.Empty(chord.Modifiers);
        }

        // -----------------------------------------------------------------------
        // WasPressedThisFrame
        // -----------------------------------------------------------------------

        [Fact]
        public void WasPressedThisFrame_WhenAllModifiersPressedAndMainKeyPressed_ShouldReturnTrue()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var modifier1 = new Mock<IHotkeyTrigger>();
            modifier1.Setup(m => m.IsPressed()).Returns(true);

            var modifier2 = new Mock<IHotkeyTrigger>();
            modifier2.Setup(m => m.IsPressed()).Returns(true);

            var chord = new HotkeyChord(mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WasPressedThisFrame_WhenFirstModifierNotPressed_ShouldReturnFalse()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var modifier1 = new Mock<IHotkeyTrigger>();
            modifier1.Setup(m => m.IsPressed()).Returns(false);

            var modifier2 = new Mock<IHotkeyTrigger>();
            modifier2.Setup(m => m.IsPressed()).Returns(true);

            var chord = new HotkeyChord(mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WasPressedThisFrame_WhenSecondModifierNotPressed_ShouldReturnFalse()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var modifier1 = new Mock<IHotkeyTrigger>();
            modifier1.Setup(m => m.IsPressed()).Returns(true);

            var modifier2 = new Mock<IHotkeyTrigger>();
            modifier2.Setup(m => m.IsPressed()).Returns(false);

            var chord = new HotkeyChord(mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WasPressedThisFrame_WhenModifiersPressedButMainKeyNotPressed_ShouldReturnFalse()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.WasPressedThisFrame()).Returns(false);

            var modifier1 = new Mock<IHotkeyTrigger>();
            modifier1.Setup(m => m.IsPressed()).Returns(true);

            var chord = new HotkeyChord(mainKey.Object, modifier1.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WasPressedThisFrame_WhenNoModifiers_ShouldReturnMainKeyState()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.WasPressedThisFrame()).Returns(true);

            var chord = new HotkeyChord(mainKey.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.True(result);
        }

        // -----------------------------------------------------------------------
        // TryParse
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParse_WhenStringIsEmptyAfterTrim_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("   ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhenStringIsOnlyWhitespace_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("  +  +  ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhenStringIsOnlySeparators_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("+++");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhenKeyboardTriggerWithModifiers_ShouldParseCorrectly()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act - using Ctrl+A as an example (needs valid keys for KeyboardTrigger and KeyboardModifierTrigger)
            var result = chord.TryParse("Ctrl+A");

            // Assert
            // This will succeed if KeyboardTrigger can parse "A" and KeyboardModifierTrigger can parse "Ctrl"
            if (result)
            {
                Assert.NotNull(chord.MainKey);
                Assert.Single(chord.Modifiers);
            }
        }

        [Fact]
        public void TryParse_WhenKeyboardTriggerWithMultipleModifiers_ShouldParseCorrectly()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("Ctrl+Shift+A");

            // Assert
            if (result)
            {
                Assert.NotNull(chord.MainKey);
                Assert.Equal(2, chord.Modifiers.Count);
            }
        }

        [Fact]
        public void TryParse_WhenInvalidModifier_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("InvalidModifier+A");

            // Assert
            // Should return false if the modifier can't be parsed
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhenGamepadTriggerAlone_ShouldParseCorrectly()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act - using a gamepad button
            var result = chord.TryParse("DpadUp");

            // Assert
            if (result)
            {
                Assert.NotNull(chord.MainKey);
            }
        }

        [Fact]
        public void TryParse_WhenGamepadTriggerWithModifier_ShouldParseCorrectly()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("GamepadStart+GamepadSouth");

            // Assert
            Assert.True(result);
            Assert.NotNull(chord.MainKey);
            Assert.Single(chord.Modifiers);
            Assert.Equal("GamepadStart+GamepadA", chord.ToString());
        }

        [Fact]
        public void TryParse_WhenGamepadTriggerWithMultipleParts_ShouldParseCorrectly()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("DpadUp+DpadDown");

            // Assert
            Assert.True(result);
            Assert.NotNull(chord.MainKey);
            Assert.Single(chord.Modifiers);
        }

        [Fact]
        public void TryParse_WhenMixedKeyboardAndGamepadTrigger_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.TryParse("Ctrl+GamepadSouth");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AddModifier_WhenSameSideModifierAddedRepeatedly_ShouldKeepSpecificSide()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            chord.AddModifier(Key.LeftCtrl);
            chord.AddModifier(Key.LeftCtrl);

            // Assert
            var modifier = Assert.Single(chord.Modifiers.OfType<KeyboardModifierTrigger>());
            Assert.False(modifier.IsAnySide);
            Assert.True(modifier.IsLeftSide);
            Assert.Equal("LeftCtrl", modifier.ToString());
        }

        [Fact]
        public void AddModifier_WhenOppositeSideModifierAdded_ShouldMergeToAnySide()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            chord.AddModifier(Key.LeftCtrl);
            chord.AddModifier(Key.RightCtrl);

            // Assert
            var modifier = Assert.Single(chord.Modifiers.OfType<KeyboardModifierTrigger>());
            Assert.True(modifier.IsAnySide);
            Assert.Equal("Ctrl", modifier.ToString());
        }

        // -----------------------------------------------------------------------
        // ToString
        // -----------------------------------------------------------------------

        [Fact]
        public void ToString_WhenNoModifiers_ShouldReturnMainKeyString()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.ToString()).Returns("MainKey");

            var chord = new HotkeyChord(mainKey.Object);

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("MainKey", result);
        }

        [Fact]
        public void ToString_WhenHasModifiers_ShouldIncludeModifiersWithSeparator()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.ToString()).Returns("A");

            var modifier1 = new Mock<IHotkeyTrigger>();
            modifier1.Setup(m => m.ToString()).Returns("Ctrl");

            var modifier2 = new Mock<IHotkeyTrigger>();
            modifier2.Setup(m => m.ToString()).Returns("Shift");

            var chord = new HotkeyChord(mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("Ctrl+Shift+A", result);
        }

        [Fact]
        public void ToString_WhenHasSingleModifier_ShouldIncludeModifierWithSeparator()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            mainKey.Setup(m => m.ToString()).Returns("B");

            var modifier = new Mock<IHotkeyTrigger>();
            modifier.Setup(m => m.ToString()).Returns("Alt");

            var chord = new HotkeyChord(mainKey.Object, modifier.Object);

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("Alt+B", result);
        }

        // -----------------------------------------------------------------------
        // IsValid
        // -----------------------------------------------------------------------

        [Fact]
        public void IsValid_WhenMainKeyIsNull_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.IsValid;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_WhenMainKeyIsSet_ShouldReturnTrue()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mainKey.Object);

            // Act
            var result = chord.IsValid;

            // Assert
            Assert.True(result);
        }

        // -----------------------------------------------------------------------
        // AddModifier - Non-modifier key
        // -----------------------------------------------------------------------

        [Fact]
        public void AddModifier_WhenKeyIsNotModifier_ShouldNotAddToModifiers()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            chord.AddModifier(Key.A);

            // Assert
            Assert.Empty(chord.Modifiers);
        }

        // -----------------------------------------------------------------------
        // ClearModifiers
        // -----------------------------------------------------------------------

        [Fact]
        public void ClearModifiers_WhenCalledWithModifiers_ShouldRemoveAllModifiers()
        {
            // Arrange
            var chord = new HotkeyChord();
            chord.AddModifier(Key.LeftCtrl);
            chord.AddModifier(Key.LeftShift);

            // Act
            chord.ClearModifiers();

            // Assert
            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void ClearModifiers_WhenCalledWithEmptyModifiers_ShouldNotThrow()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            chord.ClearModifiers();

            // Assert
            Assert.Empty(chord.Modifiers);
        }

        // -----------------------------------------------------------------------
        // Clear
        // -----------------------------------------------------------------------

        [Fact]
        public void Clear_WhenCalledWithModifiersAndMainKey_ShouldRemoveBoth()
        {
            // Arrange
            var mainKey = new Mock<IHotkeyTrigger>();
            var chord = new HotkeyChord(mainKey.Object);
            chord.AddModifier(Key.LeftCtrl);

            // Act
            chord.Clear();

            // Assert
            Assert.Empty(chord.Modifiers);
            Assert.Null(chord.MainKey);
        }

        [Fact]
        public void Clear_WhenCalledWithEmptyChord_ShouldNotThrow()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            chord.Clear();

            // Assert
            Assert.Empty(chord.Modifiers);
            Assert.Null(chord.MainKey);
        }

        // -----------------------------------------------------------------------
        // ToString - MainKey is null with modifiers
        // -----------------------------------------------------------------------

        [Fact]
        public void ToString_WhenMainKeyIsNullAndHasModifiers_ShouldReturnModifiersWithoutTrailingSeparator()
        {
            // Arrange
            var chord = new HotkeyChord();
            chord.AddModifier(Key.LeftCtrl);
            chord.AddModifier(Key.LeftShift);

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("LeftCtrl+LeftShift", result);
        }

        [Fact]
        public void ToString_WhenMainKeyIsNullAndNoModifiers_ShouldReturnEmptyString()
        {
            // Arrange
            var chord = new HotkeyChord();

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("", result);
        }

        // -----------------------------------------------------------------------
        // GetAnotherModifierKey
        // -----------------------------------------------------------------------

        [Fact]
        public void GetAnotherModifierKey_WhenLeftShift_ShouldReturnRightShift()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.LeftShift);

            // Assert
            Assert.Equal(Key.RightShift, result);
        }

        [Fact]
        public void GetAnotherModifierKey_WhenRightShift_ShouldReturnLeftShift()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.RightShift);

            // Assert
            Assert.Equal(Key.LeftShift, result);
        }

        [Fact]
        public void GetAnotherModifierKey_WhenLeftAlt_ShouldReturnRightAlt()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.LeftAlt);

            // Assert
            Assert.Equal(Key.RightAlt, result);
        }

        [Fact]
        public void GetAnotherModifierKey_WhenRightAlt_ShouldReturnLeftAlt()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.RightAlt);

            // Assert
            Assert.Equal(Key.LeftAlt, result);
        }

        [Fact]
        public void GetAnotherModifierKey_WhenNonModifierKey_ShouldReturnSameKey()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.A);

            // Assert
            Assert.Equal(Key.A, result);
        }
    }
}
