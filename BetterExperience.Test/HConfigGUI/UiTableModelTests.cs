using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BetterExperience.Test
{
    public class UiTableModelTests
    {
        private ConfigTable CreateConfigTable()
        {
            var name = new Translator("表名", "Table Name");
            var description = new Translator("表描述", "Table Description");
            return new ConfigTable("TestTable", new ConfigFileTableModel("TestTable", description), name, description);
        }

        [Fact]
        public void Name_WhenAccessed_ReturnsTableName()
        {
            var expectedName = new Translator("表名", "Table Name");
            var description = new Translator("表描述", "Table Description");
            var table = new ConfigTable("TestTable", new ConfigFileTableModel("TestTable", description), expectedName, description);
            var model = new UiTableModel(table);

            var result = model.Name;

            Assert.Same(expectedName, result);
        }

        [Fact]
        public void Description_WhenAccessed_ReturnsTableDescription()
        {
            var name = new Translator("表名", "Table Name");
            var expectedDescription = new Translator("表描述", "Table Description");
            var table = new ConfigTable("TestTable", new ConfigFileTableModel("TestTable", expectedDescription), name, expectedDescription);
            var model = new UiTableModel(table);

            var result = model.Description;

            Assert.Same(expectedDescription, result);
        }

        [Fact]
        public void Constructor_WithEmptyTable_CreatesEmptyTableList()
        {
            var table = CreateConfigTable();
            
            var model = new UiTableModel(table);

            Assert.NotNull(model.Table);
            Assert.Empty(model.Table);
        }

        [Fact]
        public void Constructor_WithSingleEntry_CreatesTableWithOneUiEntryModel()
        {
            var table = CreateConfigTable();
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("SetLootDropRatio");
            table.Add(mockEntry.Object);

            var model = new UiTableModel(table);

            Assert.Single(model.Table);
        }

        [Fact]
        public void Constructor_WithMultipleEntries_CreatesTableWithAllEntries()
        {
            var table = CreateConfigTable();
            var mockEntry1 = new Mock<IConfigEntry>();
            mockEntry1.Setup(e => e.Key).Returns("SetLootDropRatio");
            var mockEntry2 = new Mock<IConfigEntry>();
            mockEntry2.Setup(e => e.Key).Returns("EnableBetterFishing");
            var mockEntry3 = new Mock<IConfigEntry>();
            mockEntry3.Setup(e => e.Key).Returns("EnableDebugMode");
            table.Add(mockEntry1.Object);
            table.Add(mockEntry2.Object);
            table.Add(mockEntry3.Object);

            var model = new UiTableModel(table);

            Assert.Equal(3, model.Table.Count);
        }

        [Fact]
        public void GetEnumerator_WithEmptyTable_ReturnsEmptyEnumerator()
        {
            var table = CreateConfigTable();
            var model = new UiTableModel(table);

            var enumerator = model.GetEnumerator();
            var hasItems = enumerator.MoveNext();

            Assert.False(hasItems);
        }

        [Fact]
        public void GetEnumerator_WithMultipleEntries_EnumeratesAllItems()
        {
            var table = CreateConfigTable();
            var mockEntry1 = new Mock<IConfigEntry>();
            mockEntry1.Setup(e => e.Key).Returns("SetLootDropRatio");
            var mockEntry2 = new Mock<IConfigEntry>();
            mockEntry2.Setup(e => e.Key).Returns("EnableBetterFishing");
            table.Add(mockEntry1.Object);
            table.Add(mockEntry2.Object);
            var model = new UiTableModel(table);

            var items = new List<UiEntryModel>();
            foreach (var item in model)
            {
                items.Add(item);
            }

            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsEnumerator()
        {
            var table = CreateConfigTable();
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("SetLootDropRatio");
            table.Add(mockEntry.Object);
            var model = new UiTableModel(table);

            var enumerable = (System.Collections.IEnumerable)model;
            var enumerator = enumerable.GetEnumerator();
            var hasItems = enumerator.MoveNext();

            Assert.True(hasItems);
        }
    }
}
