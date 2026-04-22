using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        [Fact]
        public void Hide_WhenCalled_SetsOpenedHotkeyEntryToNull()
        {
            // Arrange
            var guiHost = new GuiHost();
            var viewModel = CreateUninitializedViewModel();
            var uiEntryModel = CreateUninitializedUiEntryModel();
            viewModel.OpenedHotkeyEntry = uiEntryModel;
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.Hide();

            // Assert
            Assert.Null(viewModel.OpenedHotkeyEntry);
        }

        [Fact]
        public void Hide_WhenCalled_SetsRecordingHotkeyToNull()
        {
            // Arrange
            var guiHost = new GuiHost();
            var viewModel = CreateUninitializedViewModel();
            SetPrivateField(guiHost, "_viewModel", viewModel);

            // Act
            guiHost.Hide();

            // Assert
            Assert.Null(viewModel.RecordingHotkey);
        }

        [Fact]
        public void Hide_WhenRecordingHotkeyIsNotNull_DoesNotModifyState()
        {
            // Arrange
            var guiHost = new GuiHost();
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityService), new UnityProvider());
            var hotkeyChord = CreateUninitializedHotkeyChord();
            var enumEntry = CreateUninitializedUiEntryModel();
            viewModel.RecordingHotkey = hotkeyChord;
            viewModel.ToastDuration = 2f;
            viewModel.OpenedEnumEntry = enumEntry;
            SetPrivateField(guiHost, "_viewModel", viewModel);
            SetPrivateField(guiHost, "_isVisible", true);
            SetPrivateField(guiHost, "_hasDraggedWindowSinceOpen", true);

            // Act
            var exception = Record.Exception(() => guiHost.Hide());

            // Assert - verify state was not modified (early return happened)
            Assert.True(exception is null or System.Security.SecurityException);
            var isVisible = GetPrivateField<bool>(guiHost, "_isVisible");
            Assert.True(isVisible);
            Assert.NotNull(viewModel.OpenedEnumEntry);
            Assert.NotNull(viewModel.RecordingHotkey);
            var hasDraggedWindowSinceOpen = GetPrivateField<bool>(guiHost, "_hasDraggedWindowSinceOpen");
            Assert.True(hasDraggedWindowSinceOpen);
        }

        [Fact]
        public void Hide_WhenRecordingHotkeyIsNotNull_ReturnsEarly()
        {
            // Arrange
            var guiHost = new GuiHost();
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, nameof(ViewModel.UnityService), new UnityProvider());
            var hotkeyChord = CreateUninitializedHotkeyChord();
            viewModel.RecordingHotkey = hotkeyChord;
            viewModel.ToastDuration = 2f;
            SetPrivateField(guiHost, "_viewModel", viewModel);
            SetPrivateField(guiHost, "_isVisible", true);

            // Act
            var exception = Record.Exception(() => guiHost.Hide());

            // Assert - verify early return by checking _isVisible was not changed
            Assert.True(exception is null or System.Security.SecurityException);
            var isVisible = GetPrivateField<bool>(guiHost, "_isVisible");
            Assert.True(isVisible);
        }

        private static HotkeyChord CreateUninitializedHotkeyChord()
        {
            return (HotkeyChord)RuntimeHelpers.GetUninitializedObject(typeof(HotkeyChord));
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

        private static void SetPrivateAutoProperty(object obj, string propertyName, object value)
        {
            var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            backingField.SetValue(obj, value);
        }
    }
}
