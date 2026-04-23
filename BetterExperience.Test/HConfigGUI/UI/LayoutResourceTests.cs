using BetterExperience.HConfigSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HProvider;
using BetterExperience.HTranslatorSpace;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BetterExperience.Test.HConfigGUI.UI
{
    public class LayoutResourceTests
    {
        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithNullContext_SetsContextFieldToNull()
        {
            // Arrange & Act
            var layoutResource = new LayoutResource(null);

            // Assert
            var context = GetPrivateField<ViewModel>(layoutResource, "_context");
            Assert.Null(context);
        }

        [Fact]
        public void Constructor_WithValidContext_SetsContextField()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();

            // Act
            var layoutResource = new LayoutResource(viewModel);

            // Assert
            var context = GetPrivateField<ViewModel>(layoutResource, "_context");
            Assert.Same(viewModel, context);
        }

        // -----------------------------------------------------------------------
        // GetLabelWidth
        // -----------------------------------------------------------------------

        [Fact]
        public void GetLabelWidth_WhenContextIsNull_ReturnsZero()
        {
            // Arrange
            var layoutResource = new LayoutResource(null);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetLabelWidth_WhenSheetIsNull_ReturnsZero()
        {
            // Arrange
            var viewModel = CreateUninitializedViewModel();
            SetPrivateAutoProperty(viewModel, "Sheet", null);
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetLabelWidth_WhenSheetIsEmpty_ReturnsZero()
        {
            // Arrange
            var viewModel = CreateViewModelWithEmptySheet();
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetLabelWidth_WithSingleTableAndEntry_ReturnsWidthPlusMargin()
        {
            // Arrange
            // Fallback calculation: nameLength * 8f
            // For 100f, need length 13 (13 * 8 = 104)
            var viewModel = CreateViewModelWithEntries(100f);
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            // Expected: 104f (13 chars * 8) + 10f margin = 114f
            Assert.Equal(114f, result);
        }

        [Fact]
        public void GetLabelWidth_WithMultipleEntriesDifferentWidths_ReturnsMaxWidthPlusMargin()
        {
            // Arrange
            // Widths: 50f (7 chars * 8 = 56), 120f (15 chars * 8 = 120), 80f (10 chars * 8 = 80)
            var viewModel = CreateViewModelWithMultipleEntries(new[] { 50f, 120f, 80f });
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            // Max width is 120f, with margin: 120f + 10f = 130f
            Assert.Equal(130f, result);
        }

        [Fact]
        public void GetLabelWidth_WithMultipleTables_ReturnsMaxWidthAcrossAllTables()
        {
            // Arrange
            // Table 1: 60f (8 chars * 8 = 64), 70f (9 chars * 8 = 72)
            // Table 2: 90f (12 chars * 8 = 96), 110f (14 chars * 8 = 112)
            // Table 3: 40f (5 chars * 8 = 40), 50f (7 chars * 8 = 56)
            var viewModel = CreateViewModelWithMultipleTables(
                new[] { 60f, 70f },
                new[] { 90f, 110f },
                new[] { 40f, 50f }
            );
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            // Max width is 112f (14 chars * 8), with margin: 112f + 10f = 122f
            Assert.Equal(122f, result);
        }

        [Fact]
        public void GetLabelWidth_WithZeroWidthEntries_ReturnsMarginOnly()
        {
            // Arrange
            // For 0f width, nameLength = 0, so 0 * 8 = 0f
            var viewModel = CreateViewModelWithEntries(0f);
            var layoutResource = new LayoutResource(viewModel);

            // Act
            var result = layoutResource.GetLabelWidth();

            // Assert
            // Expected: 0f + 10f margin = 10f
            Assert.Equal(10f, result);
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private ViewModel CreateUninitializedViewModel()
        {
            return (ViewModel)RuntimeHelpers.GetUninitializedObject(typeof(ViewModel));
        }

        private UiSheetModel CreateUninitializedUiSheetModel()
        {
            return (UiSheetModel)RuntimeHelpers.GetUninitializedObject(typeof(UiSheetModel));
        }

        private UiTableModel CreateUninitializedUiTableModel()
        {
            return (UiTableModel)RuntimeHelpers.GetUninitializedObject(typeof(UiTableModel));
        }

        private UiEntryModel CreateUninitializedUiEntryModel()
        {
            return (UiEntryModel)RuntimeHelpers.GetUninitializedObject(typeof(UiEntryModel));
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new System.Exception($"Could not find field {fieldName} in type {obj.GetType().Name}");
            }
            field.SetValue(obj, value);
        }

        private void SetPrivateAutoProperty(object obj, string propertyName, object value)
        {
            var backingField = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField == null)
            {
                throw new System.Exception($"Could not find backing field for property {propertyName} in type {obj.GetType().Name}");
            }
            backingField.SetValue(obj, value);
        }

        private ViewModel CreateViewModelWithEmptySheet()
        {
            var viewModel = CreateUninitializedViewModel();
            var sheet = CreateUninitializedUiSheetModel();
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel>());
            SetPrivateAutoProperty(viewModel, "Sheet", sheet);
            return viewModel;
        }

        private ViewModel CreateViewModelWithEntries(float entryWidth)
        {
            // Note: entryWidth parameter is not used because we can't mock GUIStyle.CalcSize
            // The code will fall back to: entry.Name.ToString().Length * 8f
            // To get width of 100f, we need a name of length 100/8 = 12.5, so use 13 chars
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
            SetPrivateAutoProperty(viewModel, "UnityGuiService", unityGuiService);

            var entry = CreateUninitializedUiEntryModel();
            // Create translator with name length that will give us desired width via fallback
            // fallback formula: text.Length * 8f
            // For 100f width: length should be 100/8 = 12.5, round to 13
            var nameLength = (int)System.Math.Ceiling(entryWidth / 8f);
            var name = new string('X', nameLength);
            var translator = new Translator(name, name);
            
            // Set up mock IConfigEntry with the translator
            var mockConfigEntry = new Mock<IConfigEntry>();
            mockConfigEntry.Setup(e => e.Name).Returns(translator);
            SetPrivateField(entry, "_entry", mockConfigEntry.Object);

            var table = CreateUninitializedUiTableModel();
            SetPrivateAutoProperty(table, "Table", new List<UiEntryModel> { entry });

            var sheet = CreateUninitializedUiSheetModel();
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel> { table });

            SetPrivateAutoProperty(viewModel, "Sheet", sheet);
            return viewModel;
        }

        private ViewModel CreateViewModelWithMultipleEntries(float[] entryWidths)
        {
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
            SetPrivateAutoProperty(viewModel, "UnityGuiService", unityGuiService);

            var entries = new List<UiEntryModel>();
            for (int i = 0; i < entryWidths.Length; i++)
            {
                var entry = CreateUninitializedUiEntryModel();
                var nameLength = (int)System.Math.Ceiling(entryWidths[i] / 8f);
                var name = new string('X', nameLength);
                var translator = new Translator(name, name);
                
                var mockConfigEntry = new Mock<IConfigEntry>();
                mockConfigEntry.Setup(e => e.Name).Returns(translator);
                SetPrivateField(entry, "_entry", mockConfigEntry.Object);
                
                entries.Add(entry);
            }

            var table = CreateUninitializedUiTableModel();
            SetPrivateAutoProperty(table, "Table", entries);

            var sheet = CreateUninitializedUiSheetModel();
            SetPrivateAutoProperty(sheet, "Sheet", new List<UiTableModel> { table });

            SetPrivateAutoProperty(viewModel, "Sheet", sheet);
            return viewModel;
        }

        private ViewModel CreateViewModelWithMultipleTables(params float[][] tableEntryWidths)
        {
            var viewModel = CreateUninitializedViewModel();
            var unityGuiService = (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
            SetPrivateAutoProperty(viewModel, "UnityGuiService", unityGuiService);

            var tables = new List<UiTableModel>();

            int entryIndex = 0;
            foreach (var widths in tableEntryWidths)
            {
                var entries = new List<UiEntryModel>();
                foreach (var width in widths)
                {
                    var entry = CreateUninitializedUiEntryModel();
                    var nameLength = (int)System.Math.Ceiling(width / 8f);
                    var name = new string('X', nameLength);
                    var translator = new Translator(name, name);
                    
                    var mockConfigEntry = new Mock<IConfigEntry>();
                    mockConfigEntry.Setup(e => e.Name).Returns(translator);
                    SetPrivateField(entry, "_entry", mockConfigEntry.Object);
                    
                    entries.Add(entry);
                    entryIndex++;
                }

                var table = CreateUninitializedUiTableModel();
                SetPrivateAutoProperty(table, "Table", entries);
                tables.Add(table);
            }

            var sheet = CreateUninitializedUiSheetModel();
            SetPrivateAutoProperty(sheet, "Sheet", tables);

            SetPrivateAutoProperty(viewModel, "Sheet", sheet);
            return viewModel;
        }

        private UnityGuiProvider CreateMockUnityGuiService(float width)
        {
            // This method is no longer needed since we use uninitialized instances
            // Kept for potential future use
            return (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
        }

        private UnityGuiProvider CreateMockUnityGuiServiceWithMultipleWidths(float[] widths)
        {
            // This method is no longer needed since we use uninitialized instances
            // Kept for potential future use
            return (UnityGuiProvider)RuntimeHelpers.GetUninitializedObject(typeof(UnityGuiProvider));
        }
    }

    internal interface ICalcSizeHelper
    {
        Vector2 CalcSize(GUIContent content);
    }
}
