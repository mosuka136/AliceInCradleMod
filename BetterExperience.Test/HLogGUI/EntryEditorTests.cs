using BetterExperience.HGuiSpace;
using BetterExperience.HLogGUI;
using BetterExperience.HLogGUI.Resource;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using Moq;
using UnityEngine;

namespace BetterExperience.Test.HLogGUI
{
    public class EntryEditorTests
    {
        [Fact]
        public void EntryEditor_WhenConstructed_StoresDependencies()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastUnityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastEditor = new ToastEditor(toastUnityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object));

            // Act
            var editor = new EntryEditor(unityGuiMock.Object, unityServiceMock.Object, toastEditor);

            // Assert
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(unityServiceMock.Object, editor.UnityService);
            Assert.Same(toastEditor, editor.ToastEditor);
        }

        [Fact]
        public void Render_WhenUnityGuiIsNull_ReturnsWithoutUsingUnityService()
        {
            // Arrange
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastUnityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastEditor = new ToastEditor(toastUnityServiceMock.Object, null, new StyleResource(null));
            var editor = new EntryEditor(null, unityServiceMock.Object, toastEditor);

            // Act
            editor.Render("text", null, 100f);

            // Assert
            unityServiceMock.VerifyNoOtherCalls();
            toastUnityServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Render_WhenButtonIsNotClicked_UsesFirstNonEmptyLineAsButtonTextAndOriginalTextAsTooltip()
        {
            // Arrange
            const string text = "\r\nfirst line\nsecond line";
            var content = new GUIContent("first line", text);
            GUIStyle style = null;
            GUILayoutOption widthOption = null;
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.Setup(x => x.BeginHorizontal());
            unityGuiMock.Setup(x => x.GetContent("first line", text)).Returns(content);
            unityGuiMock.Setup(x => x.Width(120f)).Returns(widthOption);
            unityGuiMock
                .Setup(x => x.Button(content, style, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == widthOption)))
                .Returns(false);
            unityGuiMock.Setup(x => x.EndHorizontal());
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastUnityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var toastEditor = new ToastEditor(toastUnityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object));
            var editor = new EntryEditor(unityGuiMock.Object, unityServiceMock.Object, toastEditor);

            // Act
            editor.Render(text, style, 120f);

            // Assert
            unityGuiMock.Verify(x => x.BeginHorizontal(), Times.Once);
            unityGuiMock.Verify(x => x.GetContent("first line", text), Times.Once);
            unityGuiMock.Verify(x => x.Width(120f), Times.Once);
            unityGuiMock.Verify(x => x.Button(content, style, It.IsAny<GUILayoutOption[]>()), Times.Once);
            unityGuiMock.Verify(x => x.EndHorizontal(), Times.Once);
            unityGuiMock.VerifyNoOtherCalls();
            unityServiceMock.VerifyNoOtherCalls();
            toastUnityServiceMock.VerifyNoOtherCalls();
            Assert.Null(toastEditor.Message);
        }

        [Fact]
        public void Render_WhenButtonIsClicked_CopiesOriginalTextAndShowsToastForDisplayedLine()
        {
            // Arrange
            const string text = "first line\nsecond line";
            var originalDefaultLanguage = Translator.DefaultLanguage;
            Translator.DefaultLanguage = LanguageType.English;
            try
            {
                var content = new GUIContent("first line", text);
                GUIStyle style = null;
                GUILayoutOption widthOption = null;
                var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
                unityGuiMock.Setup(x => x.BeginHorizontal());
                unityGuiMock.Setup(x => x.GetContent("first line", text)).Returns(content);
                unityGuiMock.Setup(x => x.Width(80f)).Returns(widthOption);
                unityGuiMock
                    .Setup(x => x.Button(content, style, It.Is<GUILayoutOption[]>(options => options.Length == 1 && options[0] == widthOption)))
                    .Returns(true);
                unityGuiMock.Setup(x => x.EndHorizontal());

                var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
                unityServiceMock.Setup(x => x.ClipboardCopy(text));
                var toastUnityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
                toastUnityServiceMock.SetupGet(x => x.RealtimeSinceStartup).Returns(10f);
                var toastEditor = new ToastEditor(toastUnityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object));
                var editor = new EntryEditor(unityGuiMock.Object, unityServiceMock.Object, toastEditor);

                // Act
                editor.Render(text, style, 80f);

                // Assert
                unityServiceMock.Verify(x => x.ClipboardCopy(text), Times.Once);
                toastUnityServiceMock.VerifyGet(x => x.RealtimeSinceStartup, Times.Once);
                Assert.Equal("Copied:first line", toastEditor.Message);
                Assert.Equal(12f, toastEditor.EndTime);
                unityGuiMock.Verify(x => x.BeginHorizontal(), Times.Once);
                unityGuiMock.Verify(x => x.GetContent("first line", text), Times.Once);
                unityGuiMock.Verify(x => x.Width(80f), Times.Once);
                unityGuiMock.Verify(x => x.Button(content, style, It.IsAny<GUILayoutOption[]>()), Times.Once);
                unityGuiMock.Verify(x => x.EndHorizontal(), Times.Once);
                unityGuiMock.VerifyNoOtherCalls();
                unityServiceMock.VerifyNoOtherCalls();
                toastUnityServiceMock.VerifyNoOtherCalls();
            }
            finally
            {
                Translator.DefaultLanguage = originalDefaultLanguage;
            }
        }
    }
}
