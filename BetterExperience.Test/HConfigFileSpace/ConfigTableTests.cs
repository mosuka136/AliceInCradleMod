using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BetterExperience.Test.HConfigFileSpace
{
    public class ConfigTableTests
    {
        [Fact]
        public void Constructor_InvalidTableName_ThrowsArgumentException()
        {
            // Arrange
            var invalidKey = "invalid-key";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var name = new Translator("Name");
            var description = new Translator("Desc");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                new ConfigTable(invalidKey, table, name, description));
            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public void Constructor_ValidTableName_CreatesInstance()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var name = new Translator("Name");
            var description = new Translator("Desc");

            // Act
            var result = new ConfigTable(validKey, table, name, description);

            // Assert
            Assert.Equal(validKey, result.Key);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.NotNull(result.Table);
            Assert.Empty(result.Table);
            Assert.Same(table, result.FileTable);
            Assert.Same(name, table.Name);
            Assert.Same(description, table.Description);
        }

        [Fact]
        public void Constructor_NullName_SetsEmptyTranslator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            Translator name = null;
            var description = new Translator("Desc");

            // Act
            var result = new ConfigTable(validKey, table, name, description);

            // Assert
            Assert.NotNull(result.Name);
        }

        [Fact]
        public void Constructor_NullDescription_SetsEmptyTranslator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var name = new Translator("Name");
            Translator description = null;

            // Act
            var result = new ConfigTable(validKey, table, name, description);

            // Assert
            Assert.NotNull(result.Description);
        }

        [Fact]
        public void Add_NullEntry_DoesNotAddToTable()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));

            // Act
            configTable.Add(null);

            // Assert
            Assert.Empty(configTable.Table);
        }

        [Fact]
        public void Add_ValidEntry_AddsToTable()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));
            var mockEntry = new Mock<IConfigEntry>();

            // Act
            configTable.Add(mockEntry.Object);

            // Assert
            Assert.Single(configTable.Table);
            Assert.Same(mockEntry.Object, configTable.Table[0]);
        }

        [Fact]
        public void Add_MultipleEntries_AddsAllToTable()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));
            var mockEntry1 = new Mock<IConfigEntry>();
            var mockEntry2 = new Mock<IConfigEntry>();

            // Act
            configTable.Add(mockEntry1.Object);
            configTable.Add(mockEntry2.Object);

            // Assert
            Assert.Equal(2, configTable.Table.Count);
            Assert.Same(mockEntry1.Object, configTable.Table[0]);
            Assert.Same(mockEntry2.Object, configTable.Table[1]);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsTableEnumerator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));
            var mockEntry1 = new Mock<IConfigEntry>();
            var mockEntry2 = new Mock<IConfigEntry>();
            configTable.Add(mockEntry1.Object);
            configTable.Add(mockEntry2.Object);

            // Act
            var result = new List<IConfigEntry>();
            foreach (var entry in configTable)
            {
                result.Add(entry);
            }

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Same(mockEntry1.Object, result[0]);
            Assert.Same(mockEntry2.Object, result[1]);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsTableEnumerator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));
            var mockEntry1 = new Mock<IConfigEntry>();
            var mockEntry2 = new Mock<IConfigEntry>();
            configTable.Add(mockEntry1.Object);
            configTable.Add(mockEntry2.Object);

            // Act
            var result = new List<object>();
            var enumerable = (IEnumerable)configTable;
            foreach (var entry in enumerable)
            {
                result.Add(entry);
            }

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Same(mockEntry1.Object, result[0]);
            Assert.Same(mockEntry2.Object, result[1]);
        }

        [Fact]
        public void GetEnumerator_EmptyTable_ReturnsEmptyEnumerator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));

            // Act
            var result = configTable.ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_EmptyTable_ReturnsEmptyEnumerator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));

            // Act
            var result = new List<object>();
            var enumerable = (IEnumerable)configTable;
            foreach (var entry in enumerable)
            {
                result.Add(entry);
            }

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_DirectCall_ReturnsTableEnumerator()
        {
            // Arrange
            var validKey = "Valid_Table";
            var table = new ConfigFileTableModel("table", new Translator(string.Empty));
            var configTable = new ConfigTable(validKey, table, new Translator("Name"), new Translator("Desc"));
            var mockEntry = new Mock<IConfigEntry>();
            configTable.Add(mockEntry.Object);

            // Act
            var enumerable = (IEnumerable)configTable;
            var enumerator = enumerable.GetEnumerator();
            var hasFirst = enumerator.MoveNext();
            var first = enumerator.Current;
            var hasSecond = enumerator.MoveNext();

            // Assert
            Assert.True(hasFirst);
            Assert.Same(mockEntry.Object, first);
            Assert.False(hasSecond);
        }
    }
}
