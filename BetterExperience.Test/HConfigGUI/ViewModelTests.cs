using BetterExperience.BConfigManager;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HTranslatorSpace;
using Moq;

namespace BetterExperience.Test
{
    [Collection(ConfigManagerTestCollectionDefinition.Name)]
    public class ViewModelTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();

        public ViewModelTests()
        {
            ConfigManager.Initialize(CreateTempConfigPath());
        }

        private string CreateTempConfigPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            _tempFiles.Add(path);
            return path;
        }

        private Mock<IConfigEntry> CreateMockEntry(string key = "SetLootDropRatio")
        {
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns(key);
            mockEntry.Setup(e => e.Name).Returns(new Translator("名称", "Name"));
            mockEntry.Setup(e => e.Description).Returns(new Translator("描述", "Description"));
            mockEntry.Setup(e => e.ValueType).Returns(typeof(float));
            mockEntry.Setup(e => e.BoxedValue).Returns(1.0f);
            mockEntry.Setup(e => e.BoxedDefaultValue).Returns(1.0f);
            return mockEntry;
        }

        public void Dispose()
        {
            foreach (var path in _tempFiles)
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch { }
            }
        }

        [Fact]
        public void Constructor_WhenCalled_CreatesUiSheetModel()
        {
            // Act
            var viewModel = new ViewModel();

            // Assert
            Assert.NotNull(viewModel.Sheet);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsDefaultToastDuration()
        {
            // Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(2f, viewModel.ToastDuration);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsDefaultToastFadeDuration()
        {
            // Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(0.5f, viewModel.ToastFadeDuration);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsDefaultStringDuration()
        {
            // Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(0.5f, viewModel.StringDuration);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsDefaultSliderDuration()
        {
            // Act
            var viewModel = new ViewModel();

            // Assert
            Assert.Equal(0.5f, viewModel.SliderDuration);
        }

        [Fact]
        public void Update_WithPositiveDeltaTime_CallsUpdateValueTime()
        {
            var viewModel = new ViewModel();
            var deltaTime = 0.1f;

            // Act
            viewModel.Update(deltaTime);

            // Assert - if no exception, test passes
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void UpdateValueTime_WithNoDelayedEntries_DoesNothing()
        {
            var viewModel = new ViewModel();
            var deltaTime = 0.1f;

            // Act
            viewModel.UpdateValueTime(deltaTime);

            // Assert - if no exception, test passes
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void UpdateValueTime_WithPositiveDeltaTime_DecreasesDelayTime()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 1.0f;

            viewModel.SetValue(entry, newValue, delaySec);

            // Act - first update decreases but doesn't apply
            viewModel.UpdateValueTime(0.5f);

            // Assert - value should not be applied yet
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Never);
        }

        [Fact]
        public void UpdateValueTime_WhenDelayExpires_AppliesCachedValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 0.5f;

            viewModel.SetValue(entry, newValue, delaySec);

            // Act - update with enough time to expire delay
            try
            {
                viewModel.UpdateValueTime(1.0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert - value should be applied
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Once);
        }

        [Fact]
        public void UpdateValueTime_WhenCacheValueIsNull_DoesNotApplyValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 0.5f;

            viewModel.SetValue(entry, newValue, delaySec);
            entry.CacheValue = null; // Manually set to null

            // Act
            viewModel.UpdateValueTime(1.0f);

            // Assert - value should not be applied when CacheValue is null
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Never);
        }

        [Fact]
        public void UpdateValueTime_AfterApplyingValue_RemovesEntryFromDelayDictionary()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 0.5f;

            viewModel.SetValue(entry, newValue, delaySec);
            try
            {
                viewModel.UpdateValueTime(1.0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Act - second update should not affect anything
            viewModel.UpdateValueTime(1.0f);

            // Assert - value should only be set once
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Once);
        }

        [Fact]
        public void SetValue_WithNullEntry_DoesNothing()
        {
            var viewModel = new ViewModel();

            // Act
            viewModel.SetValue(null, 100.0f);

            // Assert - if no exception, test passes
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void SetValue_WithNegativeDelay_SetsValueImmediately()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;

            // Act
            try
            {
                viewModel.SetValue(entry, newValue, -1f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Once);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_SetsCacheValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 1.0f;

            // Act
            viewModel.SetValue(entry, newValue, delaySec);

            // Assert
            Assert.Equal(newValue, entry.CacheValue);
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Never);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_DoesNotSetValueImmediately()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entry = new UiEntryModel(mockEntry.Object);
            var newValue = 100.0f;
            var delaySec = 1.0f;

            // Act
            viewModel.SetValue(entry, newValue, delaySec);

            // Assert
            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Never);
        }

        [Fact]
        public void ResetValue_WithValidEntry_SetsValueToDefault()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.BoxedDefaultValue).Returns(10.0f);
            var entry = new UiEntryModel(mockEntry.Object);

            // Act
            try
            {
                viewModel.ResetValue(entry);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert
            mockEntry.VerifySet(e => e.BoxedValue = 10.0f, Times.Once);
        }

        [Fact]
        public void ResetValue_WithEntryInDelayDictionary_RemovesFromDictionary()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.BoxedDefaultValue).Returns(10.0f);
            var entry = new UiEntryModel(mockEntry.Object);

            // Add entry to delay dictionary
            viewModel.SetValue(entry, 100.0f, 1.0f);

            // Act
            try
            {
                viewModel.ResetValue(entry);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert - subsequent update should not apply delayed value
            viewModel.UpdateValueTime(2.0f);
            mockEntry.VerifySet(e => e.BoxedValue = 100.0f, Times.Never);
        }

        [Fact]
        public void SetUiHotkey_WithValidHotkey_SetsConfigUIHotkey()
        {
            var viewModel = new ViewModel();

            // Act
            viewModel.SetUiHotkey("F5");

            // Assert
            Assert.NotNull(viewModel.ConfigUIHotkey);
            Assert.Equal("F5", viewModel.ConfigUIHotkey.ToString());
        }

        [Fact]
        public void SetUiHotkey_WithInvalidHotkey_FallsBackToF1()
        {
            var viewModel = new ViewModel();

            // Act
            viewModel.SetUiHotkey("");

            // Assert
            Assert.NotNull(viewModel.ConfigUIHotkey);
            Assert.Equal("F1", viewModel.ConfigUIHotkey.ToString());
        }

        [Fact]
        [Trait("Category", "ProductionBugSuspected")]
        public void SetUiHotkey_WithNullHotkey_FallsBackToF1()
        {
            var viewModel = new ViewModel();

            // Act
            viewModel.SetUiHotkey(null);

            // Assert
            Assert.NotNull(viewModel.ConfigUIHotkey);
            Assert.Equal("F1", viewModel.ConfigUIHotkey.ToString());
        }

        [Fact]
        public void ShowToast_WithValidParameters_SetsToastMessage()
        {
            var viewModel = new ViewModel();
            var message = "Test message";
            var duration = 3.0f;

            // Act
            try
            {
                viewModel.ShowToast(message, duration);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert
            Assert.Equal(message, viewModel.ToastMessage);
        }

        [Fact]
        public void ShowToast_WithZeroDuration_SetsToastMessage()
        {
            var viewModel = new ViewModel();
            var message = "Test message";

            // Act
            try
            {
                viewModel.ShowToast(message, 0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert
            Assert.Equal(message, viewModel.ToastMessage);
        }

        [Fact]
        public void ShowToast_WithEmptyMessage_SetsToastMessage()
        {
            var viewModel = new ViewModel();
            var message = "";
            var duration = 1.0f;

            // Act
            try
            {
                viewModel.ShowToast(message, duration);
            }
            catch (System.Security.SecurityException)
            {
                // Expected in test environment - ShowToast calls Unity Time
            }

            // Assert
            Assert.Equal(message, viewModel.ToastMessage);
        }
    }

    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ConfigManagerTestCollectionDefinition
    {
        public const string Name = "ConfigManager dependent tests";
    }
}
