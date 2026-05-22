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
        [Fact]
        public void KeyboardTrigger_ToString_WhenKeyIsNone_ReturnsEmptyString()
        {
            var trigger = new KeyboardTrigger(new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardTrigger_Clone_WhenCalled_ReturnsNewTriggerWithSameKeyAndUnityService()
        {
            var unityService = new UnityProvider();
            var trigger = new KeyboardTrigger(Key.Tab, unityService);

            var clone = trigger.Clone();

            var typedClone = Assert.IsType<KeyboardTrigger>(clone);
            Assert.NotSame(trigger, typedClone);
            Assert.Equal(Key.Tab, typedClone.Key);
            Assert.Same(unityService, typedClone.UnityService);
        }

        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithUnityService_SetsDefaults()
        {
            var unityService = new UnityProvider();

            var trigger = new KeyboardModifierTrigger(unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(Key.None, trigger.LeftKey);
            Assert.Equal(Key.None, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardTrigger_TryParse_WhenTokenIsNone_ReturnsFailure()
        {
            var result = KeyboardTrigger.TryParse("None", new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Parsed key is None.", result.Errors);
        }

        [Fact]
        public void KeyboardTrigger_TryParse_WhenTokenIsNotAKey_ReturnsFailureWithTrimmedToken()
        {
            var result = KeyboardTrigger.TryParse("  not-a-key  ", new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Failed to parse token 'not-a-key' as a Key.", result.Errors);
        }

        [Theory]
        [InlineData(Key.LeftCtrl, Key.LeftCtrl, Key.RightCtrl, true)]
        [InlineData(Key.RightCtrl, Key.LeftCtrl, Key.RightCtrl, false)]
        [InlineData(Key.LeftShift, Key.LeftShift, Key.RightShift, true)]
        [InlineData(Key.RightShift, Key.LeftShift, Key.RightShift, false)]
        [InlineData(Key.LeftAlt, Key.LeftAlt, Key.RightAlt, true)]
        [InlineData(Key.RightAlt, Key.LeftAlt, Key.RightAlt, false)]
        public void KeyboardModifierTrigger_Constructor_WithModifierKey_SetsExpectedState(Key key, Key expectedLeftKey, Key expectedRightKey, bool expectedIsLeftSide)
        {
            var unityService = new UnityProvider();

            var trigger = new KeyboardModifierTrigger(key, unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(expectedLeftKey, trigger.LeftKey);
            Assert.Equal(expectedRightKey, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.Equal(expectedIsLeftSide, trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithNonModifierKey_SetsLeftKeyOnly()
        {
            var unityService = new UnityProvider();

            var trigger = new KeyboardModifierTrigger(Key.Enter, unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(Key.Enter, trigger.LeftKey);
            Assert.Equal(Key.None, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }
        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithExplicitState_SetsAllProperties()
        {
            var unityService = new UnityProvider();

            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, false, unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(Key.RightAlt, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }
        [Fact]
        public void KeyboardModifierTrigger_IsPressed_WhenUnityServiceIsNull_ReturnsFalse()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true, null);

            var result = trigger.IsPressed();

            Assert.False(result);
        }

        [Fact]
        public void KeyboardModifierTrigger_WasPressedThisFrame_WhenUnityServiceIsNull_ReturnsFalse()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true, null);

            var result = trigger.WasPressedThisFrame();

            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsWhitespace_ReturnsFailure(string token)
        {
            var result = KeyboardModifierTrigger.TryParse(token, new UnityProvider());

            Assert.False(result.Success);
            Assert.Contains("Token is null or whitespace.", result.Errors);
        }

        [Theory]
        [InlineData("  alt  ", Key.LeftAlt, Key.RightAlt, true, true, "Alt")]
        [InlineData("rctrl", Key.LeftCtrl, Key.RightCtrl, false, false, "RightCtrl")]
        [InlineData("RShift", Key.LeftShift, Key.RightShift, false, false, "RightShift")]
        [InlineData("lalt", Key.LeftAlt, Key.RightAlt, false, true, "LeftAlt")]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsAdditionalAlias_ReturnsExpectedModifier(string token, Key leftKey, Key rightKey, bool isAnySide, bool isLeftSide, string expectedText)
        {
            var unityService = new UnityProvider();

            var result = KeyboardModifierTrigger.TryParse(token, unityService);

            Assert.True(result.Success);
            Assert.Same(unityService, result.Value.UnityService);
            Assert.Equal(leftKey, result.Value.LeftKey);
            Assert.Equal(rightKey, result.Value.RightKey);
            Assert.Equal(isAnySide, result.Value.IsAnySide);
            Assert.Equal(isLeftSide, result.Value.IsLeftSide);
            Assert.Equal(expectedText, result.Value.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenAnySideAndLeftOrRightKeyIsNone_ReturnsEmptyString()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.None, true, true, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenLeftSideSelectedAndLeftKeyIsNone_ReturnsEmptyString()
        {
            var trigger = new KeyboardModifierTrigger(Key.None, Key.RightCtrl, false, true, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenRightSideSelectedAndRightKeyIsNone_ReturnsEmptyString()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.None, false, false, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenAnySideAltModifier_ReturnsAltAlias()
        {
            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, false, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal("Alt", result);
        }

        [Theory]
        [InlineData(true, true, Key.Enter, Key.Space, "Enter")]
        [InlineData(false, false, Key.Enter, Key.Space, "Space")]
        public void KeyboardModifierTrigger_ToString_WhenModifierIsCustomKey_ReturnsSelectedKeyName(bool isAnySide, bool isLeftSide, Key leftKey, Key rightKey, string expected)
        {
            var trigger = new KeyboardModifierTrigger(leftKey, rightKey, isAnySide, isLeftSide, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(GamepadButton.North, "GamepadY")]
        [InlineData(GamepadButton.West, "GamepadX")]
        [InlineData(GamepadButton.East, "GamepadB")]
        [InlineData(GamepadButton.RightShoulder, "GamepadRB")]
        [InlineData(GamepadButton.Select, "GamepadBack")]
        [InlineData(GamepadButton.Start, "GamepadStart")]
        [InlineData(GamepadButton.LeftStick, "GamepadLS")]
        [InlineData(GamepadButton.RightStick, "GamepadRS")]
        [InlineData(GamepadButton.DpadUp, "GamepadDpadUp")]
        [InlineData(GamepadButton.DpadDown, "GamepadDpadDown")]
        [InlineData(GamepadButton.DpadLeft, "GamepadDpadLeft")]
        [InlineData(GamepadButton.DpadRight, "GamepadDpadRight")]
        public void GamepadTrigger_ToString_WhenButtonHasAlias_ReturnsExpectedAlias(GamepadButton button, string expected)
        {
            var trigger = new GamepadTrigger(button, new UnityProvider());

            var result = trigger.ToString();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GamepadTrigger_Constructor_WithExplicitButton_SetsButtonAndUnityService()
        {
            var unityService = new UnityProvider();

            var trigger = new GamepadTrigger(GamepadButton.RightShoulder, unityService);

            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(GamepadButton.RightShoulder, trigger.Button);
        }

        [Fact]
        public void GamepadTrigger_Clone_WhenCalled_ReturnsNewTriggerWithSameButtonAndUnityService()
        {
            var unityService = new UnityProvider();
            var trigger = new GamepadTrigger(GamepadButton.DpadLeft, unityService);

            var clone = trigger.Clone();

            var typedClone = Assert.IsType<GamepadTrigger>(clone);
            Assert.NotSame(trigger, typedClone);
            Assert.Equal(GamepadButton.DpadLeft, typedClone.Button);
            Assert.Same(unityService, typedClone.UnityService);
        }










    }
}