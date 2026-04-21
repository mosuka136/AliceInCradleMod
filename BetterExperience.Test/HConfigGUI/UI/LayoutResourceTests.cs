using BetterExperience.BConfigManager;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace BetterExperience.Test.HConfigGUI.UI
{
    public class LayoutResourceTests : IDisposable
    {
        private readonly string _tempConfigPath;

        public LayoutResourceTests()
        {
            _tempConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            ConfigManager.Initialize(_tempConfigPath);
            LayoutResource.InvalidateLayout();
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

        private UiSheetModel CreateEmptySheet()
        {
            var sheet = new UiSheetModel();
            var sheetField = typeof(UiSheetModel).GetField("<Sheet>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sheetField != null)
            {
                sheetField.SetValue(sheet, new List<UiTableModel>());
            }
            return sheet;
        }

        private void SetCachedLabelWidth(float value)
        {
            var field = typeof(LayoutResource).GetField("_labelWidth", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                field.SetValue(null, value);
            }
        }

        private float GetCachedLabelWidth()
        {
            var field = typeof(LayoutResource).GetField("_labelWidth", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                return (float)field.GetValue(null);
            }
            return 0f;
        }

        private UiEntryModel CreateUninitializedUiEntryModel()
        {
            return (UiEntryModel)RuntimeHelpers.GetUninitializedObject(typeof(UiEntryModel));
        }

        private UiTableModel CreateUninitializedUiTableModel()
        {
            return (UiTableModel)RuntimeHelpers.GetUninitializedObject(typeof(UiTableModel));
        }

        private UiEntryModel CreateUiEntryModelWithMockEntry(string name)
        {
            var entry = CreateUninitializedUiEntryModel();
            var mockConfigEntry = new Mock<IConfigEntry>();
            mockConfigEntry.Setup(e => e.Name).Returns(new Translator(name, name));
            mockConfigEntry.Setup(e => e.Description).Returns(new Translator(name, name));
            mockConfigEntry.Setup(e => e.Key).Returns("TestKey");

            var field = typeof(UiEntryModel).GetField("_entry", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(entry, mockConfigEntry.Object);

            return entry;
        }

        private UiTableModel CreateUiTableModelWithEntries(string name, UiEntryModel[] entries)
        {
            var table = CreateUninitializedUiTableModel();
            var tableDescription = new Translator(name, name);
            var configTable = new ConfigTable("TestTableKey", new ConfigFileTableModel("TestTableKey", tableDescription), new Translator(name, name), tableDescription);

            var tableField = typeof(UiTableModel).GetField("_table", BindingFlags.NonPublic | BindingFlags.Instance);
            tableField.SetValue(table, configTable);

            var entriesField = typeof(UiTableModel).GetField("<Table>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (entriesField != null)
            {
                entriesField.SetValue(table, new List<UiEntryModel>(entries));
            }

            return table;
        }

        private UiSheetModel CreateSheetWithTables(UiTableModel[] tables)
        {
            var sheet = new UiSheetModel();
            var sheetField = typeof(UiSheetModel).GetField("<Sheet>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sheetField != null)
            {
                sheetField.SetValue(sheet, new List<UiTableModel>(tables));
            }
            return sheet;
        }

        [Fact]
        public void InvalidateLayout_ShouldResetCachedLabelWidth()
        {
            // Arrange
            SetCachedLabelWidth(100f);

            // Act
            LayoutResource.InvalidateLayout();

            // Assert
            var cachedWidth = GetCachedLabelWidth();
            Assert.Equal(-1f, cachedWidth);
        }

        [Fact]
        public void GetLabelWidth_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            SetCachedLabelWidth(150f);
            var sheet = CreateEmptySheet();

            // Act
            var result = LayoutResource.GetLabelWidth(sheet);

            // Assert
            Assert.Equal(150f, result);
        }

        [Fact]
        public void GetLabelWidth_WithEntries_CalculatesAndCachesWidth()
        {
            // Arrange
            LayoutResource.InvalidateLayout();
            var entry1 = CreateUiEntryModelWithMockEntry("Short");
            var entry2 = CreateUiEntryModelWithMockEntry("VeryLongEntryName");
            var entry3 = CreateUiEntryModelWithMockEntry("Medium");
            var table = CreateUiTableModelWithEntries("TestTable", new[] { entry1, entry2, entry3 });
            var sheet = CreateSheetWithTables(new[] { table });

            // Act
            var result = LayoutResource.GetLabelWidth(sheet);

            // Assert
            Assert.True(result > 0f);
            var cached = GetCachedLabelWidth();
            Assert.Equal(result, cached);
        }

        [Fact]
        public void GetLabelWidth_WithMultipleTables_ProcessesAllEntries()
        {
            // Arrange
            LayoutResource.InvalidateLayout();
            var entry1 = CreateUiEntryModelWithMockEntry("Entry1");
            var entry2 = CreateUiEntryModelWithMockEntry("Entry2");
            var table1 = CreateUiTableModelWithEntries("Table1", new[] { entry1 });
            var table2 = CreateUiTableModelWithEntries("Table2", new[] { entry2 });
            var sheet = CreateSheetWithTables(new[] { table1, table2 });

            // Act
            var result = LayoutResource.GetLabelWidth(sheet);

            // Assert
            Assert.True(result > 0f);
            var cached = GetCachedLabelWidth();
            Assert.Equal(result, cached);
        }

        [Fact]
        public void GetLabelWidth_AddsMarginToMaxWidth()
        {
            // Arrange
            LayoutResource.InvalidateLayout();
            var entry = CreateUiEntryModelWithMockEntry("TestEntry");
            var table = CreateUiTableModelWithEntries("TestTable", new[] { entry });
            var sheet = CreateSheetWithTables(new[] { table });

            // Act
            var result = LayoutResource.GetLabelWidth(sheet);

            // Assert
            var cached = GetCachedLabelWidth();
            Assert.Equal(result, cached);
            Assert.True(result >= 10f);
        }

    }
}
