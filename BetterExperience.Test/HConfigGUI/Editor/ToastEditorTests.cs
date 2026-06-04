using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using System.Security;
using UnityEngine;
using Xunit;

namespace BetterExperience.Test
{
    public class ToastEditorTests
    {
        [Fact]
        public void ToastEditor_WhenConstructed_StoresDependencies()
        {
            // Arrange
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var styleProvider = new StyleResource(unityGuiMock.Object);

            // Act
            var editor = new ToastEditor(unityServiceMock.Object, unityGuiMock.Object, styleProvider);

            // Assert
            Assert.Same(unityServiceMock.Object, editor.UnityService);
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(styleProvider, editor.StyleProvider);
        }

        [Fact]
        public void SetToast_WhenCalled_StoresMessageAndCalculatesEndTime()
        {
            // Arrange
            const float realtimeSinceStartup = 10f;
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            unityServiceMock.SetupGet(x => x.RealtimeSinceStartup).Returns(realtimeSinceStartup);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new ToastEditor(unityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object))
            {
                Duration = 2.5f,
            };

            // Act
            editor.SetToast("Toast message");

            // Assert
            Assert.Equal("Toast message", editor.Message);
            Assert.Equal(12.5f, editor.EndTime);
            unityServiceMock.VerifyGet(x => x.RealtimeSinceStartup, Times.Once);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DrawToast_WhenMessageIsNullOrEmpty_ReturnsWithoutDrawing(string message)
        {
            // Arrange
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new ToastEditor(unityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object))
            {
                Message = message,
            };

            // Act
            editor.DrawToast(new Rect(0f, 0f, 200f, 100f));

            // Assert
            Assert.Equal(message, editor.Message);
            unityServiceMock.VerifyNoOtherCalls();
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawToast_WhenToastHasExpired_ClearsMessageAndReturns()
        {
            // Arrange
            const float currentTime = 5f;
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            unityServiceMock.SetupGet(x => x.RealtimeSinceStartup).Returns(currentTime);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var editor = new ToastEditor(unityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object))
            {
                Message = "Expired toast",
                EndTime = currentTime,
            };

            // Act
            editor.DrawToast(new Rect(0f, 0f, 200f, 100f));

            // Assert
            Assert.Null(editor.Message);
            unityServiceMock.VerifyGet(x => x.RealtimeSinceStartup, Times.Once);
            unityGuiMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DrawToast_WhenToastIsVisible_ThrowsSecurityExceptionInTestEnvironment()
        {
            // Arrange
            const string message = "Visible toast";
            const float currentTime = 9f;
            const float endTime = 9.25f;
            const float fadeDuration = 0.5f;
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            unityServiceMock.SetupGet(x => x.RealtimeSinceStartup).Returns(currentTime);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupProperty(x => x.Color, new Color(0.2f, 0.3f, 0.4f, 1f));
            unityGuiMock.Setup(x => x.GetColor(1f, 1f, 1f, 0.5f)).Returns(new Color(1f, 1f, 1f, 0.5f));
            unityGuiMock.Setup(x => x.GetContent(message)).Returns(new GUIContent(message));
            var editor = new ToastEditor(unityServiceMock.Object, unityGuiMock.Object, new StyleResource(unityGuiMock.Object))
            {
                Message = message,
                EndTime = endTime,
                FadeDuration = fadeDuration,
            };

            // Act
            var exception = Assert.Throws<SecurityException>(() => editor.DrawToast(new Rect(0f, 0f, 150f, 50f)));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(message, editor.Message);
            unityServiceMock.VerifyGet(x => x.RealtimeSinceStartup, Times.Once);
            unityGuiMock.VerifyGet(x => x.Color, Times.Once);
            unityGuiMock.VerifySet(x => x.Color = It.IsAny<Color>(), Times.Once);
            unityGuiMock.Verify(x => x.GetColor(1f, 1f, 1f, 0.5f), Times.Once);
            unityGuiMock.Verify(x => x.GetContent(message), Times.Once);
        }
    }
}
