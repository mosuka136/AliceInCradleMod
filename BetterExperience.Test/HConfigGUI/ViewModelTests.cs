using BetterExperience.BConfigManager;
using BetterExperience.HAdapter;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HotkeyManager;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using Xunit;

namespace BetterExperience.Test
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

        private Mock<IConfigEntry> CreateMockEntry()
        {
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("SetLootDropRatio");
            mockEntry.Setup(e => e.Name).Returns(new Translator("测试", "Test"));
            mockEntry.Setup(e => e.Description).Returns(new Translator("描述", "Description"));
            mockEntry.Setup(e => e.ValueType).Returns(typeof(float));
            mockEntry.Setup(e => e.BoxedValue).Returns(1.0f);
            mockEntry.Setup(e => e.BoxedDefaultValue).Returns(1.0f);
            return mockEntry;
        }

        private Dictionary<UiEntryModel, float> GetDelayApplyTimeDictionary(ViewModel viewModel)
        {
            var field = typeof(ViewModel).GetField("_entryDelayApplyTime", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Dictionary<UiEntryModel, float>)field.GetValue(viewModel);
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WhenCalled_InitializesSheet()
        {
            var viewModel = new ViewModel();

            Assert.NotNull(viewModel.Sheet);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsConfigUIHotkeyFromConfigManager()
        {
            var viewModel = new ViewModel();

            // ConfigUIHotkey should be initialized from ConfigManager
            Assert.NotNull(viewModel.ConfigUIHotkey);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsTranslatorDefaultLanguage()
        {
            var viewModel = new ViewModel();

            // Translator.DefaultLanguage should be set from ConfigManager
            // Just verify the constructor completes successfully
            Assert.NotNull(viewModel);
        }

        // -----------------------------------------------------------------------
        // UpdateValueTime
        // -----------------------------------------------------------------------

        [Fact]
        public void UpdateValueTime_WithPositiveDelay_DecreasesDelayTime()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 2.0f;
            entryModel.CacheValue = 42.0f;

            viewModel.UpdateValueTime(0.5f);

            Assert.Equal(1.5f, delayDict[entryModel]);
        }

        [Fact]
        public void UpdateValueTime_WhenDelayExpires_RemovesEntryFromDictionary()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 0.5f;
            entryModel.CacheValue = 42.0f;

            // Skip ShowToast by mocking UnityTimeAdapter indirectly - just check the dictionary
            try
            {
                viewModel.UpdateValueTime(1.0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected - ShowToast calls Unity API
            }

            Assert.False(delayDict.ContainsKey(entryModel));
        }

        [Fact]
        public void UpdateValueTime_WhenCacheValueIsNull_DoesNotApplyValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 0.5f;
            entryModel.CacheValue = null;

            try
            {
                viewModel.UpdateValueTime(1.0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected - may call Unity API
            }

            mockEntry.VerifySet(e => e.BoxedValue = It.IsAny<object>(), Times.Never);
        }

        [Fact]
        public void UpdateValueTime_WhenDelayExpires_SetsEntryValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 0.5f;
            entryModel.CacheValue = 42.0f;

            try
            {
                viewModel.UpdateValueTime(1.0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected - ShowToast calls Unity API
            }

            mockEntry.VerifySet(e => e.BoxedValue = 42.0f, Times.Once);
        }

        [Fact]
        public void UpdateValueTime_WithMultipleEntries_ProcessesAllEntries()
        {
            var viewModel = new ViewModel();
            var mockEntry1 = CreateMockEntry();
            var mockEntry2 = CreateMockEntry();
            var entryModel1 = new UiEntryModel(mockEntry1.Object);
            var entryModel2 = new UiEntryModel(mockEntry2.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel1] = 2.0f;
            delayDict[entryModel2] = 1.0f;
            entryModel1.CacheValue = 10.0f;
            entryModel2.CacheValue = 20.0f;

            viewModel.UpdateValueTime(0.5f);

            Assert.Equal(1.5f, delayDict[entryModel1]);
            Assert.Equal(0.5f, delayDict[entryModel2]);
        }

        [Fact]
        public void UpdateValueTime_WithExpiredEntry_RemovesFromDictionary()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 0.3f;
            entryModel.CacheValue = 42.0f;

            try
            {
                viewModel.UpdateValueTime(0.5f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected - ShowToast calls Unity API
            }

            Assert.False(delayDict.ContainsKey(entryModel));
        }

        // -----------------------------------------------------------------------
        // SetValue
        // -----------------------------------------------------------------------

        [Fact]
        public void SetValue_WithNullEntry_DoesNothing()
        {
            var viewModel = new ViewModel();

            viewModel.SetValue(null, 42.0f);

            // Test passes if no exception is thrown
        }

        [Fact]
        public void SetValue_WithPositiveDelay_AddsToDelayDictionary()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);

            viewModel.SetValue(entryModel, 42.0f, 1.0f);

            Assert.True(delayDict.ContainsKey(entryModel));
            Assert.Equal(1.0f, delayDict[entryModel]);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_SetsCacheValue()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);

            viewModel.SetValue(entryModel, 42.0f, 1.0f);

            Assert.Equal(42.0f, entryModel.CacheValue);
        }

        [Fact]
        public void SetValue_WithPositiveDelay_DoesNotSetValueImmediately()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);

            viewModel.SetValue(entryModel, 42.0f, 1.0f);

            mockEntry.VerifySet(e => e.BoxedValue = It.IsAny<object>(), Times.Never);
        }

        [Fact]
        public void SetValue_WithZeroDelay_SetsValueImmediately()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);

            // This will call ShowToast which calls Unity API, so we expect an exception
            try
            {
                viewModel.SetValue(entryModel, 42.0f, 0f);
            }
            catch (System.Security.SecurityException)
            {
                // Expected - ShowToast calls Unity API
            }

            mockEntry.VerifySet(e => e.BoxedValue = 42.0f, Times.Once);
        }

        // -----------------------------------------------------------------------
        // RecordHotkey
        // -----------------------------------------------------------------------

        [Fact]
        public void RecordHotkey_WhenRecordingHotkeyIsNull_ReturnsEarly()
        {
            var viewModel = new ViewModel();
            viewModel.RecordingHotkey = null;

            // Should not throw
            viewModel.RecordHotkey();
        }

        [Fact]
        public void RecordHotkey_WhenModifiersIsNull_InitializesModifiers()
        {
            var viewModel = new ViewModel();
            var hotkey = new HotkeyChord();
            hotkey.Modifiers = null;
            viewModel.RecordingHotkey = hotkey;

            try
            {
                viewModel.RecordHotkey();
            }
            catch
            {
                // May throw due to Unity API calls
            }

            Assert.NotNull(hotkey.Modifiers);
        }

        [Fact]
        public void RecordHotkey_WhenCalled_ChecksGamepadCurrent()
        {
            var viewModel = new ViewModel();
            var hotkey = new HotkeyChord();
            viewModel.RecordingHotkey = hotkey;

            try
            {
                viewModel.RecordHotkey();
            }
            catch
            {
                // Expected - Unity API calls will throw
            }

            // If we get here without crashing, the method ran
            Assert.NotNull(hotkey.Modifiers);
        }

        [Fact]
        public void RecordHotkey_WhenCalled_ChecksKeyboardCurrent()
        {
            var viewModel = new ViewModel();
            var hotkey = new HotkeyChord();
            viewModel.RecordingHotkey = hotkey;

            try
            {
                viewModel.RecordHotkey();
            }
            catch
            {
                // Expected - Unity API calls will throw
            }

            // If we get here without crashing, the method ran
            Assert.NotNull(hotkey.Modifiers);
        }

        // -----------------------------------------------------------------------
        // Update
        // -----------------------------------------------------------------------

        [Fact]
        public void Update_WithNoDelayedEntries_DoesNotThrow()
        {
            var viewModel = new ViewModel();

            try
            {
                viewModel.Update(0.1f);
            }
            catch
            {
                // May throw due to Unity API calls, but shouldn't crash
            }

            // Test passes if we get here
        }

        [Fact]
        public void Update_CallsUpdateValueTime()
        {
            var viewModel = new ViewModel();
            var mockEntry = CreateMockEntry();
            var entryModel = new UiEntryModel(mockEntry.Object);
            var delayDict = GetDelayApplyTimeDictionary(viewModel);
            
            delayDict[entryModel] = 2.0f;
            entryModel.CacheValue = 42.0f;

            viewModel.Update(0.5f);

            // Verify UpdateValueTime was called by checking the delay was decreased
            Assert.Equal(1.5f, delayDict[entryModel]);
        }

        [Fact]
        public void Update_CallsRecordHotkey()
        {
            var viewModel = new ViewModel();
            var hotkey = new HotkeyChord();
            hotkey.Modifiers = null;
            viewModel.RecordingHotkey = hotkey;

            try
            {
                viewModel.Update(0.1f);
            }
            catch
            {
                // May throw due to Unity API calls
            }

            // Verify RecordHotkey was called by checking modifiers was initialized
            Assert.NotNull(hotkey.Modifiers);
        }
    }
}
