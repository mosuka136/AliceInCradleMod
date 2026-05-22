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
    }
}