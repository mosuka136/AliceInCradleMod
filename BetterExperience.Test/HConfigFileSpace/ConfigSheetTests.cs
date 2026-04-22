using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test
{
    public class ConfigSheetTests
    {
        // -----------------------------------------------------------------------
        // Count Property - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Count_WithEmptySheet_ReturnsZero()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var count = configSheet.Count;

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void Count_WithAddedItems_ReturnsCorrectCount()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var count = configSheet.Count;

            // Assert
            Assert.Equal(2, count);
        }

        // -----------------------------------------------------------------------
        // Contains Method - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Contains_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var result = configSheet.Contains("table1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Contains_WithNonExistingKey_ReturnsFalse()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var result = configSheet.Contains("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Contains_WithEmptySheet_ReturnsFalse()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var result = configSheet.Contains("anyKey");

            // Assert
            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // Keys Property - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Keys_WithEmptySheet_ReturnsEmptyEnumerable()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var keys = configSheet.Keys.ToList();

            // Assert
            Assert.Empty(keys);
        }

        [Fact]
        public void Keys_WithAddedItems_ReturnsAllKeys()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var keys = configSheet.Keys.ToList();

            // Assert
            Assert.Equal(2, keys.Count);
            Assert.Contains("table1", keys);
            Assert.Contains("table2", keys);
        }

        [Fact]
        public void Keys_IteratesCorrectly()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var keysList = new System.Collections.Generic.List<string>();
            foreach (var key in configSheet.Keys)
            {
                keysList.Add(key);
            }

            // Assert
            Assert.Single(keysList);
            Assert.Equal("table1", keysList[0]);
        }

        // -----------------------------------------------------------------------
        // Values Property - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void Values_WithEmptySheet_ReturnsEmptyEnumerable()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var values = configSheet.Values.ToList();

            // Assert
            Assert.Empty(values);
        }

        [Fact]
        public void Values_WithAddedItems_ReturnsAllValues()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var values = configSheet.Values.ToList();

            // Assert
            Assert.Equal(2, values.Count);
            Assert.Contains(mockTable1, values);
            Assert.Contains(mockTable2, values);
        }

        [Fact]
        public void Values_IteratesCorrectly()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var valuesList = new System.Collections.Generic.List<ConfigTable>();
            foreach (var value in configSheet.Values)
            {
                valuesList.Add(value);
            }

            // Assert
            Assert.Single(valuesList);
            Assert.Same(mockTable, valuesList[0]);
        }

        // -----------------------------------------------------------------------
        // GetEnumerator Method - Basic tests
        // -----------------------------------------------------------------------

        [Fact]
        public void GetEnumerator_WithEmptySheet_ReturnsEmptyEnumerator()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var enumerator = configSheet.GetEnumerator();
            var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, ConfigTable>>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            // Assert
            Assert.Empty(items);
        }

        [Fact]
        public void GetEnumerator_WithAddedItems_ReturnsAllKeyValuePairs()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, ConfigTable>>();
            foreach (var kvp in configSheet)
            {
                items.Add(kvp);
            }

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Equal("table1", items[0].Key);
            Assert.Same(mockTable1, items[0].Value);
            Assert.Equal("table2", items[1].Key);
            Assert.Same(mockTable2, items[1].Value);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsEnumerator()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var enumerator = ((System.Collections.IEnumerable)configSheet).GetEnumerator();
            var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, ConfigTable>>();
            while (enumerator.MoveNext())
            {
                items.Add((System.Collections.Generic.KeyValuePair<string, ConfigTable>)enumerator.Current);
            }

            // Assert
            Assert.Single(items);
            Assert.Equal("table1", items[0].Key);
            Assert.Same(mockTable, items[0].Value);
        }

        // -----------------------------------------------------------------------
        // Helper methods
        // -----------------------------------------------------------------------

        private ConfigTable CreateMockConfigTable(string key)
        {
            var translator = new Translator("中文", "English");
            var tableModel = new ConfigFileTableModel(key, translator);
            return new ConfigTable(key, tableModel, translator, translator);
        }
    }
}
