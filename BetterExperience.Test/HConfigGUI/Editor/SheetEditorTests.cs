using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HProvider;
using Moq;
using Xunit;

namespace BetterExperience.Test.HConfigGUI.Editor
{
    public class SheetEditorTests
    {
        [Fact]
        public void Constructor_ValidDependencies_InitializesPropertiesAndTableEditor()
        {
            // Arrange
            var unityProvider = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGui = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var guiStateStore = new GuiStateStore();

            // Act
            var editor = new SheetEditor(unityProvider.Object, unityGui.Object, guiStateStore, null);

            // Assert
            Assert.Same(unityProvider.Object, editor.UnityService);
            Assert.Same(unityGui.Object, editor.UnityGui);
            Assert.Same(guiStateStore, editor.GuiStateStore);
            Assert.Null(editor.StyleProvider);
            Assert.NotNull(editor.TableEditor);
            Assert.Same(unityProvider.Object, editor.TableEditor.UnityService);
            Assert.Same(unityGui.Object, editor.TableEditor.UnityGui);
            Assert.Same(guiStateStore, editor.TableEditor.GuiStateStore);
            Assert.Null(editor.TableEditor.StyleProvider);
        }


        [Fact]
        public void Update_DelayedEntryExists_FlushesPendingValueThroughTableEditor()
        {
            // Arrange
            var unityProvider = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGui = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new SheetEditor(unityProvider.Object, unityGui.Object, new GuiStateStore(), null);
            var entry = new Mock<IEntryBinding>(MockBehavior.Strict);
            object currentValue = "old";
            entry.SetupGet(x => x.Value).Returns(() => currentValue);
            entry.SetupSet(x => x.Value = It.IsAny<object>())
                .Callback<object>(value => currentValue = value);
            editor.TableEditor.ChangeSink.SetValue(entry.Object, "new", 0.25f);

            // Act
            editor.Update(0.25f);

            // Assert
            Assert.Equal("new", currentValue);
            entry.VerifySet(x => x.Value = It.Is<object>(value => Equals(value, "new")), Times.Once);
        }
    }
}
