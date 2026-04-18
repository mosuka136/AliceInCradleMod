using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using Moq;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace BetterExperience.Test.HConfigGUI.UI
{
    public class GuiHostTests
    {
        // -----------------------------------------------------------------------
        // Hide
        // -----------------------------------------------------------------------

        [Fact]
        public void Hide_WhenCalled_SetsIsVisibleToFalse()
        {
            // Arrange
            var guiHost = new GuiHost();
            SetPrivateField(guiHost, "_isVisible", true);
            var viewModel = CreateUninitializedViewModel();
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.Hide();

            // Assert
            var isVisible = GetPrivateField<bool>(guiHost, "_isVisible");
            Assert.False(isVisible);
        }

        [Fact]
        public void Hide_WhenCalled_SetsOpenedEnumEntryToNull()
        {
            // Arrange
            var guiHost = new GuiHost();
            var viewModel = CreateUninitializedViewModel();
            var uiEntryModel = CreateUninitializedUiEntryModel();
            viewModel.OpenedEnumEntry = uiEntryModel;
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.Hide();

            // Assert
            Assert.Null(viewModel.OpenedEnumEntry);
        }

        [Fact]
        public void Hide_WhenCalled_SetsHasDraggedWindowSinceOpenToFalse()
        {
            // Arrange
            var guiHost = new GuiHost();
            SetPrivateField(guiHost, "_hasDraggedWindowSinceOpen", true);
            var viewModel = CreateUninitializedViewModel();
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.Hide();

            // Assert
            var hasDraggedWindowSinceOpen = GetPrivateField<bool>(guiHost, "_hasDraggedWindowSinceOpen");
            Assert.False(hasDraggedWindowSinceOpen);
        }

        // -----------------------------------------------------------------------
        // ToggleVisibility
        // -----------------------------------------------------------------------

        [Fact]
        public void ToggleVisibility_WhenVisible_CallsHide()
        {
            // Arrange
            var guiHost = new GuiHost();
            SetPrivateField(guiHost, "_isVisible", true);
            var viewModel = CreateUninitializedViewModel();
            var uiEntryModel = CreateUninitializedUiEntryModel();
            viewModel.OpenedEnumEntry = uiEntryModel;
            SetPrivateField(guiHost, "_viewModel", viewModel);
            SetPrivateField(guiHost, "_hasDraggedWindowSinceOpen", true);

            // Act
            guiHost.ToggleVisibility();

            // Assert
            var isVisible = GetPrivateField<bool>(guiHost, "_isVisible");
            Assert.False(isVisible);
            Assert.Null(viewModel.OpenedEnumEntry);
            var hasDraggedWindowSinceOpen = GetPrivateField<bool>(guiHost, "_hasDraggedWindowSinceOpen");
            Assert.False(hasDraggedWindowSinceOpen);
        }

        [Fact]
        public void ToggleVisibility_WhenNotVisible_SetsIsVisibleToTrue()
        {
            // Arrange
            var guiHost = new GuiHost();
            SetPrivateField(guiHost, "_isVisible", false);
            var viewModel = CreateUninitializedViewModel();
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.ToggleVisibility();

            // Assert
            var isVisible = GetPrivateField<bool>(guiHost, "_isVisible");
            Assert.True(isVisible);
        }

        private static ViewModel CreateUninitializedViewModel()
        {
            return (ViewModel)RuntimeHelpers.GetUninitializedObject(typeof(ViewModel));
        }

        private static UiEntryModel CreateUninitializedUiEntryModel()
        {
            return (UiEntryModel)RuntimeHelpers.GetUninitializedObject(typeof(UiEntryModel));
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }
    }
}
