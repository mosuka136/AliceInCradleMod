using BetterExperience.HLogGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using System.Security;

namespace BetterExperience.Test.HLogGUI.Resource
{
    public class StyleResourceTests
    {
        [Fact]
        public void StyleResource_WhenConstructedWithUnityGui_StoresUnityGui()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);

            // Act
            var resource = new StyleResource(unityGuiMock.Object);

            // Assert
            Assert.Same(unityGuiMock.Object, resource.UnityGui);
        }

        [Fact]
        public void StyleResource_WhenConstructedWithNull_StoresNullUnityGui()
        {
            // Act
            var resource = new StyleResource(null);

            // Assert
            Assert.Null(resource.UnityGui);
        }

        [Fact]
        public void ColumnVisibilityToggleStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            var exception = Assert.Throws<SecurityException>(() => _ = resource.ColumnVisibilityToggleStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void IdButtonStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            var exception = Assert.Throws<SecurityException>(() => _ = resource.IdButtonStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void ToastStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            var exception = Assert.Throws<SecurityException>(() => _ = resource.ToastStyle);

            // Assert
            Assert.NotNull(exception);
        }
    }
}
