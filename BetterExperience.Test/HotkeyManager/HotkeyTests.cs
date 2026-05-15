using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using UnityEngine.InputSystem;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyTests
    {
        // -----------------------------------------------------------------------
        // Default Constructor - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_Default_InitializesEmptyHotkeysList()
        {
            // Arrange & Act
            var hotkey = new Hotkey();

            // Assert
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // Count Property - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Count_WithEmptyHotkeys_ReturnsZero()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var count = hotkey.Count;

            // Assert
            Assert.Equal(0, count);
        }

        // -----------------------------------------------------------------------
        // WasPressedThisFrame() - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void WasPressedThisFrame_NoHotkeys_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // TryParse(string) - Whitespace handling tests
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParse_WithWhitespaceChord_SkipsWhitespaceChord()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("Ctrl+A,   ,Shift+B");

            // Assert
            Assert.True(result);
            Assert.Equal(2, hotkey.Hotkeys.Count);
        }

        [Fact]
        public void TryParse_InvalidChord_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("InvalidKey");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Add_NullChord_DoesNotAdd()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            hotkey.Add(null);

            // Assert
            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void RemoveInvalidHotkey_EmptyList_DoesNothing()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            hotkey.RemoveInvalidHotkey();

            // Assert
            Assert.Empty(hotkey.Hotkeys);
        }

        // -----------------------------------------------------------------------
        // HasSameHotkey() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void HasSameHotkey_OtherIsNull_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.HasSameHotkey(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasSameHotkey_BothEmpty_ReturnsTrue()
        {
            // Arrange
            var hotkey1 = new Hotkey();
            var hotkey2 = new Hotkey();

            // Act
            var result = hotkey1.HasSameHotkey(hotkey2);

            // Assert
            Assert.True(result);
        }

        // -----------------------------------------------------------------------
        // TryParse(string) - Additional tests for uncovered lines
        // -----------------------------------------------------------------------

        [Fact]
        public void TryParse_NullString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_EmptyString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryParse_WhitespaceString_ReturnsFalse()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.TryParse("   ");

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // Decode() - Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Decode_ValidContent_ReturnsSuccessWithSelf()
        {
            // Arrange
            var hotkey = new Hotkey();
            var content = "Ctrl+A";

            // Act
            var result = hotkey.Decode(content);

            // Assert
            Assert.True(result.Success);
            Assert.Same(hotkey, result.Value);
        }

        [Fact]
        public void Decode_InvalidContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();
            var content = "InvalidKey";

            // Act
            var result = hotkey.Decode(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Decode_NullContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.Decode(null);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Decode_EmptyContent_ReturnsFailure()
        {
            // Arrange
            var hotkey = new Hotkey();

            // Act
            var result = hotkey.Decode("");

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Constructor_WithUnityProvider_InitializesEmptyHotkeysList()
        {
            // Arrange
            var unityService = new HProvider.UnityProvider();

            // Act
            var hotkey = new Hotkey(unityService);

            // Assert
            Assert.Same(unityService, hotkey.UnityService);
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void Constructor_WithHotkeyStringAndUnityProvider_ValidStringParsesHotkeys()
        {
            // Arrange
            var unityService = new UnityProvider();

            // Act
            var hotkey = new Hotkey("Ctrl+A", unityService);

            // Assert
            Assert.Same(unityService, hotkey.UnityService);
            Assert.Single(hotkey.Hotkeys);
            Assert.Equal("Ctrl+A", hotkey.Hotkeys[0].ToString());
        }

        [Fact]
        public void Constructor_WithHotkeyStringAndUnityProvider_InvalidStringThrowsArgumentException()
        {
            // Arrange
            var unityService = new UnityProvider();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => new Hotkey("InvalidKey", unityService));

            // Assert
            Assert.Equal("Invalid hotkey string: InvalidKey", exception.Message);
        }

        [Fact]
        public void Constructor_WithSourceHotkeyAndUnityProvider_CreatesDeepCopiedChordsWithNewUnityService()
        {
            // Arrange
            var sourceUnityService = new UnityProvider();
            var targetUnityService = new UnityProvider();
            var sourceHotkey = new Hotkey(
                sourceUnityService,
                new HotkeyChord(
                    sourceUnityService,
                    new KeyboardTrigger(Key.A, sourceUnityService),
                    new KeyboardModifierTrigger(Key.LeftCtrl, sourceUnityService)),
                new HotkeyChord(
                    sourceUnityService,
                    new KeyboardTrigger(Key.B, sourceUnityService),
                    new KeyboardModifierTrigger(Key.LeftShift, sourceUnityService)));

            // Act
            var clonedHotkey = new Hotkey(sourceHotkey, targetUnityService);

            // Assert
            Assert.Same(targetUnityService, clonedHotkey.UnityService);
            Assert.Equal(sourceHotkey.Hotkeys.Count, clonedHotkey.Hotkeys.Count);
            Assert.NotSame(sourceHotkey.Hotkeys, clonedHotkey.Hotkeys);
            Assert.Equal(sourceHotkey.Hotkeys[0].ToString(), clonedHotkey.Hotkeys[0].ToString());
            Assert.Equal(sourceHotkey.Hotkeys[1].ToString(), clonedHotkey.Hotkeys[1].ToString());
            Assert.NotSame(sourceHotkey.Hotkeys[0], clonedHotkey.Hotkeys[0]);
            Assert.NotSame(sourceHotkey.Hotkeys[1], clonedHotkey.Hotkeys[1]);
            Assert.Same(targetUnityService, clonedHotkey.Hotkeys[0].UnityService);
            Assert.Same(targetUnityService, clonedHotkey.Hotkeys[1].UnityService);
            Assert.Same(sourceHotkey.Hotkeys[0].MainKey, clonedHotkey.Hotkeys[0].MainKey);
            Assert.Same(sourceHotkey.Hotkeys[1].MainKey, clonedHotkey.Hotkeys[1].MainKey);
            Assert.Same(sourceHotkey.Hotkeys[0].Modifiers[0], clonedHotkey.Hotkeys[0].Modifiers[0]);
            Assert.Same(sourceHotkey.Hotkeys[1].Modifiers[0], clonedHotkey.Hotkeys[1].Modifiers[0]);
        }

        [Fact]
        public void Constructor_WithUnityProviderAndHotkeyChords_StoresProvidedChordsInList()
        {
            // Arrange
            var unityService = new UnityProvider();
            var firstChord = new HotkeyChord(
                unityService,
                new KeyboardTrigger(Key.A, unityService),
                new KeyboardModifierTrigger(Key.LeftCtrl, unityService));
            var secondChord = new HotkeyChord(
                unityService,
                new KeyboardTrigger(Key.B, unityService),
                new KeyboardModifierTrigger(Key.LeftShift, unityService));

            // Act
            var hotkey = new Hotkey(unityService, firstChord, secondChord);

            // Assert
            Assert.Same(unityService, hotkey.UnityService);
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Same(firstChord, hotkey.Hotkeys[0]);
            Assert.Same(secondChord, hotkey.Hotkeys[1]);
        }

        [Fact]
        public void Constructor_Default_UsesSharedDefaultUnityProvider()
        {
            // Arrange & Act
            var firstHotkey = new Hotkey();
            var secondHotkey = new Hotkey();

            // Assert
            Assert.NotNull(firstHotkey.UnityService);
            Assert.Same(firstHotkey.UnityService, secondHotkey.UnityService);
            Assert.Empty(firstHotkey.Hotkeys);
            Assert.Empty(secondHotkey.Hotkeys);
        }



        [Fact]
        public void WasPressedThisFrame_InvalidHotkey_ReturnsFalse()
        {
            // Arrange
            var unityService = new UnityProvider();
            var mainTrigger = new global::Moq.Mock<IHotkeyTrigger>();
            var hotkey = new Hotkey(unityService, mainTrigger.Object);
            hotkey.Valid = false;

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Constructor_WithUnityProviderMainAndModifiers_CreatesSingleChord()
        {
            // Arrange
            var unityService = new UnityProvider();
            var mainTrigger = new global::Moq.Mock<IHotkeyTrigger>();
            var modifier1 = new global::Moq.Mock<IHotkeyTrigger>();
            var modifier2 = new global::Moq.Mock<IHotkeyTrigger>();

            // Act
            var hotkey = new Hotkey(unityService, mainTrigger.Object, modifier1.Object, modifier2.Object);

            // Assert
            Assert.Same(unityService, hotkey.UnityService);
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(unityService, hotkey.Hotkeys[0].UnityService);
            Assert.Same(mainTrigger.Object, hotkey.Hotkeys[0].MainKey);
            Assert.Equal(2, hotkey.Hotkeys[0].Modifiers.Count);
            Assert.Same(modifier1.Object, hotkey.Hotkeys[0].Modifiers[0]);
            Assert.Same(modifier2.Object, hotkey.Hotkeys[0].Modifiers[1]);
        }

        [Fact]
        public void WasPressedThisFrame_ChordNotPressed_ReturnsFalse()
        {
            // Arrange
            var unityService = new UnityProvider();
            var mainTrigger = new global::Moq.Mock<IHotkeyTrigger>();
            mainTrigger.Setup(t => t.WasPressedThisFrame()).Returns(false);
            var hotkey = new Hotkey(unityService, mainTrigger.Object);

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.False(result);
            mainTrigger.Verify(t => t.WasPressedThisFrame(), global::Moq.Times.Once);
        }

        [Fact]
        public void WasPressedThisFrame_SecondChordPressed_ReturnsTrue()
        {
            // Arrange
            var unityService = new UnityProvider();
            var firstMainTrigger = new global::Moq.Mock<IHotkeyTrigger>();
            firstMainTrigger.Setup(t => t.WasPressedThisFrame()).Returns(false);
            var secondMainTrigger = new global::Moq.Mock<IHotkeyTrigger>();
            secondMainTrigger.Setup(t => t.WasPressedThisFrame()).Returns(true);
            var hotkey = new Hotkey();
            hotkey.UnityService.Equals(unityService);
            hotkey.Hotkeys.Add(new HotkeyChord(unityService, firstMainTrigger.Object));
            hotkey.Hotkeys.Add(new HotkeyChord(unityService, secondMainTrigger.Object));

            // Act
            var result = hotkey.WasPressedThisFrame();

            // Assert
            Assert.True(result);
            firstMainTrigger.Verify(t => t.WasPressedThisFrame(), global::Moq.Times.Once);
            secondMainTrigger.Verify(t => t.WasPressedThisFrame(), global::Moq.Times.Once);
        }

        [Fact]
        public void Add_NewChord_AddsChordToCollection()
        {
            // Arrange
            var unityService = new UnityProvider();
            var hotkey = new Hotkey();
            var chord = new HotkeyChord(unityService, new global::Moq.Mock<IHotkeyTrigger>().Object);

            // Act
            hotkey.Add(chord);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
        }

        [Fact]
        public void Add_ExistingChord_DoesNotAddDuplicate()
        {
            // Arrange
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(unityService, new global::Moq.Mock<IHotkeyTrigger>().Object);
            var hotkey = new Hotkey(unityService, chord);

            // Act
            hotkey.Add(chord);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
        }

        [Fact]
        public void Remove_ExistingChord_RemovesChordFromCollection()
        {
            // Arrange
            var unityService = new UnityProvider();
            var firstChord = new HotkeyChord(unityService, new global::Moq.Mock<IHotkeyTrigger>().Object);
            var secondChord = new HotkeyChord(unityService, new global::Moq.Mock<IHotkeyTrigger>().Object);
            var hotkey = new Hotkey(unityService, firstChord, secondChord);

            // Act
            hotkey.Remove(firstChord);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(secondChord, hotkey.Hotkeys[0]);
        }

        [Fact]
        public void Remove_NullChord_DoesNothing()
        {
            // Arrange
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(unityService, new global::Moq.Mock<IHotkeyTrigger>().Object);
            var hotkey = new Hotkey(unityService, chord);

            // Act
            hotkey.Remove(null);

            // Assert
            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
        }



    }
}
