using BetterExperience.BConfigManager;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace BetterExperience.Test
{
    public class UiSheetModelTests : IDisposable
    {
        private readonly string _tempConfigPath;

        public UiSheetModelTests()
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

        [Fact]
        public void Constructor_InitializesSheet_WithTablesFromConfigManager()
        {
            var model = new UiSheetModel();

            Assert.NotNull(model.Sheet);
            Assert.Equal(ConfigManager.Sheet.Count, model.Sheet.Count);
        }

        [Fact]
        public void Constructor_CreatesUiTableModels_ForEachConfigTable()
        {
            var model = new UiSheetModel();

            Assert.All(model.Sheet, item => Assert.IsType<UiTableModel>(item));
        }

        [Fact]
        public void Constructor_SheetCount_MatchesConfigManagerSheetCount()
        {
            var expectedCount = ConfigManager.Sheet.Count;

            var model = new UiSheetModel();

            Assert.Equal(expectedCount, model.Sheet.Count);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsEnumerator()
        {
            var model = new UiSheetModel();

            var enumerator = model.GetEnumerator();

            Assert.NotNull(enumerator);
        }

        [Fact]
        public void GetEnumerator_Generic_EnumeratesAllItems()
        {
            var model = new UiSheetModel();

            var items = new List<UiTableModel>();
            foreach (var item in model)
            {
                items.Add(item);
            }

            Assert.Equal(model.Sheet.Count, items.Count);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsEnumerator()
        {
            var model = new UiSheetModel();

            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            Assert.NotNull(enumerator);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_EnumeratesAllItems()
        {
            var model = new UiSheetModel();

            var enumerable = (IEnumerable)model;
            var items = new List<object>();
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            Assert.Equal(model.Sheet.Count, items.Count);
        }
    }
}
