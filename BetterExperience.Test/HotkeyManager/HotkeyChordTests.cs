using BetterExperience.HotkeyManager;
using UnityEngine.InputSystem;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyChordTests
    {
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
