using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HProvider;
using Moq;
using UnityEngine;

namespace BetterExperience.Test.HConfigGUI.Editor.ValueEditor
{
    public class StringEditorTests
    {
        [Fact]
        public void StringEditor_WhenConstructed_StoresUnityGuiProvider()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var editor = new StringEditor(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Equal(0.5f, editor.DelayApplyDuration);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsString_ReturnsTrue()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(string));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.True(result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsNotString_ReturnsFalse()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(int));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.False(result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void DrawValue_WhenEntryValueIsNull_ReturnsWithoutDrawing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new StringEditor(unityGuiMock.Object);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("NullEntry", null);
            var key = state.GetKey(entryMock.Object, "_string");

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);

            // Assert
            Assert.Null(state.GetText(key, "fallback"));
            Assert.Null(entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenTextFieldReturnsSameValue_DoesNotQueueChange()
        {
            // Arrange
            const string currentValue = "value";
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.TextField(currentValue, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(currentValue);
            var editor = new StringEditor(unityGuiMock.Object);
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("SameEntry", currentValue);
            var key = state.GetKey(entryMock.Object, "_string");

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(1.0f);

            // Assert
            Assert.Equal(currentValue, state.GetText(key, "fallback"));
            Assert.Equal(currentValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.TextField(currentValue, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenTextFieldReturnsDifferentValue_UpdatesStateAndQueuesDelayedChange()
        {
            // Arrange
            const string currentValue = "old";
            const string newValue = "new";
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.TextField(currentValue, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(newValue);
            var editor = new StringEditor(unityGuiMock.Object)
            {
                DelayApplyDuration = 0.5f,
            };
            var state = new GuiStateStore();
            var changeSink = new EntryChangeSink();
            var entryMock = CreateEntry("ChangedEntry", currentValue);
            var key = state.GetKey(entryMock.Object, "_string");

            // Act
            editor.DrawValue(entryMock.Object, state, changeSink);
            changeSink.FlushValue(0.4f);
            var valueBeforeDelayExpires = entryMock.Object.Value;
            changeSink.FlushValue(0.1f);

            // Assert
            Assert.Equal(newValue, state.GetText(key, "fallback"));
            Assert.Equal(currentValue, valueBeforeDelayExpires);
            Assert.Equal(newValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.TextField(currentValue, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenCalled_DoesNothing()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);

            // Act
            var exception = Record.Exception(() => editor.DrawExtra(entryMock.Object, new GuiStateStore(), new EntryChangeSink()));

            // Assert
            Assert.Null(exception);
        }

        private static StringEditor CreateEditor()
        {
            return new StringEditor(new Mock<IUnityGuiProvider>(MockBehavior.Strict).Object);
        }

        private static Mock<IEntryBinding> CreateEntry(string key, string value)
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupProperty(x => x.Value, value);
            return entryMock;
        }
    }
}
