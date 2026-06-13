using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using UnityEngine;

namespace BetterExperience.Test.HConfigGUI.Editor.ValueEditor
{
    public class BooleanEditorTests
    {
        [Fact]
        public void BooleanEditor_WhenConstructed_StoresUnityGuiProvider()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var editor = new BooleanEditor(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsBoolean_ReturnsTrue()
        {
            // Arrange
            var editor = CreateEditor();
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.ValueType).Returns(typeof(bool));

            // Act
            var result = editor.CanEdit(entryMock.Object);

            // Assert
            Assert.True(result);
            entryMock.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void CanEdit_WhenEntryValueTypeIsNotBoolean_ReturnsFalse()
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
        public void DrawValue_WhenEntryValueIsNotBoolean_DoesNothing()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new BooleanEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("NonBooleanEntry", "text");

            // Act
            editor.DrawValue(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Equal("text", entryMock.Object.Value);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawValue_WhenToggleReturnsSameValue_DoesNotChangeEntry()
        {
            // Arrange
            const bool currentValue = true;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.Toggle(currentValue, TranslatorResource.On.ToString(), It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(currentValue);
            var editor = new BooleanEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("BooleanEntry", currentValue);

            // Act
            editor.DrawValue(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Equal(currentValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Toggle(currentValue, TranslatorResource.On.ToString(), It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
        }

        [Fact]
        public void DrawValue_WhenToggleReturnsDifferentValue_UpdatesEntry()
        {
            // Arrange
            const bool currentValue = false;
            const bool newValue = true;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.ExpandWidth(true)).Returns((GUILayoutOption)null);
            unityGuiMock
                .Setup(x => x.Toggle(currentValue, TranslatorResource.Off.ToString(), It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)))
                .Returns(newValue);
            var editor = new BooleanEditor(unityGuiMock.Object);
            var entryMock = CreateEntry("BooleanEntry", currentValue);

            // Act
            editor.DrawValue(entryMock.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Equal(newValue, entryMock.Object.Value);
            unityGuiMock.Verify(x => x.ExpandWidth(true), Times.Once);
            unityGuiMock.Verify(x => x.Toggle(currentValue, TranslatorResource.Off.ToString(), It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == null)), Times.Once);
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

        private static BooleanEditor CreateEditor()
        {
            return new BooleanEditor(new Mock<IUnityGuiProvider>(MockBehavior.Strict).Object);
        }

        private static Mock<IEntryBinding> CreateEntry(string key, object value)
        {
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            entryMock.SetupGet(x => x.Key).Returns(key);
            entryMock.SetupProperty(x => x.Value, value);
            return entryMock;
        }
    }
}
