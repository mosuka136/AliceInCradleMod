using BetterExperience.HLogGUI;

namespace BetterExperience.Test.HLogGUI
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
            SetHasDraggedWindowSinceOpen(sut, true);

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
            SetHasDraggedWindowSinceOpen(sut, true);

            // Act
            sut.ToggleVisibility();

            // Assert
            Assert.False(sut.IsVisible);
            Assert.False(sut.HasDraggedWindowSinceOpen);
        }

        private static void SetHasDraggedWindowSinceOpen(GuiHost sut, bool value)
        {
            var property = typeof(GuiHost).GetProperty(nameof(GuiHost.HasDraggedWindowSinceOpen));
            Assert.NotNull(property);
            property.SetValue(sut, value);
        }
    }
}
