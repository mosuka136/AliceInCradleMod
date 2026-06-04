using BetterExperience.HConfigGUI;
using Xunit;

namespace BetterExperience.Test
{
    public class GuiHostTests
    {
        [Fact]
        public void Hide_WhenAlreadyHidden_LeavesWindowHidden()
        {
            // Arrange
            var sut = new GuiHost();

            // Act
            sut.Hide();

            // Assert
            Assert.False(sut.IsVisible);
            Assert.False(sut.HasDraggedWindowSinceOpen);
        }

        [Fact]
        public void Hide_WhenVisible_ClearsVisibilityAndDraggedState()
        {
            // Arrange
            var sut = new GuiHost();
            sut.ToggleVisibility();

            // Act
            sut.Hide();

            // Assert
            Assert.False(sut.IsVisible);
            Assert.False(sut.HasDraggedWindowSinceOpen);
        }

        [Fact]
        public void ToggleVisibility_WhenHidden_ShowsWindow()
        {
            // Arrange
            var sut = new GuiHost();

            // Act
            sut.ToggleVisibility();

            // Assert
            Assert.True(sut.IsVisible);
        }

        [Fact]
        public void ToggleVisibility_WhenVisible_HidesWindowAndClearsDraggedState()
        {
            // Arrange
            var sut = new GuiHost();
            sut.ToggleVisibility();

            // Act
            sut.ToggleVisibility();

            // Assert
            Assert.False(sut.IsVisible);
            Assert.False(sut.HasDraggedWindowSinceOpen);
        }
    }
}
