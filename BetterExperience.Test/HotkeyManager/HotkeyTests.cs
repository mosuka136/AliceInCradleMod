using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using Moq;

namespace BetterExperience.Test.HotkeyManager
{
    public class HotkeyTests
    {
        [Fact]
        public void Constructor_Default_InitializesEmptyHotkeysList()
        {
            var hotkey = new Hotkey();

            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void Constructor_Default_UsesSharedDefaultUnityProvider()
        {
            var firstHotkey = new Hotkey();
            var secondHotkey = new Hotkey();

            Assert.NotNull(firstHotkey.UnityService);
            Assert.Same(firstHotkey.UnityService, secondHotkey.UnityService);
        }

        [Fact]
        public void Constructor_WithHotkeyStringAndUnityProvider_ValidStringParsesHotkeys()
        {
            var unityService = new UnityProvider();

            var hotkey = new Hotkey("Ctrl+A", unityService);

            Assert.Same(unityService, hotkey.UnityService);
            Assert.Single(hotkey.Hotkeys);
            Assert.Equal("Ctrl+A", hotkey.ToString());
        }

        [Fact]
        public void Constructor_WithHotkeyStringAndUnityProvider_InvalidStringThrowsArgumentException()
        {
            var unityService = new UnityProvider();

            var exception = Assert.Throws<ArgumentException>(() => new Hotkey("InvalidKey", unityService));

            Assert.Equal("Invalid hotkey string: InvalidKey", exception.Message);
        }

        [Fact]
        public void Constructor_WithSourceHotkeyAndUnityProvider_ClonesChordList()
        {
            var source = new Hotkey("Ctrl+A,Shift+B", new UnityProvider());

            var cloned = new Hotkey(source, new UnityProvider());

            Assert.Equal(source.ToString(), cloned.ToString());
            Assert.Equal(source.Count, cloned.Count);
            Assert.NotSame(source.Hotkeys, cloned.Hotkeys);
            Assert.NotSame(source.Hotkeys[0], cloned.Hotkeys[0]);
            Assert.NotSame(source.Hotkeys[1], cloned.Hotkeys[1]);
        }

        [Fact]
        public void Count_WithEmptyHotkeys_ReturnsZero()
        {
            var hotkey = new Hotkey();

            Assert.Equal(0, hotkey.Count);
        }

        [Fact]
        public void TryParse_WithWhitespaceChord_SkipsWhitespaceChord()
        {
            var hotkey = new Hotkey();

            var result = hotkey.TryParse("Ctrl+A,   ,Shift+B");

            Assert.True(result);
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Equal("Ctrl+A,Shift+B", hotkey.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidKey")]
        public void TryParse_WhenInputInvalid_ReturnsFalse(string text)
        {
            var hotkey = new Hotkey();

            var result = hotkey.TryParse(text);

            Assert.False(result);
        }

        [Fact]
        public void WasPressedThisFrame_WhenHotkeyInvalid_ReturnsFalse()
        {
            var innerChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            var hotkey = new Hotkey(new UnityProvider(), new HotkeyChord(innerChord.Object, new UnityProvider()));
            hotkey.Valid = false;

            var result = hotkey.WasPressedThisFrame();

            Assert.False(result);
            innerChord.Verify(x => x.WasPressedThisFrame(), Times.Never);
        }

        [Fact]
        public void WasPressedThisFrame_WhenSecondChordPressed_ReturnsTrue()
        {
            var unityService = new UnityProvider();
            var firstChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            var secondChord = new Mock<IHotkeyChord>(MockBehavior.Strict);
            firstChord.Setup(x => x.WasPressedThisFrame()).Returns(false);
            secondChord.Setup(x => x.WasPressedThisFrame()).Returns(true);

            var hotkey = new Hotkey(
                unityService,
                new HotkeyChord(firstChord.Object, unityService),
                new HotkeyChord(secondChord.Object, unityService));

            var result = hotkey.WasPressedThisFrame();

            Assert.True(result);
            firstChord.Verify(x => x.WasPressedThisFrame(), Times.Once);
            secondChord.Verify(x => x.WasPressedThisFrame(), Times.Once);
        }

        [Fact]
        public void Add_NullChord_DoesNotAdd()
        {
            var hotkey = new Hotkey();

            hotkey.Add(null);

            Assert.Empty(hotkey.Hotkeys);
        }

        [Fact]
        public void Add_ExistingChord_DoesNotAddDuplicateReference()
        {
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(new Mock<IHotkeyChord>().Object, unityService);
            var hotkey = new Hotkey(unityService, chord);

            hotkey.Add(chord);

            Assert.Single(hotkey.Hotkeys);
        }

        [Fact]
        public void Remove_NullChord_DoesNothing()
        {
            var unityService = new UnityProvider();
            var chord = new HotkeyChord(new Mock<IHotkeyChord>().Object, unityService);
            var hotkey = new Hotkey(unityService, chord);

            hotkey.Remove(null);

            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
        }

        [Fact]
        public void RemoveInvalidHotkey_RemovesNullAndInvalidEntries()
        {
            var unityService = new UnityProvider();
            var validResult = HotkeyChord.TryParse("Ctrl+A", unityService);
            Assert.True(validResult.Success);

            var hotkey = new Hotkey(unityService);
            hotkey.Hotkeys.Add(null);
            hotkey.Hotkeys.Add(new HotkeyChord(unityService));
            hotkey.Hotkeys.Add(validResult.Value);

            hotkey.RemoveInvalidHotkey();

            Assert.Single(hotkey.Hotkeys);
            Assert.Equal("Ctrl+A", hotkey.Hotkeys[0].ToString());
        }

        [Fact]
        public void HasSameHotkey_WhenOrderDiffers_ReturnsTrue()
        {
            var first = new Hotkey("Ctrl+A,Shift+B", new UnityProvider());
            var second = new Hotkey("Shift+B,Ctrl+A", new UnityProvider());

            var result = first.HasSameHotkey(second);

            Assert.True(result);
        }

        [Fact]
        public void HasSameHotkey_WhenOtherIsNull_ReturnsFalse()
        {
            var hotkey = new Hotkey();

            Assert.False(hotkey.HasSameHotkey(null));
        }

        [Fact]
        public void Decode_ValidContent_ReturnsSuccessWithSelf()
        {
            var hotkey = new Hotkey();

            var result = hotkey.Decode("Ctrl+A");

            Assert.True(result.Success);
            Assert.Same(hotkey, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("InvalidKey")]
        public void Decode_InvalidContent_ReturnsFailure(string content)
        {
            var hotkey = new Hotkey();

            var result = hotkey.Decode(content);

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Constructor_WithUnityProvider_UsesProvidedUnityServiceAndInitializesEmptyHotkeysList()
        {
            var unityService = new UnityProvider();

            var hotkey = new Hotkey(unityService);

            Assert.Same(unityService, hotkey.UnityService);
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }

        [Theory]
        [InlineData(null, "Invalid hotkey string: ")]
        [InlineData("", "Invalid hotkey string: ")]
        [InlineData("   ", "Invalid hotkey string:    ")]
        public void Constructor_WithHotkeyStringAndUnityProvider_EmptyOrWhitespaceStringThrowsArgumentException(string hotkeyText, string expectedMessage)
        {
            var unityService = new UnityProvider();

            var exception = Assert.Throws<ArgumentException>(() => new Hotkey(hotkeyText, unityService));

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithSourceHotkeyAndUnityProvider_UsesProvidedUnityService()
        {
            var sourceUnityService = new UnityProvider();
            var targetUnityService = new UnityProvider();
            var source = new Hotkey("Ctrl+A", sourceUnityService);

            var cloned = new Hotkey(source, targetUnityService);

            Assert.Same(targetUnityService, cloned.UnityService);
            Assert.Single(cloned.Hotkeys);
        }

        [Fact]
        public void Constructor_WithSourceHotkeyAndUnityProvider_ChangingSourceDoesNotAffectClone()
        {
            var source = new Hotkey("Ctrl+A", new UnityProvider());
            var cloned = new Hotkey(source, new UnityProvider());

            source.Hotkeys[0].Clear();

            Assert.Equal("Ctrl+A", cloned.ToString());
            Assert.Empty(source.ToString());
        }

        [Fact]
        public void Constructor_WithUnityProviderAndHotkeys_UsesProvidedUnityServiceAndCopiesHotkeyReferencesIntoNewList()
        {
            var unityService = new UnityProvider();
            var firstChord = HotkeyChord.TryParse("Ctrl+A", unityService).Value;
            var secondChord = HotkeyChord.TryParse("Shift+B", unityService).Value;

            var hotkey = new Hotkey(unityService, firstChord, secondChord);

            Assert.Same(unityService, hotkey.UnityService);
            Assert.Equal(2, hotkey.Hotkeys.Count);
            Assert.Same(firstChord, hotkey.Hotkeys[0]);
            Assert.Same(secondChord, hotkey.Hotkeys[1]);
        }

        [Fact]
        public void Constructor_WithUnityProviderAndNoHotkeys_InitializesEmptyHotkeysList()
        {
            var unityService = new UnityProvider();

            var hotkey = new Hotkey(unityService, new HotkeyChord[0]);

            Assert.Same(unityService, hotkey.UnityService);
            Assert.NotNull(hotkey.Hotkeys);
            Assert.Empty(hotkey.Hotkeys);
        }


        [Fact]
        public void Add_NewChord_AddsChordToHotkeys()
        {
            var unityService = new UnityProvider();
            var chord = HotkeyChord.TryParse("Ctrl+A", unityService).Value;
            var hotkey = new Hotkey(unityService);

            hotkey.Add(chord);

            Assert.Single(hotkey.Hotkeys);
            Assert.Same(chord, hotkey.Hotkeys[0]);
        }

        [Fact]
        public void Remove_ExistingChord_RemovesChordFromHotkeys()
        {
            var unityService = new UnityProvider();
            var chord = HotkeyChord.TryParse("Ctrl+A", unityService).Value;
            var hotkey = new Hotkey(unityService, chord);

            hotkey.Remove(chord);

            Assert.Empty(hotkey.Hotkeys);
        }



    }
}