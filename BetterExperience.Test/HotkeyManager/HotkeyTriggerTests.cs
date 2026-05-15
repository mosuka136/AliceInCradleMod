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

        [Fact]
        public void KeyboardTrigger_Constructor_SetsUnityServiceAndDefaultsKeyToNone()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardTrigger(unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(UnityEngine.InputSystem.Key.None, trigger.Key);
        }

        [Fact]
        public void KeyboardTrigger_Constructor_WithNullUnityService_SetsUnityServiceToNull()
        {
            // Arrange
            global::BetterExperience.HProvider.UnityProvider unityService = null;

            // Act
            var trigger = new KeyboardTrigger(unityService);

            // Assert
            Assert.Null(trigger.UnityService);
            Assert.Equal(UnityEngine.InputSystem.Key.None, trigger.Key);
        }

        [Fact]
        public void KeyboardTrigger_IsPressed_WhenKeyboardCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardTrigger_WasPressedThisFrame_WhenKeyboardCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void KeyboardTrigger_TryParse_WhenTokenIsNullOrWhiteSpace_ReturnsFalse(string token)
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse(token);

            // Assert
            Assert.False(result);
            Assert.Equal(string.Empty, trigger.ToString());
        }

        [Fact]
        public void KeyboardTrigger_TryParse_WhenTokenMatchesKeyWithWhitespaceAndDifferentCase_ReturnsTrueAndSetsKey()
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("  enter  ");

            // Assert
            Assert.True(result);
            Assert.Equal("Enter", trigger.ToString());
        }

        [Fact]
        public void KeyboardTrigger_TryParse_WhenTokenIsInvalid_ReturnsFalseAndPreservesExistingKey()
        {
            // Arrange
            var trigger = new KeyboardTrigger(UnityEngine.InputSystem.Key.Space, new global::BetterExperience.HProvider.UnityProvider());
            var originalValue = trigger.ToString();

            // Act
            var result = trigger.TryParse("not-a-key");

            // Assert
            Assert.False(result);
            Assert.Equal(originalValue, trigger.ToString());
        }

        [Fact]
        public void KeyboardTrigger_ToString_WhenKeyIsNone_ReturnsEmptyString()
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardTrigger_ToString_WhenKeyIsSet_ReturnsKeyName()
        {
            // Arrange
            IHotkeyTrigger trigger = new KeyboardTrigger(UnityEngine.InputSystem.Key.Tab, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("Tab", result);
        }

        [Fact]
        public void KeyboardTrigger_Constructor_WithKeyAndUnityService_SetsProperties()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardTrigger(global::UnityEngine.InputSystem.Key.Enter, unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.Enter, trigger.Key);
        }

        [Fact]
        public void KeyboardTrigger_Constructor_WithKeyAndNullUnityService_SetsProperties()
        {
            // Arrange
            global::BetterExperience.HProvider.UnityProvider unityService = null;

            // Act
            var trigger = new KeyboardTrigger(global::UnityEngine.InputSystem.Key.Enter, unityService);

            // Assert
            Assert.Null(trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.Enter, trigger.Key);
        }




        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithLeftCtrl_SetsCtrlKeysAndLeftSide()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftCtrl, unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }
        [Fact]
        public void KeyboardModifierTrigger_Constructor_SetsUnityServiceAndDefaultModifierState()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardModifierTrigger(unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.None, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.None, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.RightCtrl, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightCtrl, global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.RightCtrl, false)]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightShift, global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, false)]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftAlt, global::UnityEngine.InputSystem.Key.LeftAlt, global::UnityEngine.InputSystem.Key.RightAlt, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightAlt, global::UnityEngine.InputSystem.Key.LeftAlt, global::UnityEngine.InputSystem.Key.RightAlt, false)]
        public void KeyboardModifierTrigger_Constructor_WithModifierKey_MapsToExpectedSide(global::UnityEngine.InputSystem.Key key, global::UnityEngine.InputSystem.Key expectedLeftKey, global::UnityEngine.InputSystem.Key expectedRightKey, bool expectedIsLeftSide)
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardModifierTrigger(key, unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(expectedLeftKey, trigger.LeftKey);
            Assert.Equal(expectedRightKey, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.Equal(expectedIsLeftSide, trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithNonModifierKey_SetsOnlyLeftKeyAndLeftSide()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.Tab, unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.Tab, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.None, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTrigger_Constructor_WithAllArguments_SetsProperties()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.A, global::UnityEngine.InputSystem.Key.B, false, false, unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.Key.A, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.B, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTrigger_IsPressed_WhenKeyboardCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.RightCtrl, true, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardModifierTrigger_WasPressedThisFrame_WhenKeyboardCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.RightCtrl, true, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }






        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsAltWithWhitespace_ReturnsTrueAndSetsAnySideAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("  alt  ");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightAlt, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("Alt", trigger.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsNullOrWhiteSpace_ReturnsFalse(string token)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.RightCtrl, true, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse(token);

            // Assert
            Assert.False(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightCtrl, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsCtrlAlias_ReturnsTrueAndSetsAnySideCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("control");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightCtrl, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("Ctrl", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsShiftAlias_ReturnsTrueAndSetsAnySideShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("shift");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftShift, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightShift, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("Shift", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsLeftCtrlAlias_ReturnsTrueAndSetsLeftCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("lctrl");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("LeftCtrl", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsRightCtrlAlias_ReturnsTrueAndSetsRightCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("rctrl");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
            Assert.Equal("RightCtrl", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsLeftShiftAlias_ReturnsTrueAndSetsLeftShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("leftshift");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftShift, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("LeftShift", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsRightShiftAlias_ReturnsTrueAndSetsRightShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("rshift");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftShift, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
            Assert.Equal("RightShift", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsLeftAltAlias_ReturnsTrueAndSetsLeftAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("leftalt");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightAlt, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
            Assert.Equal("LeftAlt", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsRightAltAlias_ReturnsTrueAndSetsRightAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("ralt");

            // Assert
            Assert.True(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightAlt, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
            Assert.Equal("RightAlt", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_TryParse_WhenTokenIsInvalid_ReturnsFalseAndPreservesState()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, false, false, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("not-a-modifier");

            // Assert
            Assert.False(result);
            Assert.Equal(global::UnityEngine.InputSystem.Key.LeftShift, trigger.LeftKey);
            Assert.Equal(global::UnityEngine.InputSystem.Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
            Assert.Equal("RightShift", trigger.ToString());
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenAnySideAndLeftKeyIsNone_ReturnsEmptyString()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.None, global::UnityEngine.InputSystem.Key.RightCtrl, true, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenLeftSideAndLeftKeyIsNone_ReturnsEmptyString()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.None, global::UnityEngine.InputSystem.Key.RightCtrl, false, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenRightSideAndRightKeyIsNone_ReturnsEmptyString()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftCtrl, global::UnityEngine.InputSystem.Key.None, false, false, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenShiftAnySide_ReturnsShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, true, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("Shift", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenShiftLeftSide_ReturnsLeftShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, false, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("LeftShift", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenShiftRightSide_ReturnsRightShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftShift, global::UnityEngine.InputSystem.Key.RightShift, false, false, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("RightShift", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenAltLeftSide_ReturnsLeftAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftAlt, global::UnityEngine.InputSystem.Key.RightAlt, false, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("LeftAlt", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenAltRightSide_ReturnsRightAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.LeftAlt, global::UnityEngine.InputSystem.Key.RightAlt, false, false, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("RightAlt", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenNonModifierAndLeftSide_ReturnsLeftKeyName()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.Tab, global::UnityEngine.InputSystem.Key.Enter, false, true, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("Tab", result);
        }

        [Fact]
        public void KeyboardModifierTrigger_ToString_WhenNonModifierAndRightSide_ReturnsRightKeyName()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(global::UnityEngine.InputSystem.Key.Tab, global::UnityEngine.InputSystem.Key.Enter, false, false, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("Enter", result);
        }

        [Theory]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftCtrl, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightCtrl, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftShift, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightShift, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.LeftAlt, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.RightAlt, true)]
        [InlineData(global::UnityEngine.InputSystem.Key.Enter, false)]
        [InlineData(global::UnityEngine.InputSystem.Key.None, false)]
        public void KeyboardModifierTrigger_IsModifierKey_WhenCalled_ReturnsExpectedResult(global::UnityEngine.InputSystem.Key key, bool expected)
        {
            // Arrange

            // Act
            var result = KeyboardModifierTrigger.IsModifierKey(key);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GamepadTrigger_Constructor_WithUnityService_SetsUnityServiceAndDefaultButton()
        {
            // Arrange
            var unityService = new global::BetterExperience.HProvider.UnityProvider();

            // Act
            var trigger = new GamepadTrigger(unityService);

            // Assert
            Assert.Same(unityService, trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.LowLevel.GamepadButton.A, trigger.Button);
        }

        [Fact]
        public void GamepadTrigger_Constructor_WithButtonAndNullUnityService_SetsProperties()
        {
            // Arrange
            global::BetterExperience.HProvider.UnityProvider unityService = null;

            // Act
            var trigger = new GamepadTrigger(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder, unityService);

            // Assert
            Assert.Null(trigger.UnityService);
            Assert.Equal(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder, trigger.Button);
        }



        [Fact]
        public void GamepadTrigger_TryParse_WhenTokenIsNullOrWhiteSpace_ReturnsFalseAndPreservesButton()
        {
            // Arrange
            var trigger = new GamepadTrigger(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse(" ");

            // Assert
            Assert.False(result);
            Assert.Equal(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder, trigger.Button);
        }

        [Fact]
        public void GamepadTrigger_IsPressed_WhenGamepadCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new GamepadTrigger(global::UnityEngine.InputSystem.LowLevel.GamepadButton.South, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTrigger_WasPressedThisFrame_WhenGamepadCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new GamepadTrigger(global::UnityEngine.InputSystem.LowLevel.GamepadButton.South, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTrigger_TryParse_WhenTokenHasPrefixAndNumericEnumValue_ReturnsTrueAndSetsParsedButton()
        {
            // Arrange
            var trigger = new GamepadTrigger(global::UnityEngine.InputSystem.LowLevel.GamepadButton.South, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.TryParse("  gAmEpAd123  ");

            // Assert
            Assert.True(result);
            Assert.Equal((global::UnityEngine.InputSystem.LowLevel.GamepadButton)123, trigger.Button);
        }

        [Theory]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.South, "GamepadA")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.North, "GamepadY")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.West, "GamepadX")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.East, "GamepadB")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.LeftShoulder, "GamepadLB")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightShoulder, "GamepadRB")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.Select, "GamepadBack")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.Start, "GamepadStart")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.LeftStick, "GamepadLS")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.RightStick, "GamepadRS")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.DpadUp, "GamepadDpadUp")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.DpadDown, "GamepadDpadDown")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.DpadLeft, "GamepadDpadLeft")]
        [InlineData(global::UnityEngine.InputSystem.LowLevel.GamepadButton.DpadRight, "GamepadDpadRight")]
        public void GamepadTrigger_ToString_WhenButtonHasKnownAlias_ReturnsPrefixedAlias(global::UnityEngine.InputSystem.LowLevel.GamepadButton button, string expected)
        {
            // Arrange
            var trigger = new GamepadTrigger(button, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GamepadTrigger_ToString_WhenButtonDoesNotHaveKnownAlias_ReturnsPrefixedEnumString()
        {
            // Arrange
            var trigger = new GamepadTrigger((global::UnityEngine.InputSystem.LowLevel.GamepadButton)123, new global::BetterExperience.HProvider.UnityProvider());

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("Gamepad123", result);
        }


    }
}
