using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;
using Moq;
using System.Security;

namespace BetterExperience.Test.HConfigGUI.Resource
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
            // Arrange

            // Act
            var resource = new StyleResource(null);

            // Assert
            Assert.Null(resource.UnityGui);
        }

        [Fact]
        public void PopupTitleStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.PopupTitleStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void SidebarEntryStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.SidebarEntryStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void SidebarSelectedEntryStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.SidebarSelectedEntryStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void TableTitleStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.TableTitleStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void TooltipStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.TooltipStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void RecordingHotkeyLabelStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.RecordingHotkeyLabelStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void SliderStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.SliderStyle);

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void SliderThumbStyle_WhenAccessedInTestHost_ThrowsSecurityException()
        {
            // Arrange
            var unityGuiMock = new Mock<IUnityGuiProvider>(MockBehavior.Strict);
            var resource = new StyleResource(unityGuiMock.Object);

            // Act
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.SliderThumbStyle);

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
            SecurityException exception = Assert.Throws<SecurityException>(() => _ = resource.ToastStyle);

            // Assert
            Assert.NotNull(exception);
        }


    }
}
