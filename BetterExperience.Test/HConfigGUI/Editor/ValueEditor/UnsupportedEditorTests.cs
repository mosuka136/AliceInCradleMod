using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using BetterExperience.HProvider;
using Moq;
using System;

namespace BetterExperience.Test.HConfigGUI.Editor.ValueEditor
{
    public class UnsupportedEditorTests
    {
        [Fact]
        public void UnsupportedEditor_WhenConstructed_InitializesUnityGuiProvider()
        {
            // Arrange

            // Act
            var editor = new UnsupportedEditor();

            // Assert
            Assert.NotNull(editor.UnityGui);
            Assert.IsType<UnityGuiProvider>(editor.UnityGui);
        }

        [Fact]
        public void CanEdit_WhenEntryIsNull_ReturnsFalse()
        {
            // Arrange
            var editor = new UnsupportedEditor();

            // Act
            var result = editor.CanEdit(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DrawValue_WhenEntryIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var editor = new UnsupportedEditor();

            // Act
            Action action = () => editor.DrawValue(null, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Throws<NullReferenceException>(action);
        }

        [Fact]
        public void DrawValue_WhenEntryHasValueType_ThrowsSecurityExceptionInTestEnvironment()
        {
            // Arrange
            var entry = new Mock<IEntryBinding>(MockBehavior.Strict);
            entry.SetupGet(x => x.ValueType).Returns(typeof(int));
            var editor = new UnsupportedEditor();

            // Act
            Action action = () => editor.DrawValue(entry.Object, new GuiStateStore(), new EntryChangeSink());

            // Assert
            Assert.Throws<System.Security.SecurityException>(action);
            entry.VerifyGet(x => x.ValueType, Times.Once);
        }

        [Fact]
        public void DrawExtra_WhenArgumentsProvided_DoesNotThrow()
        {
            // Arrange
            var entry = new Mock<IEntryBinding>(MockBehavior.Strict);
            var editor = new UnsupportedEditor();

            // Act
            var exception = Record.Exception(() => editor.DrawExtra(entry.Object, new GuiStateStore(), new EntryChangeSink()));

            // Assert
            Assert.Null(exception);
        }
    }
}
