using BetterExperience.HotkeyManager;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyTriggerTests
    {
        // EqualsL tests
        [Fact]
        public void EqualsL_WithMatchingEntry_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "ctrl", "control" };
            var entry = "ctrl";

            // Act
            var result = KeyboardModifierTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualsL_WithMatchingEntryCaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "ctrl", "control" };
            var entry = "CTRL";

            // Act
            var result = KeyboardModifierTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualsL_WithNonMatchingEntry_ReturnsFalse()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "ctrl", "control" };
            var entry = "shift";

            // Act
            var result = KeyboardModifierTrigger.EqualsL(entry, list);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualsL_WithEmptyList_ReturnsFalse()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string>();
            var entry = "ctrl";

            // Act
            var result = KeyboardModifierTrigger.EqualsL(entry, list);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualsL_WithMultipleItemsAndMatchingSecondItem_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "ctrl", "control", "alt" };
            var entry = "control";

            // Act
            var result = KeyboardModifierTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        // GamepadTrigger.EqualsL tests
        [Fact]
        public void GamepadTriggerEqualsL_WithMatchingEntry_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "A", "South", "Cross" };
            var entry = "A";

            // Act
            var result = GamepadTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GamepadTriggerEqualsL_WithMatchingEntryCaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "A", "South", "Cross" };
            var entry = "a";

            // Act
            var result = GamepadTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GamepadTriggerEqualsL_WithNonMatchingEntry_ReturnsFalse()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "A", "South", "Cross" };
            var entry = "B";

            // Act
            var result = GamepadTrigger.EqualsL(entry, list);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTriggerEqualsL_WithEmptyList_ReturnsFalse()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string>();
            var entry = "A";

            // Act
            var result = GamepadTrigger.EqualsL(entry, list);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTriggerEqualsL_WithMultipleItemsAndMatchingSecondItem_ReturnsTrue()
        {
            // Arrange
            var list = new System.Collections.Generic.List<string> { "A", "South", "Cross" };
            var entry = "South";

            // Act
            var result = GamepadTrigger.EqualsL(entry, list);

            // Assert
            Assert.True(result);
        }

        // Note: IsPressed and WasPressedThisFrame tests with keyboard present
        // are not possible in standard unit tests as they require Unity runtime.
        // The lines checking IsAnySide and IsLeftSide conditions can only execute
        // when UnityInputAdapter.KeyboardCurrent is not null, which requires Unity Test Framework.
        // The null check tests are already covered by existing tests.
    }
}
