using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using Moq;
using UnityEngine.InputSystem;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyChordTests
    {
        [Theory]
        [InlineData(Key.LeftShift, Key.RightShift)]
        [InlineData(Key.RightShift, Key.LeftShift)]
        [InlineData(Key.LeftCtrl, Key.RightCtrl)]
        [InlineData(Key.RightAlt, Key.LeftAlt)]
        [InlineData(Key.A, Key.A)]
        public void GetAnotherModifierKey_ReturnsExpectedKey(Key key, Key expected)
        {
            var result = KeyboardChord.GetAnotherModifierKey(key);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void KeyboardChord_WasPressedThisFrame_WhenModifierNotPressed_DoesNotQueryMainKey()
        {
            var unityService = new UnityProvider();
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var mainKey = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(false);
            var chord = new KeyboardChord(unityService, mainKey.Object, modifier.Object);

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
            mainKey.Verify(x => x.WasPressedThisFrame(), Times.Never);
        }

        [Fact]
        public void KeyboardChord_AddModifier_WhenOppositeSideExists_MergesToAnySide()
        {
            var unityService = new UnityProvider();
            var existingModifier = new KeyboardModifierTrigger(Key.LeftShift, unityService);
            var chord = new KeyboardChord(unityService, new KeyboardTrigger(Key.Enter, unityService), existingModifier);

            chord.AddModifier(Key.RightShift);

            var modifier = Assert.IsType<KeyboardModifierTrigger>(Assert.Single(chord.Modifiers));
            Assert.Same(existingModifier, modifier);
            Assert.True(modifier.IsAnySide);
        }

        [Fact]
        public void KeyboardChord_SetMainKey_WhenModifierKey_IgnoresInput()
        {
            var unityService = new UnityProvider();
            var chord = new KeyboardChord(unityService);

            chord.SetMainKey(Key.LeftCtrl);

            Assert.Null(chord.MainKey);
            Assert.False(chord.IsValid);
        }

        [Fact]
        public void KeyboardChord_TryParse_WhenValidKeyboardChord_ReturnsMainKeyAndModifiers()
        {
            var unityService = new UnityProvider();

            var result = KeyboardChord.TryParse("Ctrl+LeftShift+Enter", unityService);

            Assert.True(result.Success);
            Assert.NotNull(result.Value);

            var chord = result.Value;
            var mainKey = Assert.IsType<KeyboardTrigger>(chord.MainKey);
            Assert.Equal(Key.Enter, mainKey.Key);

            Assert.Equal(2, chord.Modifiers.Count);
            Assert.Equal("Ctrl", chord.Modifiers[0].ToString());
            Assert.Equal("LeftShift", chord.Modifiers[1].ToString());
            Assert.Equal("Ctrl+LeftShift+Enter", chord.ToString());
        }

        [Fact]
        public void KeyboardChord_TryParse_WhenInputContainsOnlyWhitespaceParts_ReturnsFailure()
        {
            var result = KeyboardChord.TryParse("  +   +  ", new UnityProvider());

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void GamepadChord_TryParse_WhenValidChord_ReturnsButtons()
        {
            var result = GamepadChord.TryParse("GamepadLB+GamepadA", new UnityProvider());

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Buttons.Count);
            Assert.Equal("GamepadLB+GamepadA", result.Value.ToString());
        }

        [Fact]
        public void HotkeyChord_TryParse_WhenKeyboardChord_ReturnsWrappedKeyboardChord()
        {
            var result = HotkeyChord.TryParse("Ctrl+A", new UnityProvider());

            Assert.True(result.Success);
            Assert.IsType<KeyboardChord>(result.Value.Chord);
            Assert.Equal("Ctrl+A", result.Value.ToString());
        }

        [Fact]
        public void HotkeyChord_TryParse_WhenInputMixesKeyboardAndGamepad_ReturnsFailure()
        {
            var result = HotkeyChord.TryParse("Ctrl+GamepadA", new UnityProvider());

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void HotkeyChord_Clear_ClearsWrappedChord()
        {
            var unityService = new UnityProvider();
            var parseResult = HotkeyChord.TryParse("Ctrl+A", unityService);
            Assert.True(parseResult.Success);

            var chord = parseResult.Value;
            chord.Clear();

            Assert.False(chord.IsValid);
            Assert.Equal(string.Empty, chord.ToString());
        }

        [Fact]
        public void HotkeyChord_Clone_CreatesNewWrapperAndClonedInnerChord()
        {
            var unityService = new UnityProvider();
            var parseResult = HotkeyChord.TryParse("Ctrl+A", unityService);
            Assert.True(parseResult.Success);

            var chord = parseResult.Value;
            var clone = Assert.IsType<HotkeyChord>(chord.Clone());

            Assert.NotSame(chord, clone);
            Assert.NotSame(chord.Chord, clone.Chord);
            Assert.Equal(chord.ToString(), clone.ToString());
        }

        [Fact]
        public void HotkeyChord_IsPressed_WhenWrappedChordIsNull_ReturnsFalse()
        {
            var chord = new HotkeyChord(new UnityProvider());

            var result = chord.IsPressed();

            Assert.False(result);
        }

        [Fact]
        public void HotkeyChord_IsPressed_WhenWrappedChordIsPressed_ReturnsTrue()
        {
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.IsPressed()).Returns(true);
            var chord = new HotkeyChord(wrappedChord.Object, new UnityProvider());

            var result = chord.IsPressed();

            Assert.True(result);
            wrappedChord.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void HotkeyChord_WasPressedThisFrame_WhenWrappedChordIsNull_ReturnsFalse()
        {
            var chord = new HotkeyChord(new UnityProvider());

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
        }

        [Fact]
        public void HotkeyChord_WasPressedThisFrame_WhenWrappedChordWasPressedThisFrame_ReturnsTrue()
        {
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.WasPressedThisFrame()).Returns(true);
            var chord = new HotkeyChord(wrappedChord.Object, new UnityProvider());

            var result = chord.WasPressedThisFrame();

            Assert.True(result);
            wrappedChord.Verify(x => x.WasPressedThisFrame(), Times.Once);
        }

        [Fact]
        public void HotkeyChord_Clear_WhenWrappedChordIsNull_DoesNotThrow()
        {
            var chord = new HotkeyChord(new UnityProvider());

            var exception = Record.Exception(() => chord.Clear());

            Assert.Null(exception);
            Assert.False(chord.IsValid);
        }

        [Fact]
        public void HotkeyChord_Clear_WhenWrappedChordExists_CallsInnerClear()
        {
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.Clear());
            var chord = new HotkeyChord(wrappedChord.Object, new UnityProvider());

            chord.Clear();

            wrappedChord.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public void HotkeyChord_ToString_WhenWrappedChordIsNull_ReturnsEmptyString()
        {
            var chord = new HotkeyChord(new UnityProvider());

            var result = chord.ToString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void HotkeyChord_ToString_WhenWrappedChordExists_ReturnsInnerString()
        {
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.ToString()).Returns("Ctrl+K");
            var chord = new HotkeyChord(wrappedChord.Object, new UnityProvider());

            var result = chord.ToString();

            Assert.Equal("Ctrl+K", result);
            wrappedChord.Verify(x => x.ToString(), Times.Once);
        }

        [Fact]
        public void HotkeyChord_Clone_WhenWrappedChordIsNull_CreatesWrapperWithNullInnerChord()
        {
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(unityService);

            var clone = Assert.IsType<HotkeyChord>(chord.Clone());

            Assert.NotSame(chord, clone);
            Assert.Same(unityService, clone.UnityService);
            Assert.Null(clone.Chord);
        }

        [Fact]
        public void HotkeyChord_Clone_WhenWrappedChordExists_ClonesInnerChord()
        {
            var clonedInnerChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            clonedInnerChord.SetupGet(x => x.IsValid).Returns(true);
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.Clone()).Returns(clonedInnerChord.Object);
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(wrappedChord.Object, unityService);

            var clone = Assert.IsType<HotkeyChord>(chord.Clone());

            Assert.NotSame(chord, clone);
            Assert.Same(unityService, clone.UnityService);
            Assert.Same(clonedInnerChord.Object, clone.Chord);
            wrappedChord.Verify(x => x.Clone(), Times.Once);
        }


        [Fact]
        public void KeyboardChord_IsPressed_WhenAllModifiersAndMainKeyArePressed_ReturnsTrue()
        {
            var unityService = new UnityProvider();
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var mainKey = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(true);
            mainKey.Setup(x => x.IsPressed()).Returns(true);
            var chord = new KeyboardChord(unityService, mainKey.Object, modifier.Object);

            var result = chord.IsPressed();

            Assert.True(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
            mainKey.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void KeyboardChord_HasAnyKey_WhenMainKeyAndModifiersAreMissing_ReturnsFalse()
        {
            var chord = new KeyboardChord(new UnityProvider());

            var result = chord.HasAnyKey;

            Assert.False(result);
        }

        [Fact]
        public void KeyboardChord_HasAnyKey_WhenMainKeyExists_ReturnsTrue()
        {
            var chord = new KeyboardChord(new UnityProvider());
            chord.MainKey = new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object;

            var result = chord.HasAnyKey;

            Assert.True(result);
        }

        [Fact]
        public void KeyboardChord_HasAnyKey_WhenModifierExists_ReturnsTrue()
        {
            var chord = new KeyboardChord(new UnityProvider());
            chord.Modifiers.Add(new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object);

            var result = chord.HasAnyKey;

            Assert.True(result);
        }

        [Fact]
        public void KeyboardChord_IsPressed_WhenModifierNotPressed_DoesNotQueryMainKey()
        {
            var unityService = new UnityProvider();
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var mainKey = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(false);
            var chord = new KeyboardChord(unityService, mainKey.Object, modifier.Object);

            var result = chord.IsPressed();

            Assert.False(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
            mainKey.Verify(x => x.IsPressed(), Times.Never);
        }

        [Fact]
        public void KeyboardChord_IsPressed_WhenMainKeyIsMissing_ReturnsFalse()
        {
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(true);
            var chord = new KeyboardChord(new UnityProvider(), null, modifier.Object);

            var result = chord.IsPressed();

            Assert.False(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void KeyboardChord_WasPressedThisFrame_WhenAllModifiersPressedAndMainKeyWasPressed_ReturnsTrue()
        {
            var unityService = new UnityProvider();
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var mainKey = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(true);
            mainKey.Setup(x => x.WasPressedThisFrame()).Returns(true);
            var chord = new KeyboardChord(unityService, mainKey.Object, modifier.Object);

            var result = chord.WasPressedThisFrame();

            Assert.True(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
            mainKey.Verify(x => x.WasPressedThisFrame(), Times.Once);
        }

        [Fact]
        public void KeyboardChord_WasPressedThisFrame_WhenMainKeyIsMissing_ReturnsFalse()
        {
            var modifier = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            modifier.Setup(x => x.IsPressed()).Returns(true);
            var chord = new KeyboardChord(new UnityProvider(), null, modifier.Object);

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
            modifier.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void KeyboardChord_AddModifier_WhenKeyIsNotModifier_DoesNothing()
        {
            var chord = new KeyboardChord(new UnityProvider());

            chord.AddModifier(Key.A);

            Assert.Empty(chord.Modifiers);
        }

        [Fact]
        public void KeyboardChord_AddModifier_WhenSameModifierAlreadyExists_DoesNotAddDuplicate()
        {
            var unityService = new UnityProvider();
            var existingModifier = new KeyboardModifierTrigger(Key.LeftShift, unityService);
            var chord = new KeyboardChord(unityService, new KeyboardTrigger(Key.Enter, unityService), existingModifier);

            chord.AddModifier(Key.LeftShift);

            var modifier = Assert.IsType<KeyboardModifierTrigger>(Assert.Single(chord.Modifiers));
            Assert.Same(existingModifier, modifier);
            Assert.False(modifier.IsAnySide);
            Assert.True(modifier.IsLeftSide);
        }

        [Fact]
        public void KeyboardChord_AddModifier_WhenModifierDoesNotExist_AddsNewModifier()
        {
            var unityService = new UnityProvider();
            var chord = new KeyboardChord(unityService);

            chord.AddModifier(Key.LeftAlt);

            var modifier = Assert.IsType<KeyboardModifierTrigger>(Assert.Single(chord.Modifiers));
            Assert.Equal(Key.LeftAlt, modifier.LeftKey);
            Assert.Equal(Key.RightAlt, modifier.RightKey);
            Assert.False(modifier.IsAnySide);
            Assert.True(modifier.IsLeftSide);
        }

        [Fact]
        public void KeyboardChord_SetMainKey_WhenKeyIsNotModifier_SetsKeyboardTrigger()
        {
            var unityService = new UnityProvider();
            var chord = new KeyboardChord(unityService);

            chord.SetMainKey(Key.Enter);

            var mainKey = Assert.IsType<KeyboardTrigger>(chord.MainKey);
            Assert.Equal(Key.Enter, mainKey.Key);
            Assert.True(chord.IsValid);
            Assert.True(chord.HasAnyKey);
        }


        [Fact]
        public void KeyboardChord_ClearModifiers_WhenModifiersExist_ClearsModifiersAndKeepsMainKey()
        {
            var unityService = new UnityProvider();
            var chord = new KeyboardChord(
                unityService,
                new KeyboardTrigger(Key.Enter, unityService),
                new KeyboardModifierTrigger(Key.LeftCtrl, unityService),
                new KeyboardModifierTrigger(Key.LeftShift, unityService));

            chord.ClearModifiers();

            Assert.Empty(chord.Modifiers);
            Assert.NotNull(chord.MainKey);
            Assert.True(chord.IsValid);
        }

        [Fact]
        public void KeyboardChord_TryParse_WhenModifierIsInvalid_ReturnsFailure()
        {
            var result = KeyboardChord.TryParse("NotAModifier+Enter", new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Failed to parse keyboard modifier: NotAModifier", result.Errors);
        }

        [Fact]
        public void KeyboardChord_ToString_WhenMainKeyIsNullAndHasModifiers_RemovesTrailingSeparator()
        {
            var unityService = new UnityProvider();
            var chord = new KeyboardChord(unityService, null, new KeyboardModifierTrigger(Key.LeftCtrl, unityService));

            var result = chord.ToString();

            Assert.Equal("LeftCtrl", result);
        }

        [Theory]
        [InlineData(Key.RightCtrl, Key.LeftCtrl)]
        [InlineData(Key.LeftAlt, Key.RightAlt)]
        public void GetAnotherModifierKey_WhenModifierBranchNotPreviouslyCovered_ReturnsExpectedKey(Key key, Key expected)
        {
            var result = KeyboardChord.GetAnotherModifierKey(key);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GamepadChord_IsValid_WhenButtonsAreEmpty_ReturnsFalse()
        {
            var chord = new GamepadChord(new UnityProvider());

            var result = chord.IsValid;

            Assert.False(result);
        }

        [Fact]
        public void GamepadChord_IsValid_WhenButtonsExist_ReturnsTrue()
        {
            var chord = new GamepadChord(new UnityProvider(), new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object);

            var result = chord.IsValid;

            Assert.True(result);
        }




        [Fact]
        public void GamepadChord_ConstructorWithKeys_InitializesButtonsAndCount()
        {
            var unityService = new UnityProvider();
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object;
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object;

            var chord = new GamepadChord(unityService, firstButton, secondButton);

            Assert.Same(unityService, chord.UnityService);
            Assert.Equal(2, chord.Count);
            Assert.Equal(new[] { firstButton, secondButton }, chord.Buttons);
        }

        [Fact]
        public void GamepadChord_IsPressed_WhenButtonsAreEmpty_ReturnsFalse()
        {
            var chord = new GamepadChord(new UnityProvider());

            var result = chord.IsPressed();

            Assert.False(result);
        }

        [Fact]
        public void GamepadChord_IsPressed_WhenAllButtonsArePressed_ReturnsTrue()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.IsPressed()).Returns(true);
            secondButton.Setup(x => x.IsPressed()).Returns(true);
            var chord = new GamepadChord(new UnityProvider(), firstButton.Object, secondButton.Object);

            var result = chord.IsPressed();

            Assert.True(result);
            firstButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void GamepadChord_IsPressed_WhenAnyButtonIsNotPressed_ReturnsFalse()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.IsPressed()).Returns(true);
            secondButton.Setup(x => x.IsPressed()).Returns(false);
            var chord = new GamepadChord(new UnityProvider(), firstButton.Object, secondButton.Object);

            var result = chord.IsPressed();

            Assert.False(result);
            firstButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.IsPressed(), Times.Once);
        }

        [Fact]
        public void GamepadChord_WasPressedThisFrame_WhenButtonsAreEmpty_ReturnsFalse()
        {
            var chord = new GamepadChord(new UnityProvider());

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
        }

        [Fact]
        public void GamepadChord_WasPressedThisFrame_WhenOneButtonWasPressedThisFrameAndOthersArePressed_ReturnsTrue()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.WasPressedThisFrame()).Returns(true);
            firstButton.Setup(x => x.IsPressed()).Returns(false);
            secondButton.Setup(x => x.IsPressed()).Returns(true);
            var chord = new GamepadChord(new UnityProvider(), firstButton.Object, secondButton.Object);

            var result = chord.WasPressedThisFrame();

            Assert.True(result);
            firstButton.Verify(x => x.WasPressedThisFrame(), Times.Exactly(2));
            firstButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.WasPressedThisFrame(), Times.Never);
        }

        [Fact]
        public void GamepadChord_WasPressedThisFrame_WhenNoButtonWasPressedThisFrame_ReturnsFalse()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.WasPressedThisFrame()).Returns(false);
            secondButton.Setup(x => x.WasPressedThisFrame()).Returns(false);
            var chord = new GamepadChord(new UnityProvider(), firstButton.Object, secondButton.Object);

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
            firstButton.Verify(x => x.WasPressedThisFrame(), Times.Once);
            secondButton.Verify(x => x.WasPressedThisFrame(), Times.Once);
        }

        [Fact]
        public void GamepadChord_WasPressedThisFrame_WhenPressedThisFrameButtonLeavesAnotherInactive_ReturnsFalse()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.WasPressedThisFrame()).Returns(true);
            firstButton.Setup(x => x.IsPressed()).Returns(false);
            secondButton.Setup(x => x.IsPressed()).Returns(false);
            secondButton.Setup(x => x.WasPressedThisFrame()).Returns(false);
            var chord = new GamepadChord(new UnityProvider(), firstButton.Object, secondButton.Object);

            var result = chord.WasPressedThisFrame();

            Assert.False(result);
            firstButton.Verify(x => x.WasPressedThisFrame(), Times.Exactly(2));
            firstButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.IsPressed(), Times.Once);
            secondButton.Verify(x => x.WasPressedThisFrame(), Times.Once);
        }

        [Fact]
        public void GamepadChord_Clear_WhenButtonsExist_RemovesAllButtons()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object;
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict).Object;
            var chord = new GamepadChord(new UnityProvider(), firstButton, secondButton);

            chord.Clear();

            Assert.Empty(chord.Buttons);
            Assert.Equal(0, chord.Count);
            Assert.False(chord.IsValid);
        }



        [Fact]
        public void HotkeyChord_TryParse_WhenGamepadChord_ReturnsWrappedGamepadChord()
        {
            var result = HotkeyChord.TryParse("GamepadA", new UnityProvider());

            Assert.True(result.Success);
            Assert.IsType<GamepadChord>(result.Value.Chord);
            Assert.Equal("GamepadA", result.Value.ToString());
        }

        [Fact]
        public void GamepadChord_AddButton_WhenButtonAlreadyExists_DoesNotAddDuplicate()
        {
            var unityService = new UnityProvider();
            var existingButton = new GamepadTrigger(UnityEngine.InputSystem.LowLevel.GamepadButton.South, unityService);
            var chord = new GamepadChord(unityService, existingButton);

            chord.AddButton(UnityEngine.InputSystem.LowLevel.GamepadButton.South);

            var button = Assert.IsType<GamepadTrigger>(Assert.Single(chord.Buttons));
            Assert.Same(existingButton, button);
            Assert.Equal(UnityEngine.InputSystem.LowLevel.GamepadButton.South, button.Button);
        }

        [Fact]
        public void GamepadChord_AddButton_WhenButtonDoesNotExist_AddsNewGamepadTrigger()
        {
            var unityService = new UnityProvider();
            var chord = new GamepadChord(unityService);

            chord.AddButton(UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder);

            var button = Assert.IsType<GamepadTrigger>(Assert.Single(chord.Buttons));
            Assert.Equal(UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder, button.Button);
            Assert.Same(unityService, button.UnityService);
            Assert.True(chord.IsValid);
        }

        [Fact]
        public void GamepadChord_TryParse_WhenInputContainsOnlyWhitespaceParts_ReturnsFailure()
        {
            var result = GamepadChord.TryParse("  +   +  ", new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Failed to parse gamepad chord: empty input", result.Errors);
        }

        [Fact]
        public void GamepadChord_Clone_WhenButtonsExist_ClonesAllButtons()
        {
            var firstButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondButton = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var firstClone = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            var secondClone = new Mock<IHotkeyTrigger>(MockBehavior.Strict);
            firstButton.Setup(x => x.Clone()).Returns(firstClone.Object);
            secondButton.Setup(x => x.Clone()).Returns(secondClone.Object);
            var unityService = new UnityProvider();
            var chord = new GamepadChord(unityService, firstButton.Object, secondButton.Object);

            var clone = Assert.IsType<GamepadChord>(chord.Clone());

            Assert.NotSame(chord, clone);
            Assert.Same(unityService, clone.UnityService);
            Assert.Equal(2, clone.Buttons.Count);
            Assert.Same(firstClone.Object, clone.Buttons[0]);
            Assert.Same(secondClone.Object, clone.Buttons[1]);
            firstButton.Verify(x => x.Clone(), Times.Once);
            secondButton.Verify(x => x.Clone(), Times.Once);
        }

        [Fact]
        public void HotkeyChord_TryParse_WhenInputIsAmbiguous_ReturnsFailure()
        {
            var result = HotkeyChord.TryParse("A", new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Failed to parse hotkey chord: input can be parsed as both keyboard and gamepad chord, which is ambiguous.", result.Errors);
        }

        [Fact]
        public void HotkeyChord_IsPressed_WhenWrappedChordIsNotPressed_ReturnsFalse()
        {
            var wrappedChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            wrappedChord.SetupGet(x => x.IsValid).Returns(true);
            wrappedChord.Setup(x => x.IsPressed()).Returns(false);
            var chord = new HotkeyChord(wrappedChord.Object, new UnityProvider());

            var result = chord.IsPressed();

            Assert.False(result);
            wrappedChord.Verify(x => x.IsPressed(), Times.Once);
        }


    }
}