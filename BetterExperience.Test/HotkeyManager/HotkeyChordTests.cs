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

        [Fact]
        public void WasPressedThisFrame_WhenAnyModifierIsNotPressed_ShouldReturnFalseAndNotQueryMainKey()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var modifier = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(false);
            var chord = new HotkeyChord(unityService, mainKey.Object, modifier.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.False(result);
            modifier.Verify(x => x.IsPressed(), Moq.Times.Once);
            mainKey.Verify(x => x.WasPressedThisFrame(), Moq.Times.Never);
        }

        [Fact]
        public void HotkeyChord_WhenConstructedWithMainKeyAndModifiers_ShouldStoreProvidedValues()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var modifier1 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var modifier2 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);

            // Act
            var chord = new HotkeyChord(unityService, mainKey.Object, modifier1.Object, modifier2.Object);

            // Assert
            Assert.Same(unityService, chord.UnityService);
            Assert.Same(mainKey.Object, chord.MainKey);
            Assert.True(chord.IsValid);
            Assert.Equal(2, chord.Modifiers.Count);
            Assert.Same(modifier1.Object, chord.Modifiers[0]);
            Assert.Same(modifier2.Object, chord.Modifiers[1]);
        }

        [Fact]
        public void IsValid_WhenMainKeyIsNull_ShouldReturnFalse()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var chord = new HotkeyChord(unityService, null);

            // Assert
            Assert.False(chord.IsValid);
        }

        [Fact]
        public void WasPressedThisFrame_WhenAllModifiersArePressed_ShouldReturnMainKeyState()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var modifier1 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var modifier2 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            modifier1.Setup(x => x.IsPressed()).Returns(true);
            modifier2.Setup(x => x.IsPressed()).Returns(true);
            mainKey.Setup(x => x.WasPressedThisFrame()).Returns(true);
            var chord = new HotkeyChord(unityService, mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            var result = chord.WasPressedThisFrame();

            // Assert
            Assert.True(result);
            modifier1.Verify(x => x.IsPressed(), Moq.Times.Once);
            modifier2.Verify(x => x.IsPressed(), Moq.Times.Once);
            mainKey.Verify(x => x.WasPressedThisFrame(), Moq.Times.Once);
        }

        [Fact]
        public void AddModifier_WhenKeyIsNotModifier_ShouldLeaveModifiersUnchanged()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var chord = new HotkeyChord(unityService, mainKey.Object);

            // Act
            chord.AddModifier(Key.A);

            // Assert
            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void AddModifier_WhenAnySideModifierAlreadyExists_ShouldNotAddDuplicate()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var existingModifier = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true, unityService);
            var chord = new HotkeyChord(unityService, mainKey.Object, existingModifier);

            // Act
            chord.AddModifier(Key.LeftShift);

            // Assert
            Assert.Single(chord.Modifiers);
            Assert.Same(existingModifier, chord.Modifiers[0]);
            Assert.True(existingModifier.IsAnySide);
        }

        [Fact]
        public void AddModifier_WhenMatchingRightSideModifierAlreadyExists_ShouldNotAddDuplicate()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var existingModifier = new KeyboardModifierTrigger(Key.RightShift, unityService);
            var chord = new HotkeyChord(unityService, mainKey.Object, existingModifier);

            // Act
            chord.AddModifier(Key.RightShift);

            // Assert
            Assert.Single(chord.Modifiers);
            Assert.Same(existingModifier, chord.Modifiers[0]);
            Assert.False(existingModifier.IsAnySide);
            Assert.False(existingModifier.IsLeftSide);
        }

        [Fact]
        public void AddModifier_WhenOppositeSideModifierExists_ShouldConvertModifierToAnySide()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var existingModifier = new KeyboardModifierTrigger(Key.LeftShift, unityService);
            var chord = new HotkeyChord(unityService, mainKey.Object, existingModifier);

            // Act
            chord.AddModifier(Key.RightShift);

            // Assert
            Assert.Single(chord.Modifiers);
            Assert.Same(existingModifier, chord.Modifiers[0]);
            Assert.True(existingModifier.IsAnySide);
        }

        [Fact]
        public void AddModifier_WhenModifierDoesNotExist_ShouldAddNewKeyboardModifierTrigger()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var chord = new HotkeyChord(unityService, mainKey.Object);

            // Act
            chord.AddModifier(Key.LeftAlt);

            // Assert
            var modifier = Assert.IsType<KeyboardModifierTrigger>(Assert.Single(chord.Modifiers));
            Assert.Same(unityService, modifier.UnityService);
            Assert.Equal(Key.LeftAlt, modifier.LeftKey);
            Assert.Equal(Key.RightAlt, modifier.RightKey);
            Assert.False(modifier.IsAnySide);
            Assert.True(modifier.IsLeftSide);
        }

        [Fact]
        public void ClearModifiers_WhenModifiersExist_ShouldRemoveAllModifiers()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var modifier1 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var modifier2 = new Moq.Mock<IHotkeyTrigger>(Moq.MockBehavior.Strict);
            var chord = new HotkeyChord(unityService, mainKey.Object, modifier1.Object, modifier2.Object);

            // Act
            chord.ClearModifiers();

            // Assert
            Assert.Empty(chord.Modifiers);
        }
        [Fact]
        public void GetAnotherModifierKey_WhenLeftCtrl_ShouldReturnRightCtrl()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.LeftCtrl);

            // Assert
            Assert.Equal(Key.RightCtrl, result);
        }

        [Fact]
        public void Clear_WhenMainKeyAndModifiersExist_ShouldResetChord()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var mainKey = new KeyboardTrigger(Key.Enter, unityService);
            var modifier = new KeyboardModifierTrigger(Key.LeftShift, unityService);
            var chord = new HotkeyChord(unityService, mainKey, modifier);

            // Act
            chord.Clear();

            // Assert
            Assert.Null(chord.MainKey);
            Assert.Empty(chord.Modifiers);
            Assert.False(chord.IsValid);
        }

        [Fact]
        public void TryParse_WhenInputContainsOnlyWhitespaceSegments_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = chord.TryParse("  +   +  ");

            // Assert
            Assert.False(result);
            Assert.Null(chord.MainKey);
            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void TryParse_WhenKeyboardModifierIsInvalid_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = chord.TryParse("GamepadLB+Enter");

            // Assert
            Assert.False(result);
            Assert.IsType<KeyboardTrigger>(chord.MainKey);
            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void TryParse_WhenGamepadChordIsValidWithWhitespaceSegments_ShouldParseMainKeyAndModifiers()
        {
            // Arrange
            var chord = new HotkeyChord(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = chord.TryParse("  + GamepadLB + GamepadA + ");

            // Assert
            Assert.True(result);

            var mainKey = Assert.IsType<GamepadTrigger>(chord.MainKey);
            Assert.Equal(global::UnityEngine.InputSystem.LowLevel.GamepadButton.South, mainKey.Button);

            var modifier = Assert.IsType<GamepadTrigger>(Assert.Single(chord.Modifiers));
            Assert.Equal(global::UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder, modifier.Button);
            Assert.Equal("GamepadLB+GamepadA", chord.ToString());
        }

        [Fact]
        public void TryParse_WhenGamepadModifierIsInvalid_ShouldReturnFalse()
        {
            // Arrange
            var chord = new HotkeyChord(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = chord.TryParse("Ctrl+GamepadA");

            // Assert
            Assert.False(result);
            Assert.IsType<GamepadTrigger>(chord.MainKey);
            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void ToString_WhenMainKeyIsNullAndModifiersExist_ShouldRemoveTrailingSeparator()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();
            var modifier = new KeyboardModifierTrigger(Key.LeftShift, unityService);
            var chord = new HotkeyChord(unityService, null, modifier);

            // Act
            var result = chord.ToString();

            // Assert
            Assert.Equal("LeftShift", result);
        }

        [Fact]
        public void GetAnotherModifierKey_WhenRightCtrl_ShouldReturnLeftCtrl()
        {
            // Arrange & Act
            var result = HotkeyChord.GetAnotherModifierKey(Key.RightCtrl);

            // Assert
            Assert.Equal(Key.LeftCtrl, result);
        }





    }
}
