using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Linq;

namespace BetterExperience.Test
{
    public class SheetBindingTests
    {
        [Fact]
        public void CreateSheet_EmptySheet_ReturnsBindingWithNoTables()
        {
            // Arrange
            var sheet = new ConfigSheet();

            // Act
            var result = SheetBinding.CreateSheet(sheet);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sheet);
            Assert.Empty(result.Sheet);
        }

        [Fact]
        public void CreateSheet_SheetWithTablesAndEntries_ReturnsBindingsMatchingSourceStructure()
        {
            // Arrange
            var firstTableName = new Translator("表一", "Table One");
            var firstTableDescription = new Translator("第一个表", "First table");
            var firstTable = new ConfigTable(
                "FirstTable",
                new ConfigFileTable("FirstTable", firstTableDescription),
                firstTableName,
                firstTableDescription);
            var firstEntryName = new Translator("条目一", "Entry One");
            var firstEntryDescription = new Translator("第一个条目", "First entry");
            var firstEntry = CreateEntryMock("EnableHLog", firstEntryName, firstEntryDescription, typeof(bool));
            var secondEntryName = new Translator("条目二", "Entry Two");
            var secondEntryDescription = new Translator("第二个条目", "Second entry");
            var secondEntry = CreateEntryMock("SetLootDropRatio", secondEntryName, secondEntryDescription, typeof(float));
            firstTable.Add(firstEntry.Object);
            firstTable.Add(secondEntry.Object);

            var secondTableName = new Translator("表二", "Table Two");
            var secondTableDescription = new Translator("第二个表", "Second table");
            var secondTable = new ConfigTable(
                "SecondTable",
                new ConfigFileTable("SecondTable", secondTableDescription),
                secondTableName,
                secondTableDescription);

            var sheet = new ConfigSheet();
            sheet.Add("FirstTable", firstTable);
            sheet.Add("SecondTable", secondTable);

            // Act
            var result = SheetBinding.CreateSheet(sheet);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Sheet);
            Assert.Equal(2, result.Sheet.Count);

            var firstTableBinding = result.Sheet[0];
            Assert.Same(firstTableName, firstTableBinding.Name);
            Assert.Same(firstTableDescription, firstTableBinding.Description);
            var firstEntryBindings = firstTableBinding.Table.ToList();
            Assert.Equal(2, firstEntryBindings.Count);
            Assert.Equal("EnableHLog", firstEntryBindings[0].Key);
            Assert.Same(firstEntryName, firstEntryBindings[0].Name);
            Assert.Same(firstEntryDescription, firstEntryBindings[0].Description);
            Assert.Equal(typeof(bool), firstEntryBindings[0].ValueType);
            Assert.Equal("SetLootDropRatio", firstEntryBindings[1].Key);
            Assert.Same(secondEntryName, firstEntryBindings[1].Name);
            Assert.Same(secondEntryDescription, firstEntryBindings[1].Description);
            Assert.Equal(typeof(float), firstEntryBindings[1].ValueType);

            var secondTableBinding = result.Sheet[1];
            Assert.Same(secondTableName, secondTableBinding.Name);
            Assert.Same(secondTableDescription, secondTableBinding.Description);
            Assert.Empty(secondTableBinding.Table);
        }

        private static Mock<IConfigEntry> CreateEntryMock(string key, Translator name, Translator description, Type valueType)
        {
            var entryMock = new Mock<IConfigEntry>();
            entryMock.SetupGet(entry => entry.Key).Returns(key);
            entryMock.SetupGet(entry => entry.Name).Returns(name);
            entryMock.SetupGet(entry => entry.Description).Returns(description);
            entryMock.SetupGet(entry => entry.ValueType).Returns(valueType);
            return entryMock;
        }
    }
}
