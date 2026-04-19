using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using BetterExperience.HotkeyManager;

namespace BetterExperience.Test
{
    public class HotkeyTriggerTests
    {
        // Constructor tests
        [Fact]
        public void KeyboardTriggerConstructor_WithKey_SetsKeyProperty()
        {
            // Arrange
            var key = Key.A;

            // Act
            var trigger = new KeyboardTrigger(key);

            // Assert
            Assert.Equal(key, trigger.Key);
        }

        [Fact]
        public void KeyboardTriggerConstructor_WithDifferentKey_SetsKeyProperty()
        {
            // Arrange
            var key = Key.Space;

            // Act
            var trigger = new KeyboardTrigger(key);

            // Assert
            Assert.Equal(key, trigger.Key);
        }

        // IsPressed tests
        [Fact]
        public void KeyboardTriggerIsPressed_WhenKeyboardIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardTrigger(Key.A);

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardModifierTriggerIsPressed_WhenKeyboardIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true);

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTriggerIsPressed_WhenGamepadIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.South);

            // Act
            var result = trigger.IsPressed();

            // Assert
            Assert.False(result);
        }

        // WasPressedThisFrame tests
        [Fact]
        public void KeyboardTriggerWasPressedThisFrame_WhenKeyboardIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardTrigger(Key.B);

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardModifierTriggerWasPressedThisFrame_WhenKeyboardIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true);

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTriggerWasPressedThisFrame_WhenGamepadIsNull_ReturnsFalse()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.East);

            // Act
            var result = trigger.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        // TryParse tests - KeyboardTrigger
        [Theory]
        [InlineData("A", Key.A)]
        [InlineData("B", Key.B)]
        [InlineData("Space", Key.Space)]
        [InlineData("Enter", Key.Enter)]
        [InlineData("Escape", Key.Escape)]
        public void KeyboardTriggerTryParse_WithValidKeyName_ReturnsTrue(string input, Key expectedKey)
        {
            // Arrange
            var trigger = new KeyboardTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedKey, trigger.Key);
        }

        [Theory]
        [InlineData("a", Key.A)]
        [InlineData("b", Key.B)]
        [InlineData("space", Key.Space)]
        public void KeyboardTriggerTryParse_WithCaseInsensitiveKeyName_ReturnsTrue(string input, Key expectedKey)
        {
            // Arrange
            var trigger = new KeyboardTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedKey, trigger.Key);
        }

        [Theory]
        [InlineData("  A  ", Key.A)]
        [InlineData("\tSpace\t", Key.Space)]
        public void KeyboardTriggerTryParse_WithWhitespace_TrimsAndReturnsTrue(string input, Key expectedKey)
        {
            // Arrange
            var trigger = new KeyboardTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedKey, trigger.Key);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void KeyboardTriggerTryParse_WithNullOrWhitespace_ReturnsFalse(string input)
        {
            // Arrange
            var trigger = new KeyboardTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardTriggerTryParse_WithInvalidKeyName_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardTrigger();

            // Act
            var result = trigger.TryParse("InvalidKeyName");

            // Assert
            Assert.False(result);
        }

        // TryParse tests - KeyboardModifierTrigger
        [Theory]
        [InlineData("ctrl")]
        [InlineData("control")]
        [InlineData("Ctrl")]
        [InlineData("CONTROL")]
        public void KeyboardModifierTriggerTryParse_WithCtrlVariants_ReturnsTrue(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(Key.RightCtrl, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
        }

        [Theory]
        [InlineData("shift")]
        [InlineData("Shift")]
        [InlineData("SHIFT")]
        public void KeyboardModifierTriggerTryParse_WithShiftVariants_ReturnsTrue(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftShift, trigger.LeftKey);
            Assert.Equal(Key.RightShift, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
        }

        [Theory]
        [InlineData("alt")]
        [InlineData("Alt")]
        [InlineData("ALT")]
        public void KeyboardModifierTriggerTryParse_WithAltVariants_ReturnsTrue(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(Key.RightAlt, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
        }

        [Theory]
        [InlineData("leftctrl")]
        [InlineData("lctrl")]
        public void KeyboardModifierTriggerTryParse_WithLeftCtrl_ReturnsTrueAndSetsLeftSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("rightctrl")]
        [InlineData("rctrl")]
        public void KeyboardModifierTriggerTryParse_WithRightCtrl_ReturnsTrueAndSetsRightSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(Key.RightCtrl, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("leftshift")]
        [InlineData("lshift")]
        public void KeyboardModifierTriggerTryParse_WithLeftShift_ReturnsTrueAndSetsLeftSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftShift, trigger.LeftKey);
            Assert.Equal(Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("rightshift")]
        [InlineData("rshift")]
        public void KeyboardModifierTriggerTryParse_WithRightShift_ReturnsTrueAndSetsRightSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftShift, trigger.LeftKey);
            Assert.Equal(Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("leftalt")]
        [InlineData("lalt")]
        public void KeyboardModifierTriggerTryParse_WithLeftAlt_ReturnsTrueAndSetsLeftSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(Key.RightAlt, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData("rightalt")]
        [InlineData("ralt")]
        public void KeyboardModifierTriggerTryParse_WithRightAlt_ReturnsTrueAndSetsRightSide(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(Key.RightAlt, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void KeyboardModifierTriggerTryParse_WithNullOrWhitespace_ReturnsFalse(string input)
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void KeyboardModifierTriggerTryParse_WithInvalidInput_ReturnsFalse()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger();

            // Act
            var result = trigger.TryParse("InvalidModifier");

            // Assert
            Assert.False(result);
        }

        // TryParse tests - GamepadTrigger
        [Theory]
        [InlineData("GamepadA", GamepadButton.South)]
        [InlineData("GamepadSouth", GamepadButton.South)]
        [InlineData("GamepadCross", GamepadButton.South)]
        [InlineData("A", GamepadButton.South)]
        [InlineData("South", GamepadButton.South)]
        [InlineData("Cross", GamepadButton.South)]
        public void GamepadTriggerTryParse_WithSouthVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadB", GamepadButton.East)]
        [InlineData("GamepadEast", GamepadButton.East)]
        [InlineData("GamepadCircle", GamepadButton.East)]
        [InlineData("B", GamepadButton.East)]
        [InlineData("East", GamepadButton.East)]
        [InlineData("Circle", GamepadButton.East)]
        public void GamepadTriggerTryParse_WithEastVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadX", GamepadButton.West)]
        [InlineData("GamepadWest", GamepadButton.West)]
        [InlineData("GamepadSquare", GamepadButton.West)]
        [InlineData("X", GamepadButton.West)]
        [InlineData("West", GamepadButton.West)]
        [InlineData("Square", GamepadButton.West)]
        public void GamepadTriggerTryParse_WithWestVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadY", GamepadButton.North)]
        [InlineData("GamepadNorth", GamepadButton.North)]
        [InlineData("GamepadTriangle", GamepadButton.North)]
        [InlineData("Y", GamepadButton.North)]
        [InlineData("North", GamepadButton.North)]
        [InlineData("Triangle", GamepadButton.North)]
        public void GamepadTriggerTryParse_WithNorthVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadLB", GamepadButton.LeftShoulder)]
        [InlineData("GamepadLeftShoulder", GamepadButton.LeftShoulder)]
        [InlineData("LB", GamepadButton.LeftShoulder)]
        [InlineData("LeftShoulder", GamepadButton.LeftShoulder)]
        public void GamepadTriggerTryParse_WithLeftShoulderVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadRB", GamepadButton.RightShoulder)]
        [InlineData("GamepadRightShoulder", GamepadButton.RightShoulder)]
        [InlineData("RB", GamepadButton.RightShoulder)]
        [InlineData("RightShoulder", GamepadButton.RightShoulder)]
        public void GamepadTriggerTryParse_WithRightShoulderVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadBack", GamepadButton.Select)]
        [InlineData("GamepadSelect", GamepadButton.Select)]
        [InlineData("Back", GamepadButton.Select)]
        [InlineData("Select", GamepadButton.Select)]
        public void GamepadTriggerTryParse_WithSelectVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadStart", GamepadButton.Start)]
        [InlineData("Start", GamepadButton.Start)]
        public void GamepadTriggerTryParse_WithStartVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadLS", GamepadButton.LeftStick)]
        [InlineData("GamepadLeftStick", GamepadButton.LeftStick)]
        [InlineData("LS", GamepadButton.LeftStick)]
        [InlineData("LeftStick", GamepadButton.LeftStick)]
        public void GamepadTriggerTryParse_WithLeftStickVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadRS", GamepadButton.RightStick)]
        [InlineData("GamepadRightStick", GamepadButton.RightStick)]
        [InlineData("RS", GamepadButton.RightStick)]
        [InlineData("RightStick", GamepadButton.RightStick)]
        public void GamepadTriggerTryParse_WithRightStickVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadDpadUp", GamepadButton.DpadUp)]
        [InlineData("DpadUp", GamepadButton.DpadUp)]
        public void GamepadTriggerTryParse_WithDpadUpVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadDpadDown", GamepadButton.DpadDown)]
        [InlineData("DpadDown", GamepadButton.DpadDown)]
        public void GamepadTriggerTryParse_WithDpadDownVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadDpadLeft", GamepadButton.DpadLeft)]
        [InlineData("DpadLeft", GamepadButton.DpadLeft)]
        public void GamepadTriggerTryParse_WithDpadLeftVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadDpadRight", GamepadButton.DpadRight)]
        [InlineData("DpadRight", GamepadButton.DpadRight)]
        public void GamepadTriggerTryParse_WithDpadRightVariants_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData("GamepadLeftTrigger", GamepadButton.LeftTrigger)]
        [InlineData("LeftTrigger", GamepadButton.LeftTrigger)]
        public void GamepadTriggerTryParse_WithEnumName_ReturnsTrue(string input, GamepadButton expected)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, trigger.Button);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GamepadTriggerTryParse_WithNullOrWhitespace_ReturnsFalse(string input)
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GamepadTriggerTryParse_WithInvalidInput_ReturnsFalse()
        {
            // Arrange
            var trigger = new GamepadTrigger();

            // Act
            var result = trigger.TryParse("InvalidButton");

            // Assert
            Assert.False(result);
        }

        // ToString tests - KeyboardTrigger
        [Theory]
        [InlineData(Key.A, "A")]
        [InlineData(Key.B, "B")]
        [InlineData(Key.Space, "Space")]
        [InlineData(Key.Enter, "Enter")]
        public void KeyboardTriggerToString_ReturnsKeyName(Key key, string expected)
        {
            // Arrange
            var trigger = new KeyboardTrigger(key);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal(expected, result);
        }

        // ToString tests - KeyboardModifierTrigger
        [Fact]
        public void KeyboardModifierTriggerToString_WithCtrlAnySide_ReturnsCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("ctrl", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithCtrlLeftSide_ReturnsLeftCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, false, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("leftctrl", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithCtrlRightSide_ReturnsRightCtrl()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, false, false);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("rightctrl", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithShiftAnySide_ReturnsShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("shift", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithShiftLeftSide_ReturnsLeftShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, false, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("leftshift", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithShiftRightSide_ReturnsRightShift()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, false, false);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("rightshift", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithAltAnySide_ReturnsAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("alt", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithAltLeftSide_ReturnsLeftAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, false, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("leftalt", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithAltRightSide_ReturnsRightAlt()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, false, false);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("rightalt", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithOtherKeysLeftSide_ReturnsLeftKeyName()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.A, Key.B, false, true);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("A", result);
        }

        [Fact]
        public void KeyboardModifierTriggerToString_WithOtherKeysRightSide_ReturnsRightKeyName()
        {
            // Arrange
            var trigger = new KeyboardModifierTrigger(Key.A, Key.B, false, false);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("B", result);
        }

        // ToString tests - GamepadTrigger
        [Fact]
        public void GamepadTriggerToString_WithSouth_ReturnsGamepadA()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.South);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadA", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithEast_ReturnsGamepadB()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.East);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadB", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithWest_ReturnsGamepadX()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.West);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadX", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithNorth_ReturnsGamepadY()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.North);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadY", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithLeftShoulder_ReturnsGamepadLB()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.LeftShoulder);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadLB", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithRightShoulder_ReturnsGamepadRB()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.RightShoulder);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadRB", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithSelect_ReturnsGamepadBack()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.Select);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadBack", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithStart_ReturnsGamepadStart()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.Start);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadStart", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithLeftStick_ReturnsGamepadLS()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.LeftStick);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadLS", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithRightStick_ReturnsGamepadRS()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.RightStick);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadRS", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithDpadUp_ReturnsGamepadDpadUp()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.DpadUp);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadDpadUp", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithDpadDown_ReturnsGamepadDpadDown()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.DpadDown);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadDpadDown", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithDpadLeft_ReturnsGamepadDpadLeft()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.DpadLeft);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadDpadLeft", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithDpadRight_ReturnsGamepadDpadRight()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.DpadRight);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadDpadRight", result);
        }

        [Fact]
        public void GamepadTriggerToString_WithOtherButton_ReturnsGamepadPrefixedButtonName()
        {
            // Arrange
            var trigger = new GamepadTrigger(GamepadButton.LeftTrigger);

            // Act
            var result = trigger.ToString();

            // Assert
            Assert.Equal("GamepadLeftTrigger", result);
        }

        // KeyboardModifierTrigger Constructor Tests
        [Fact]
        public void KeyboardModifierTriggerConstructor_WithParameters_SetsAllProperties()
        {
            // Arrange & Act
            var trigger = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, false);

            // Assert
            Assert.Equal(Key.LeftCtrl, trigger.LeftKey);
            Assert.Equal(Key.RightCtrl, trigger.RightKey);
            Assert.True(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTriggerConstructor_WithDifferentKeys_SetsProperties()
        {
            // Arrange & Act
            var trigger = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, false, true);

            // Assert
            Assert.Equal(Key.LeftShift, trigger.LeftKey);
            Assert.Equal(Key.RightShift, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.True(trigger.IsLeftSide);
        }

        [Fact]
        public void KeyboardModifierTriggerConstructor_WithAllFalseFlags_SetsProperties()
        {
            // Arrange & Act
            var trigger = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, false, false);

            // Assert
            Assert.Equal(Key.LeftAlt, trigger.LeftKey);
            Assert.Equal(Key.RightAlt, trigger.RightKey);
            Assert.False(trigger.IsAnySide);
            Assert.False(trigger.IsLeftSide);
        }

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

        // CopyTo tests
        [Fact]
        public void CopyTo_WithAllProperties_CopiesAllValues()
        {
            // Arrange
            var source = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, false);
            var target = new KeyboardModifierTrigger();

            // Act
            source.CopyTo(target);

            // Assert
            Assert.Equal(Key.LeftCtrl, target.LeftKey);
            Assert.Equal(Key.RightCtrl, target.RightKey);
            Assert.True(target.IsAnySide);
            Assert.False(target.IsLeftSide);
        }

        [Fact]
        public void CopyTo_WithDifferentValues_OverwritesTarget()
        {
            // Arrange
            var source = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, false, true);
            var target = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, false);

            // Act
            source.CopyTo(target);

            // Assert
            Assert.Equal(Key.LeftShift, target.LeftKey);
            Assert.Equal(Key.RightShift, target.RightKey);
            Assert.False(target.IsAnySide);
            Assert.True(target.IsLeftSide);
        }

        [Fact]
        public void CopyTo_WithAllFalseFlags_CopiesCorrectly()
        {
            // Arrange
            var source = new KeyboardModifierTrigger(Key.A, Key.B, false, false);
            var target = new KeyboardModifierTrigger();

            // Act
            source.CopyTo(target);

            // Assert
            Assert.Equal(Key.A, target.LeftKey);
            Assert.Equal(Key.B, target.RightKey);
            Assert.False(target.IsAnySide);
            Assert.False(target.IsLeftSide);
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
    }
}
