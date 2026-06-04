using BetterExperience.HConfigGUI.Editor;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using UnityEngine;

namespace BetterExperience.Test
{
    public class TooltipEditorTests
    {
        [Fact]
        public void TooltipEditor_WhenConstructed_StoresDependencies()
        {
            // Arrange
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var styleProvider = new StyleResource(unityGuiMock.Object);

            // Act
            var editor = new TooltipEditor(unityServiceMock.Object, unityGuiMock.Object, styleProvider);

            // Assert
            Assert.Same(unityServiceMock.Object, editor.UnityService);
            Assert.Same(unityGuiMock.Object, editor.UnityGui);
            Assert.Same(styleProvider, editor.StyleProvider);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DrawTooltip_WhenTooltipIsNullOrEmpty_ReturnsWithoutDrawing(string tooltip)
        {
            // Arrange
            var unityServiceMock = new Mock<IUnityProvider>(MockBehavior.Strict);
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            unityGuiMock.SetupGet(x => x.Tooltip).Returns(tooltip);
            var styleProvider = new StyleResource(unityGuiMock.Object);
            var editor = new TooltipEditor(unityServiceMock.Object, unityGuiMock.Object, styleProvider);

            // Act
            editor.DrawTooltip(new Rect(0f, 0f, 200f, 100f));

            // Assert
            unityGuiMock.VerifyGet(x => x.Tooltip, Times.Once);
            unityGuiMock.VerifyNoOtherCalls();
            unityServiceMock.VerifyNoOtherCalls();
        }

    }
}
