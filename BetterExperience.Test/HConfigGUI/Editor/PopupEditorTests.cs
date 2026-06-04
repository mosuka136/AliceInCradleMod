using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using Moq;
using UnityEngine;

namespace BetterExperience.Test
{
    public class PopupEditorTests
    {
        [Fact]
        public void PopupEditor_WhenConstructed_InitializesDependenciesAndCentersPopupRect()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.ScreenWidth).Returns(800f);
            unityGuiMock.SetupGet(x => x.ScreenHeight).Returns(600f);
            var styleResource = new StyleResource(unityGuiMock.Object);
            var guiStateStore = new GuiStateStore();

            // Act
            var editor = new PopupEditor(unityGuiMock.Object, styleResource, guiStateStore);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(styleResource, editor.StyleProvider);
            Assert.Same(guiStateStore, editor.GuiStateStore);
            Assert.Equal(new Rect(300f, 255f, 200f, 90f), editor.PopupRect);
            unityGuiMock.VerifyGet(x => x.ScreenWidth, Times.Exactly(2));
            unityGuiMock.VerifyGet(x => x.ScreenHeight, Times.Exactly(2));
        }

        [Fact]
        public void DrawPopup_WhenCalled_SetsPopupStateDrawsBackdropAndUpdatesPopupRect()
        {
            // Arrange
            var originalColor = new Color(0.2f, 0.3f, 0.4f, 0.5f);
            var overlayColor = new Color(0f, 0f, 0f, 0.55f);
            var expectedPopupRect = new Rect(300f, 255f, 200f, 90f);
            var updatedPopupRect = new Rect(10f, 20f, 30f, 40f);
            var title = new Translator("标题", "Title");
            Action drawContentAction = () => { };
            Action closePopupAction = () => { };
            GUIStyle boxStyle = null;
            Rect boxRect = default;
            Rect modalClientRect = default;
            GUI.WindowFunction modalFunction = null;
            int modalId = 0;
            string modalTitle = null;
            GUIStyle modalStyle = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.ScreenWidth).Returns(800f);
            unityGuiMock.SetupGet(x => x.ScreenHeight).Returns(600f);
            unityGuiMock.SetupProperty(x => x.Color, originalColor);
            unityGuiMock.SetupGet(x => x.BoxStyle).Returns(boxStyle);
            unityGuiMock
                .Setup(x => x.Box(It.IsAny<Rect>(), string.Empty))
                .Callback<Rect, string>((rect, _) => boxRect = rect);
            unityGuiMock
                .Setup(x => x.ModalWindow(It.IsAny<int>(), It.IsAny<Rect>(), It.IsAny<GUI.WindowFunction>(), string.Empty, boxStyle))
                .Callback<int, Rect, GUI.WindowFunction, string, GUIStyle>((id, rect, func, popupTitle, style) =>
                {
                    modalId = id;
                    modalClientRect = rect;
                    modalFunction = func;
                    modalTitle = popupTitle;
                    modalStyle = style;
                })
                .Returns(updatedPopupRect);
            var styleResource = new StyleResource(unityGuiMock.Object);
            var guiStateStore = new GuiStateStore();
            var editor = new PopupEditor(unityGuiMock.Object, styleResource, guiStateStore);

            // Act
            editor.DrawPopup(title, drawContentAction, closePopupAction);

            // Assert
            Assert.Same(title, editor.Title);
            Assert.Same(drawContentAction, editor.DrawContentAction);
            Assert.Same(closePopupAction, editor.ClosePopupAction);
            Assert.Equal(originalColor, unityGuiMock.Object.Color);
            Assert.Equal(new Rect(0f, 0f, 800f, 600f), boxRect);
            Assert.Equal(editor.PopupID, modalId);
            Assert.Equal(expectedPopupRect, modalClientRect);
            Assert.NotNull(modalFunction);
            Assert.Equal(string.Empty, modalTitle);
            Assert.Same(boxStyle, modalStyle);
            Assert.Equal(updatedPopupRect, editor.PopupRect);
            unityGuiMock.VerifySet(x => x.Color = overlayColor, Times.Once);
            unityGuiMock.VerifySet(x => x.Color = originalColor, Times.Once);
            unityGuiMock.Verify(x => x.Box(It.IsAny<Rect>(), string.Empty), Times.Once);
            unityGuiMock.Verify(x => x.ModalWindow(It.IsAny<int>(), It.IsAny<Rect>(), It.IsAny<GUI.WindowFunction>(), string.Empty, boxStyle), Times.Once);
        }
    }
}
