using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyTriggerTests
    {
        [Fact]
        public void KeyboardModifierTrigger_EqualsL_WithMatchingEntry_ReturnsTrue()
        {
            var result = KeyboardModifierTrigger.EqualsL("CTRL", new List<string> { "ctrl", "control" });

            Assert.True(result);
        }

        [Fact]
        public void GamepadTrigger_EqualsL_WithNonMatchingEntry_ReturnsFalse()
        {
            var result = GamepadTrigger.EqualsL("B", new List<string> { "A", "South", "Cross" });

            Assert.False(result);
        }

        [Fact]
        public void KeyboardTrigger_Constructor_WithUnityService_SetsDefaults()
        {
            var unityService = new UnityProvider();

            var trigger = new KeyboardTrigger(unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(Key.None, trigger.Key);
        }

        [Fact]
        public void KeyboardTrigger_InputQueries_WhenKeyboardCurrentIsNull_ReturnFalse()
        {
            var trigger = new KeyboardTrigger(Key.Enter, new UnityProvider());

            Assert.False(trigger.IsPressed());
            Assert.False(trigger.WasPressedThisFrame());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void KeyboardTrigger_TryParse_WhenTokenIsInvalidInput_ReturnsFailure(string token)
        {
            var result = KeyboardTrigger.TryParse(token, new UnityProvider());

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void KeyboardTrigger_TryParse_WhenTokenIsValid_ReturnsParsedKey()
        {
            var result = KeyboardTrigger.TryParse("  enter  ", new UnityProvider());

            Assert.True(result.Success);
            Assert.Equal(Key.Enter, result.Value.Key);
            Assert.Equal("Enter", result.Value.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithLeftCtrl_SetsExpectedState()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, new UnityProvider());

            Assert.Equal(Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("control", Key.LeftCtrl, Key.RightCtrl, true, true, "Ctrl")]
        [InlineData("lctrl", Key.LeftCtrl, Key.RightCtrl, false, true, "LeftCtrl")]
        [InlineData("ralt", Key.LeftAlt, Key.RightAlt, false, false, "RightAlt")]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsAlias_ReturnsExpectedModifier(string token, Key leftKey, Key rightKey, bool isAnySide, bool isLeftSide, string expectedText)
        {
            var result = KeyboardModifierTrigger.TryParse(token, new UnityProvider());

            Assert.True(result.Success);
            Assert.Equal(leftKey, result.Value.LeftKey);
            Assert.Equal(rightKey, result.Value.RightKey);
            Assert.Equal(isAnySide, result.Value.IsAnySide);
            Assert.Equal(isLeftSide, result.Value.IsLeftSide);
            Assert.Equal(expectedText, result.Value.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsInvalid_ReturnsFailure()
        {
            var result = KeyboardModifierTrigger.TryParse("not-a-modifier", new UnityProvider());

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Theory]
        [InlineData(Key.LeftCtrl, true)]
        [InlineData(Key.RightCtrl, true)]
        [InlineData(Key.LeftShift, true)]
        [InlineData(Key.RightShift, true)]
        [InlineData(Key.LeftAlt, true)]
        [InlineData(Key.RightAlt, true)]
        [InlineData(Key.Enter, false)]
        [InlineData(Key.None, false)]
        public void KeyboardModifierTrigger_IsModifierKey_ReturnsExpectedValue(Key key, bool expected)
        {
            Assert.Equal(expected, KeyboardModifierTrigger.IsModifierKey(key));
        }

        [Fact]
        public void GamepadTrigger_Constructor_WithUnityService_SetsDefaultButton()
        {
            var unityService = new UnityProvider();

            var trigger = new GamepadTrigger(unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(GamepadButton.A, trigger.Button);
        }

        [Fact]
        public void GamepadTrigger_InputQueries_WhenGamepadCurrentIsNull_ReturnFalse()
        {
            var trigger = new GamepadTrigger(GamepadButton.South, new UnityProvider());

            Assert.False(trigger.IsPressed());
            Assert.False(trigger.WasPressedThisFrame());
        }

        [Theory]
        [InlineData("GamepadLB", GamepadButton.LeftShoulder, "GamepadLB")]
        [InlineData("south", GamepadButton.South, "GamepadA")]
        [InlineData("  gAmEpAd123  ", (GamepadButton)123, "Gamepad123")]
        public void GamepadTrigger_TryParse_WhenTokenIsSupported_ReturnsParsedButton(string token, GamepadButton expectedButton, string expectedText)
        {
            var result = GamepadTrigger.TryParse(token, new UnityProvider());

            Assert.True(result.Success);
            Assert.Equal(expectedButton, result.Value.Button);
            Assert.Equal(expectedText, result.Value.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("GamepadNope")]
        public void GamepadTrigger_TryParse_WhenTokenIsInvalid_ReturnsFailure(string token)
        {
            var result = GamepadTrigger.TryParse(token, new UnityProvider());

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }
    }
}