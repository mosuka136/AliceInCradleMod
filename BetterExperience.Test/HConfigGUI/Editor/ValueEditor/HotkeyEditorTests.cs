using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using Moq;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterExperience.Test.HConfigGUI.Editor.ValueEditor
{
    public class HotkeyEditorTests
    {
        [Fact]
        public void HotkeyEditor_WhenConstructed_InitializesSessionAndDependencies()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var styleResource = new StyleResource(unityGuiMock.Object);

            // Act
            var editor = new HotkeyEditor(unityGuiMock.Object, styleResource);

            // Assert
            Assert.NotNull(editor.Session);
            Assert.IsType<HotkeyEditSession>(editor.Session);
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(styleResource, editor.StyleProvider);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsHotkey_ReturnsTrue()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(Hotkey));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.True(result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsNotHotkey_ReturnsFalse()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(string));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.False(result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void DrawValue_WhenEntryValueTypeIsNotHotkey_ReturnsWithoutDrawing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(string));

            // Act
            editor.DrawValue(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenButtonNotClicked_DoesNotChangeSession()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.Button("Displayed Hotkey", It.IsAny<GUILayoutOption[]>())).Returns(false);
            var editor = CreateEditor(unityGuiMock);
            var state = new GuiStateStore();
            var hotkey = CreateHotkey(new UnityProvider(), Key.A);
            var entryMock = CreateHotkeyEntry(hotkey);
            state.SetText(state.GetKey(entryMock.Object, "_hotkey"), "Displayed Hotkey");

            // Act
            editor.DrawValue(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.Null(editor.Session.Entry);
            Assert.Equal(HotkeyEditState.Idle, editor.Session.State);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("Displayed Hotkey", It.IsAny<GUILayoutOption[]>()), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenButtonClickedForCurrentEntry_ClearsCurrentSession()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);
            var hotkey = CreateHotkey(new UnityProvider(), Key.A);
            var entryMock = CreateHotkeyEntry(hotkey);
            editor.Session.BeginEdit(entryMock.Object);
            var displayedValue = editor.Session.WorkingValue.ToString();
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.Button(displayedValue, It.IsAny<GUILayoutOption[]>())).Returns(true);

            // Act
            editor.DrawValue(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Null(editor.Session.Entry);
            Assert.Equal(HotkeyEditState.Idle, editor.Session.State);
            Assert.Null(editor.Session.WorkingValue);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button(displayedValue, It.IsAny<GUILayoutOption[]>()), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenButtonClickedForDifferentEntry_BeginsEditingNewEntry()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.Button("New Hotkey", It.IsAny<GUILayoutOption[]>())).Returns(true);
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var oldEntryMock = CreateHotkeyEntry(CreateHotkey(unityProvider, Key.A), "OldEntry");
            var newEntryMock = CreateHotkeyEntry(CreateHotkey(unityProvider, Key.B), "NewEntry");
            var state = new GuiStateStore();
            state.SetText(state.GetKey(newEntryMock.Object, "_hotkey"), "New Hotkey");
            editor.Session.BeginEdit(oldEntryMock.Object);

            // Act
            editor.DrawValue(newEntryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.Same(newEntryMock.Object, editor.Session.Entry);
            Assert.Equal(HotkeyEditState.Expanded, editor.Session.State);
            Assert.NotNull(editor.Session.WorkingValue);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Button("New Hotkey", It.IsAny<GUILayoutOption[]>()), Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenSessionEntryDoesNotMatch_ReturnsWithoutDrawing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);
            var entryMock = CreateHotkeyEntry(new Hotkey(), "Entry");
            var otherEntryMock = CreateHotkeyEntry(new Hotkey(), "OtherEntry");
            editor.Session.Entry = otherEntryMock.Object;

            // Act
            editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawExtra_WhenWorkingValueIsNull_ReturnsAfterUpdate()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);
            var entryMock = CreateHotkeyEntry(new Hotkey(), "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.Expanded;
            editor.Session.WorkingValue = null;

            // Act
            editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawExtra_WhenSingleChordAndNoButtonsClicked_RendersWithoutChangingHotkey()
        {
            // Arrange
            const float leadingBlank = 12.5f;
            GUIStyle boxStyle = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.ExpandWidth(false)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.BeginHorizontal(It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndHorizontal());
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(leadingBlank));
            unityGuiMock.Setup(x => x.Button(It.IsAny<string>(), It.IsAny<GUILayoutOption[]>())).Returns(false);
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var originalHotkey = CreateHotkey(unityProvider, Key.A);
            var workingHotkey = originalHotkey.Clone();
            var entryMock = CreateHotkeyEntry(originalHotkey, "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.Expanded;
            editor.Session.OriginalValue = originalHotkey;
            editor.Session.WorkingValue = workingHotkey;
            var state = new GuiStateStore();
            state.SetFloat(GuiStateStore.LeadingBlankWidthKey, leadingBlank);

            // Act
            editor.DrawExtra(entryMock.Object, state, new EntryChangeSink());

            // Assert
            Assert.Equal(1, editor.Session.WorkingValue.Count);
            Assert.Equal(HotkeyEditState.Expanded, editor.Session.State);
            unityGuiMock.Verify(x => x.BeginHorizontal(It.IsAny<GUILayoutOption[]>()), Times.Exactly(3));
            unityGuiMock.Verify(x => x.EndHorizontal(), Times.Exactly(3));
            unityGuiMock.Verify(x => x.Space(leadingBlank), Times.Once);
            unityGuiMock.Verify(x => x.BeginVertical(boxStyle, It.IsAny<GUILayoutOption[]>()), Times.Once);
            unityGuiMock.Verify(x => x.EndVertical(), Times.Once);
            unityGuiMock.Verify(x => x.Button("A", It.IsAny<GUILayoutOption[]>()), Times.Once);
            unityGuiMock.Verify(x => x.Button(TranslatorResource.Record.ToString(), It.IsAny<GUILayoutOption[]>()), Times.Once);
            unityGuiMock.Verify(x => x.Button(TranslatorResource.Add.ToString(), It.IsAny<GUILayoutOption[]>()), Times.Once);
            unityGuiMock.Verify(x => x.Button(TranslatorResource.Remove.ToString(), It.IsAny<GUILayoutOption[]>()), Times.Never);
        }

        [Fact]
        public void DrawExtra_WhenRecordClicked_ClearsChordAndOpensPopup()
        {
            // Arrange
            var originalPopupTitle = GuiPipe.PopupTitle;
            var originalPopupWindowAction = GuiPipe.PopupWindowAction;
            var originalClosePopupWindowAction = GuiPipe.ClosePopupWindowAction;
            Hotkey.GlobalValid = true;
            GUIStyle boxStyle = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.ExpandWidth(false)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.BeginHorizontal(It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndHorizontal());
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(0f));
            unityGuiMock
                .Setup(x => x.Button(It.IsAny<string>(), It.IsAny<GUILayoutOption[]>()))
                .Returns<string, GUILayoutOption[]>((text, _) => text == TranslatorResource.Record.ToString());
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var originalHotkey = CreateHotkey(unityProvider, Key.A);
            var workingHotkey = originalHotkey.Clone();
            var chord = workingHotkey.Hotkeys[0];
            var entryMock = CreateHotkeyEntry(originalHotkey, "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.Expanded;
            editor.Session.OriginalValue = originalHotkey;
            editor.Session.WorkingValue = workingHotkey;
            var state = new GuiStateStore();

            try
            {
                // Act
                editor.DrawExtra(entryMock.Object, state, new EntryChangeSink());

                // Assert
                Assert.False(chord.IsValid);
                Assert.Same(chord, editor.Session.WorkingChord);
                Assert.Equal(HotkeyEditState.WaitingPress, editor.Session.State);
                Assert.False(Hotkey.GlobalValid);
                Assert.True(state.GetBool(GuiStateStore.IsPopupOpenKey));
                Assert.Same(TranslatorResource.RecordHotkeyPopupTitle, GuiPipe.PopupTitle);
                Assert.NotNull(GuiPipe.PopupWindowAction);
                Assert.NotNull(GuiPipe.ClosePopupWindowAction);
            }
            finally
            {
                Hotkey.GlobalValid = true;
                GuiPipe.PopupTitle = originalPopupTitle;
                GuiPipe.PopupWindowAction = originalPopupWindowAction;
                GuiPipe.ClosePopupWindowAction = originalClosePopupWindowAction;
            }
        }

        [Fact]
        public void DrawExtra_WhenRemoveClickedForMultipleChords_RemovesChordAndCommitsValue()
        {
            // Arrange
            GUIStyle boxStyle = null;
            var removeClickCount = 0;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.ExpandWidth(false)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.BeginHorizontal(It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndHorizontal());
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(0f));
            unityGuiMock
                .Setup(x => x.Button(It.IsAny<string>(), It.IsAny<GUILayoutOption[]>()))
                .Returns<string, GUILayoutOption[]>((text, _) =>
                {
                    if (text == TranslatorResource.Remove.ToString() && removeClickCount == 0)
                    {
                        removeClickCount++;
                        return true;
                    }

                    return false;
                });
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var originalHotkey = CreateHotkey(unityProvider, Key.A, Key.B);
            var workingHotkey = originalHotkey.Clone();
            var removedChord = workingHotkey.Hotkeys[0];
            var entryMock = CreateHotkeyEntry(originalHotkey, "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.Expanded;
            editor.Session.OriginalValue = originalHotkey;
            editor.Session.WorkingValue = workingHotkey;
            var changeSink = new EntryChangeSink();

            // Act
            editor.DrawExtra(entryMock.Object, new GuiStateStore(), changeSink);

            // Assert
            var updatedHotkey = Assert.IsType<Hotkey>(entryMock.Object.Value);
            Assert.Equal(1, updatedHotkey.Count);
            Assert.DoesNotContain(removedChord, updatedHotkey.Hotkeys);
            Assert.Equal(HotkeyEditState.Expanded, editor.Session.State);
            unityGuiMock.Verify(x => x.Button(TranslatorResource.Remove.ToString(), It.IsAny<GUILayoutOption[]>()), Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenAddClicked_AddsChordAndOpensPopup()
        {
            // Arrange
            var originalPopupTitle = GuiPipe.PopupTitle;
            var originalPopupWindowAction = GuiPipe.PopupWindowAction;
            var originalClosePopupWindowAction = GuiPipe.ClosePopupWindowAction;
            Hotkey.GlobalValid = true;
            GUIStyle boxStyle = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.ExpandWidth(false)).Returns((GUILayoutOption)null);
            unityGuiMock.Setup(x => x.BeginHorizontal(It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndHorizontal());
            unityGuiMock.Setup(x => x.BeginVertical(boxStyle, It.IsAny<GUILayoutOption[]>()));
            unityGuiMock.Setup(x => x.EndVertical());
            unityGuiMock.Setup(x => x.Space(0f));
            unityGuiMock
                .Setup(x => x.Button(It.IsAny<string>(), It.IsAny<GUILayoutOption[]>()))
                .Returns<string, GUILayoutOption[]>((text, _) => text == TranslatorResource.Add.ToString());
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var originalHotkey = CreateHotkey(unityProvider, Key.A);
            var workingHotkey = originalHotkey.Clone();
            var entryMock = CreateHotkeyEntry(originalHotkey, "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.Expanded;
            editor.Session.OriginalValue = originalHotkey;
            editor.Session.WorkingValue = workingHotkey;
            var state = new GuiStateStore();

            try
            {
                // Act
                editor.DrawExtra(entryMock.Object, state, new EntryChangeSink());

                // Assert
                Assert.Equal(2, editor.Session.WorkingValue.Count);
                Assert.NotNull(editor.Session.WorkingChord);
                Assert.False(editor.Session.WorkingChord.IsValid);
                Assert.Equal(HotkeyEditState.WaitingPress, editor.Session.State);
                Assert.False(Hotkey.GlobalValid);
                Assert.True(state.GetBool(GuiStateStore.IsPopupOpenKey));
                Assert.Same(TranslatorResource.RecordHotkeyPopupTitle, GuiPipe.PopupTitle);
                Assert.NotNull(GuiPipe.PopupWindowAction);
                Assert.NotNull(GuiPipe.ClosePopupWindowAction);
            }
            finally
            {
                Hotkey.GlobalValid = true;
                GuiPipe.PopupTitle = originalPopupTitle;
                GuiPipe.PopupWindowAction = originalPopupWindowAction;
                GuiPipe.ClosePopupWindowAction = originalClosePopupWindowAction;
            }
        }

        [Fact]
        public void SetPopupWindow_WhenCalled_SetsPopupStateAndCloseActionCancelsRecording()
        {
            // Arrange
            var originalPopupTitle = GuiPipe.PopupTitle;
            var originalPopupWindowAction = GuiPipe.PopupWindowAction;
            var originalClosePopupWindowAction = GuiPipe.ClosePopupWindowAction;
            Hotkey.GlobalValid = false;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);
            var unityProvider = new UnityProvider();
            var originalHotkey = CreateHotkey(unityProvider, Key.A);
            originalHotkey.Valid = false;
            var entryMock = CreateHotkeyEntry(originalHotkey, "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.State = HotkeyEditState.WaitingPress;
            editor.Session.OriginalValue = originalHotkey;
            editor.Session.WorkingValue = originalHotkey.Clone();
            editor.Session.WorkingChord = new HotkeyChord(unityProvider);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();

            try
            {
                // Act
                editor.SetPopupWindow(state, changeSink);
                GuiPipe.ClosePopupWindowAction();
                var exception = Record.Exception(() => GuiPipe.PopupWindowAction());

                // Assert
                Assert.True(state.GetBool(GuiStateStore.IsPopupOpenKey));
                Assert.Same(TranslatorResource.RecordHotkeyPopupTitle, GuiPipe.PopupTitle);
                Assert.NotNull(GuiPipe.PopupWindowAction);
                Assert.NotNull(GuiPipe.ClosePopupWindowAction);
                Assert.Null(exception);
                Assert.Equal(HotkeyEditState.Expanded, editor.Session.State);
                Assert.True(Hotkey.GlobalValid);
                Assert.True(originalHotkey.Valid);
                Assert.Null(editor.Session.WorkingChord);
            }
            finally
            {
                Hotkey.GlobalValid = true;
                GuiPipe.PopupTitle = originalPopupTitle;
                GuiPipe.PopupWindowAction = originalPopupWindowAction;
                GuiPipe.ClosePopupWindowAction = originalClosePopupWindowAction;
            }
        }


        [Fact]
        public void GetHotkeyDisplayString_WhenSessionEntryMatchesAndWorkingValueIsNull_ReturnsEmptyString()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = CreateHotkeyEntry(new Hotkey(), "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.WorkingValue = null;

            // Act
            var result = editor.GetHotkeyDisplayString(entryMock.Object, new GuiStateStore());

            // Assert
            Assert.Equal(string.Empty, result);
        }


        [Fact]
        public void RecordHotkey_WhenSessionIsNotRecording_ReturnsWithoutRendering()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = CreateEditor(unityGuiMock);

            // Act
            editor.RecordHotkey(new GuiStateStore(), new EntryChangeSink());

            // Assert
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetHotkeyDisplayString_WhenSessionEntryMatches_ReturnsWorkingValueString()
        {
            // Arrange
            var editor = CreateEditor();
            var workingValue = CreateHotkey(new UnityProvider(), Key.A, Key.B);
            var entryMock = CreateHotkeyEntry(new Hotkey(), "Entry");
            editor.Session.Entry = entryMock.Object;
            editor.Session.WorkingValue = workingValue;

            // Act
            var result = editor.GetHotkeyDisplayString(entryMock.Object, new GuiStateStore());

            // Assert
            Assert.Equal(workingValue.ToString(), result);
        }

        [Fact]
        public void GetHotkeyDisplayString_WhenSessionEntryDoesNotMatch_ReturnsStateText()
        {
            // Arrange
            var editor = CreateEditor();
            var state = new GuiStateStore();
            var hotkey = CreateHotkey(new UnityProvider(), Key.A);
            var entryMock = CreateHotkeyEntry(hotkey, "Entry");
            var key = state.GetKey(entryMock.Object, "_hotkey");
            state.SetText(key, "Displayed Hotkey");

            // Act
            var result = editor.GetHotkeyDisplayString(entryMock.Object, state);

            // Assert
            Assert.Equal("Displayed Hotkey", result);
        }

        private static HotkeyEditor CreateEditor(Mock<IUnityGuiProvider> unityGuiMock = null)
        {
            unityGuiMock = unityGuiMock ?? new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            return new HotkeyEditor(unityGuiMock.Object, new StyleResource(unityGuiMock.Object));
        }

        private static Mock<IEntryBinding> CreateHotkeyEntry(Hotkey hotkey, string key = "HotkeyEntry")
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(Hotkey));
            entryMock.SetupProperty(x => x.Value, hotkey);
            return entryMock;
        }

        private static Hotkey CreateHotkey(UnityProvider unityProvider, params Key[] keys)
        {
            var hotkey = new Hotkey(unityProvider);
            foreach (var key in keys)
            {
                var chord = new HotkeyChord(new KeyboardChord(unityProvider, new KeyboardTrigger(key, unityProvider)), unityProvider);
                hotkey.Add(chord);
            }

            return hotkey;
        }
    }
}
