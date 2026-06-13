using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Editor.ValueEditor;
using Moq;

namespace BetterExperience.Test.HConfigGUI.Editor
{
    public class ValueEditorRegistryTests
    {
        [Fact]
        public void RegisterEditor_WhenRegisteredEditorCanEdit_ReturnsRegisteredEditor()
        {
            // Arrange
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var editorMock = new Mock<IValueEditor>(MockBehavior.Strict);
            editorMock.Setup(x => x.CanEdit(entryMock.Object)).Returns(true);
            var registry = new ValueEditorRegistry();
            registry.RegisterEditor(editorMock.Object);

            // Act
            var result = registry.GetEditor(entryMock.Object);

            // Assert
            Assert.Same(editorMock.Object, result);
            editorMock.Verify(x => x.CanEdit(entryMock.Object), Times.Once);
            editorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetEditor_WhenFirstRegisteredEditorCannotEdit_ReturnsFirstLaterMatchingEditor()
        {
            // Arrange
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var firstEditorMock = new Mock<IValueEditor>(MockBehavior.Strict);
            var secondEditorMock = new Mock<IValueEditor>(MockBehavior.Strict);
            firstEditorMock.Setup(x => x.CanEdit(entryMock.Object)).Returns(false);
            secondEditorMock.Setup(x => x.CanEdit(entryMock.Object)).Returns(true);
            var registry = new ValueEditorRegistry();
            registry.RegisterEditor(firstEditorMock.Object);
            registry.RegisterEditor(secondEditorMock.Object);

            // Act
            var result = registry.GetEditor(entryMock.Object);

            // Assert
            Assert.Same(secondEditorMock.Object, result);
            firstEditorMock.Verify(x => x.CanEdit(entryMock.Object), Times.Once);
            secondEditorMock.Verify(x => x.CanEdit(entryMock.Object), Times.Once);
            firstEditorMock.VerifyNoOtherCalls();
            secondEditorMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetEditor_WhenNoRegisteredEditorCanEdit_ReturnsUnsupportedEditorInstance()
        {
            // Arrange
            var entryMock = new Mock<IEntryBinding>(MockBehavior.Strict);
            var firstEditorMock = new Mock<IValueEditor>(MockBehavior.Strict);
            var secondEditorMock = new Mock<IValueEditor>(MockBehavior.Strict);
            firstEditorMock.Setup(x => x.CanEdit(entryMock.Object)).Returns(false);
            secondEditorMock.Setup(x => x.CanEdit(entryMock.Object)).Returns(false);
            var registry = new ValueEditorRegistry();
            registry.RegisterEditor(firstEditorMock.Object);
            registry.RegisterEditor(secondEditorMock.Object);

            // Act
            var result = registry.GetEditor(entryMock.Object);

            // Assert
            Assert.Same(UnsupportedEditor.Instance, result);
            firstEditorMock.Verify(x => x.CanEdit(entryMock.Object), Times.Once);
            secondEditorMock.Verify(x => x.CanEdit(entryMock.Object), Times.Once);
            firstEditorMock.VerifyNoOtherCalls();
            secondEditorMock.VerifyNoOtherCalls();
        }

    }
}
