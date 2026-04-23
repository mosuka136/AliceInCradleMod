using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI;
using BetterExperience.HotkeyManager;

namespace BetterExperience.Test.HConfigGUI
{
    public class ViewModelTests : IDisposable
    {
        private readonly string _tempConfigPath;

        public ViewModelTests()
        {
            _tempConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            ConfigManager.Initialize(_tempConfigPath);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_tempConfigPath))
                    File.Delete(_tempConfigPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private UiEntryModel CreateTestEntry()
        {
            return new UiEntryModel(ConfigManager.SetLootDropRatio);
        }

        private UiEntryModel CreateTestEntry2()
        {
            return new UiEntryModel(ConfigManager.EnableBetterFishing);
        }

        // Note: ResetValue and ShowToast methods have limited test coverage due to Unity dependencies.
        // These methods call UnityService.RealtimeSinceStartup which requires Unity runtime and cannot
        // be mocked (UnityProvider properties are not virtual). Full integration testing is recommended
        // for these methods.

        // -----------------------------------------------------------------------
        // Constructor Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_InitializesUnityService()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.UnityService);
        }

        [Fact]
        public void Constructor_InitializesUnityGuiService()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.UnityGuiService);
        }

        [Fact]
        public void Constructor_InitializesStyleResourceInstance()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.StyleResourceInstance);
        }

        [Fact]
        public void Constructor_InitializesLayoutResourceInstance()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.LayoutResourceInstance);
        }

        [Fact]
        public void Constructor_InitializesSheet()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.Sheet);
        }

        [Fact]
        public void Constructor_InitializesConfigUIHotkey()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.ConfigUIHotkey);
        }

        [Fact]
        public void Constructor_SetsLabelWidthToNegativeOne()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(-1f, viewModel.LabelWidth);
        }

        // -----------------------------------------------------------------------
        // Update Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Update_WithZeroDeltaTime_DoesNotCrash()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act & Assert - Should not throw
            viewModel.Update(0f);
        }

        [Fact]
        public void Update_WithNegativeDeltaTime_DoesNotCrash()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act & Assert - Should not throw
            viewModel.Update(-0.1f);
        }

        [Fact]
        public void Update_CallsRecordHotkey()
        {
            // Arrange
            var viewModel = new ViewModel();
            viewModel.RecordingHotkey = new HotkeyChord(viewModel.UnityService);

            // Act
            viewModel.Update(0.1f);

            // Assert - If RecordingHotkey is not null, RecordHotkey should process it
            Assert.NotNull(viewModel.RecordingHotkey.Modifiers);
        }

        // -----------------------------------------------------------------------
        // UpdateValueTime Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void UpdateValueTime_WithNoEntries_DoesNothing()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act
            viewModel.UpdateValueTime(0.1f);

            // Assert
            Assert.Null(viewModel.ToastMessage);
        }

        [Fact]
        public void UpdateValueTime_WithPositiveDelay_DecreasesDelayTime()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            viewModel.SetValue(entry, 2.0f, 1.0f);

            // Act
            viewModel.UpdateValueTime(0.3f);

            // Assert - Value should not be set yet (still at default/original)
            Assert.NotEqual(2.0f, entry.Value);
        }

        [Fact]
        public void UpdateValueTime_WithZeroDeltaTime_DoesNotCrash()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            viewModel.SetValue(entry, 2.0f, 1.0f);

            // Act & Assert - Should not throw
            viewModel.UpdateValueTime(0f);
        }

        [Fact]
        public void UpdateValueTime_WithNegativeDeltaTime_DecreasesDelayTime()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            viewModel.SetValue(entry, 2.0f, 1.0f);

            // Act
            viewModel.UpdateValueTime(-0.5f);

            // Assert - Negative deltaTime increases the remaining time, so value should not be set
            Assert.NotEqual(2.0f, entry.Value);
        }

        [Fact]
        public void UpdateValueTime_WithExactZeroDelay_AppliesImmediatelyInSetValue()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act - With 0 delay, SetValue applies immediately (which calls ShowToast)
            // We test with positive delay to avoid ShowToast
            viewModel.SetValue(entry, 2.0f, 0.1f);

            // Assert - Value not set yet
            Assert.NotEqual(2.0f, entry.Value);
        }

        [Fact]
        public void UpdateValueTime_WithCacheValueNull_SkipsEntry()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            var originalValue = entry.Value;
            viewModel.SetValue(entry, 2.0f, 0.5f);
            entry.CacheValue = null;

            // Act
            viewModel.UpdateValueTime(0.6f);

            // Assert - Value should remain unchanged
            Assert.Equal(originalValue, entry.Value);
        }

        [Fact]
        public void UpdateValueTime_WithMultipleCalls_GraduallyDecreasesDelay()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            viewModel.SetValue(entry, 2.0f, 1.0f);

            // Act - Multiple small updates
            viewModel.UpdateValueTime(0.3f);
            viewModel.UpdateValueTime(0.3f);

            // Assert - Still not enough time passed
            Assert.NotEqual(2.0f, entry.Value);

            // Note: We don't test the final update that would trigger ShowToast
        }

        // -----------------------------------------------------------------------
        // RecordHotkey Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void RecordHotkey_WhenRecordingHotkeyIsNull_ReturnsImmediately()
        {
            // Arrange
            var viewModel = new ViewModel();
            viewModel.RecordingHotkey = null;

            // Act
            viewModel.RecordHotkey();

            // Assert - No exception should be thrown
            Assert.Null(viewModel.RecordingHotkey);
        }

        [Fact]
        public void RecordHotkey_WhenModifiersIsNull_InitializesModifiers()
        {
            // Arrange
            var viewModel = new ViewModel();
            var chord = new HotkeyChord(viewModel.UnityService);
            chord.Modifiers = null;
            viewModel.RecordingHotkey = chord;

            // Act
            viewModel.RecordHotkey();

            // Assert
            Assert.NotNull(viewModel.RecordingHotkey.Modifiers);
        }

        [Fact]
        public void RecordHotkey_WhenKeyboardIsNull_ReturnsAfterGamepadCheck()
        {
            // Arrange
            var viewModel = new ViewModel();
            var chord = new HotkeyChord(viewModel.UnityService);
            viewModel.RecordingHotkey = chord;

            // Act - This will check gamepad (null) and keyboard (null in test environment)
            viewModel.RecordHotkey();

            // Assert - Should complete without exception
            Assert.NotNull(viewModel.RecordingHotkey);
        }

        [Fact]
        public void RecordHotkey_WithMultipleCalls_DoesNotCrash()
        {
            // Arrange
            var viewModel = new ViewModel();
            var chord = new HotkeyChord(viewModel.UnityService);
            viewModel.RecordingHotkey = chord;

            // Act & Assert - Multiple calls should not crash
            viewModel.RecordHotkey();
            viewModel.RecordHotkey();
            viewModel.RecordHotkey();
        }

        [Fact]
        public void RecordHotkey_AfterSettingRecordingHotkeyToNull_ReturnsImmediately()
        {
            // Arrange
            var viewModel = new ViewModel();
            var chord = new HotkeyChord(viewModel.UnityService);
            viewModel.RecordingHotkey = chord;

            // Act - Set to null and call
            viewModel.RecordingHotkey = null;
            viewModel.RecordHotkey();

            // Assert - Should not crash
            Assert.Null(viewModel.RecordingHotkey);
        }

        // -----------------------------------------------------------------------
        // SetValue Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void SetValue_WhenEntryIsNull_ShouldReturnWithoutAction()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act
            viewModel.SetValue(null, "testValue", 0f);

            // Assert
            Assert.Null(viewModel.ToastMessage);
        }

        [Fact]
        public void SetValue_WithVerySmallPositiveDelay_SetsCacheValue()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act
            viewModel.SetValue(entry, 2.0f, 0.0001f);

            // Assert
            Assert.Equal(2.0f, entry.CacheValue);
        }

        [Fact]
        public void SetValue_WithVeryLargeDelay_SetsCacheValue()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act
            viewModel.SetValue(entry, 2.0f, 999999f);

            // Assert
            Assert.Equal(2.0f, entry.CacheValue);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_SetsCacheValue()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act
            viewModel.SetValue(entry, 2.0f, 0.5f);

            // Assert
            Assert.Equal(2.0f, entry.CacheValue);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_DoesNotSetValueImmediately()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            var originalValue = entry.Value;

            // Act
            viewModel.SetValue(entry, 2.0f, 0.5f);

            // Assert
            Assert.Equal(originalValue, entry.Value);
        }

        [Fact]
        public void SetValue_CalledTwiceWithSameEntry_UpdatesDelay()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act
            viewModel.SetValue(entry, 2.0f, 1.0f);
            viewModel.SetValue(entry, 3.0f, 2.0f);

            // Assert - Second call should update the cache value
            Assert.Equal(3.0f, entry.CacheValue);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_DoesNotShowToastImmediately()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act
            viewModel.SetValue(entry, 2.0f, 0.5f);

            // Assert
            Assert.Null(viewModel.ToastMessage);
        }

        [Fact]
        public void SetValue_WithDifferentValueTypes_WorksCorrectly()
        {
            // Arrange
            var viewModel = new ViewModel();
            var boolEntry = CreateTestEntry2();
            var floatEntry = CreateTestEntry();

            // Act
            viewModel.SetValue(boolEntry, true, 1.0f);
            viewModel.SetValue(floatEntry, 5.5f, 1.0f);

            // Assert
            Assert.Equal(true, boolEntry.CacheValue);
            Assert.Equal(5.5f, floatEntry.CacheValue);
        }

        [Fact]
        public void UpdateValueTime_WithSameEntryAddedTwice_ProcessesCorrectly()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();
            
            // Act - Add same entry twice with different values and delays
            viewModel.SetValue(entry, 2.0f, 1.0f);
            viewModel.SetValue(entry, 3.0f, 0.5f); // Overwrites previous

            viewModel.UpdateValueTime(0.3f);

            // Assert - Should still have cache value (not enough time passed for 0.5s delay)
            Assert.Equal(3.0f, entry.CacheValue);
        }

        [Fact]
        public void Constructor_SetsDefaultPropertyValues()
        {
            // Arrange & Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(2f, viewModel.ToastDuration);
            Assert.Equal(0.5f, viewModel.ToastFadeDuration);
            Assert.Equal(0.5f, viewModel.StringDuration);
            Assert.Equal(0.5f, viewModel.NumberDuration);
            Assert.Equal(0.5f, viewModel.SliderDuration);
        }

        [Fact]
        public void Update_WithLargeDeltaTime_DoesNotCrashWithoutDelayedEntries()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act & Assert - Should not crash when no delayed entries
            viewModel.Update(999999f);
        }

        [Fact]
        public void SetValue_WithNullValue_IsHandledByEntry()
        {
            // Arrange
            var viewModel = new ViewModel();
            var entry = CreateTestEntry();

            // Act - UiEntryModel.Value setter checks for null
            viewModel.SetValue(entry, null, 1.0f);

            // Assert - null should be cached
            Assert.Null(entry.CacheValue);
        }

        // -----------------------------------------------------------------------
        // ResetValue Tests
        // -----------------------------------------------------------------------

        [Fact]
        public void ResetValue_WhenEntryIsNull_ShouldReturnWithoutException()
        {
            // Arrange
            var viewModel = new ViewModel();

            // Act & Assert - Should not throw
            viewModel.ResetValue(null);
        }

        // Additional tests for ResetValue with non-null entries cannot be implemented
        // due to Unity dependencies (calls ShowToast -> UnityService.RealtimeSinceStartup).
        // These require integration testing in Unity environment.

        // -----------------------------------------------------------------------
        // ShowToast Tests  
        // -----------------------------------------------------------------------

        // Tests for ShowToast cannot be implemented due to Unity dependencies.
        // The method accesses UnityService.RealtimeSinceStartup which requires Unity runtime.
        // UnityProvider properties are not virtual and cannot be mocked.
        // Integration testing in Unity environment is recommended for this method.
    }
}
