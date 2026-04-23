using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI;
using System.Collections;

namespace BetterExperience.Test.HConfigGUI
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

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsIEnumeratorOfUiTableModel()
        {
            var model = new UiSheetModel();

            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            Assert.IsAssignableFrom<IEnumerator<UiTableModel>>(enumerator);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsSameItemsAsGeneric()
        {
            var model = new UiSheetModel();

            var genericItems = new List<UiTableModel>();
            foreach (var item in model)
            {
                genericItems.Add(item);
            }

            var nonGenericItems = new List<object>();
            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                nonGenericItems.Add(enumerator.Current);
            }

            Assert.Equal(genericItems.Count, nonGenericItems.Count);
            for (int i = 0; i < genericItems.Count; i++)
            {
                Assert.Same(genericItems[i], nonGenericItems[i]);
            }
        }

        [Fact]
        public void GetEnumerator_NonGeneric_SupportsMultipleEnumerations()
        {
            var model = new UiSheetModel();
            var enumerable = (IEnumerable)model;

            var enumerator1 = enumerable.GetEnumerator();
            var count1 = 0;
            while (enumerator1.MoveNext())
            {
                count1++;
            }

            var enumerator2 = enumerable.GetEnumerator();
            var count2 = 0;
            while (enumerator2.MoveNext())
            {
                count2++;
            }

            Assert.Equal(count1, count2);
            Assert.Equal(model.Sheet.Count, count1);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_MoveNextReturnsFalseAfterLastItem()
        {
            var model = new UiSheetModel();
            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            var expectedCount = model.Sheet.Count;
            var moveNextCount = 0;
            while (enumerator.MoveNext())
            {
                moveNextCount++;
            }

            Assert.Equal(expectedCount, moveNextCount);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void GetEnumerator_NonGeneric_CurrentReturnsUiTableModel()
        {
            var model = new UiSheetModel();
            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                Assert.NotNull(current);
                Assert.IsType<UiTableModel>(current);
            }
        }

        [Fact]
        public void GetEnumerator_NonGeneric_EnumeratesInSameOrderAsSheet()
        {
            var model = new UiSheetModel();
            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            var index = 0;
            while (enumerator.MoveNext())
            {
                Assert.Same(model.Sheet[index], enumerator.Current);
                index++;
            }
        }

        [Fact]
        public void GetEnumerator_NonGeneric_EnumeratorIsIndependentFromSheet()
        {
            var model = new UiSheetModel();
            var enumerable = (IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();

            var originalCount = model.Sheet.Count;
            var enumeratedCount = 0;
            while (enumerator.MoveNext())
            {
                enumeratedCount++;
            }

            Assert.Equal(originalCount, enumeratedCount);
        }
    }
}
