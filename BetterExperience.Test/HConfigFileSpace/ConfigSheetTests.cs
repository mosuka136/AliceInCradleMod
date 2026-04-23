using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.HConfigFileSpace
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

        [Fact]
        public void Keys_MultipleEnumerations_ReturnsSameKeys()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var keys1 = configSheet.Keys.ToList();
            var keys2 = configSheet.Keys.ToList();

            // Assert
            Assert.Equal(keys1, keys2);
        }

        [Fact]
        public void Keys_PreservesInsertionOrder()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("first", mockTable1);
            configSheet.Add("second", mockTable2);
            configSheet.Add("third", mockTable3);

            // Act
            var keys = configSheet.Keys.ToList();

            // Assert
            Assert.Equal("first", keys[0]);
            Assert.Equal("second", keys[1]);
            Assert.Equal("third", keys[2]);
        }

        [Fact]
        public void Keys_PartialIteration_DoesNotThrow()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);
            configSheet.Add("table3", mockTable3);

            // Act
            var enumerator = configSheet.Keys.GetEnumerator();
            var firstKey = enumerator.MoveNext() ? enumerator.Current : null;

            // Assert
            Assert.Equal("table1", firstKey);
        }

        [Fact]
        public void Keys_WithSingleItem_ReturnsOneKey()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("singleKey", mockTable);

            // Act
            var keys = configSheet.Keys.ToList();

            // Assert
            Assert.Single(keys);
            Assert.Equal("singleKey", keys[0]);
        }

        [Fact]
        public void Keys_ConcurrentEnumerators_WorkIndependently()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var enumerator1 = configSheet.Keys.GetEnumerator();
            var enumerator2 = configSheet.Keys.GetEnumerator();
            enumerator1.MoveNext();
            enumerator2.MoveNext();
            var key1 = enumerator1.Current;
            var key2 = enumerator2.Current;

            // Assert
            Assert.Equal(key1, key2);
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

        [Fact]
        public void Values_MultipleEnumerations_ReturnsSameValues()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var values1 = configSheet.Values.ToList();
            var values2 = configSheet.Values.ToList();

            // Assert
            Assert.Equal(values1, values2);
        }

        [Fact]
        public void Values_PreservesInsertionOrder()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("first", mockTable1);
            configSheet.Add("second", mockTable2);
            configSheet.Add("third", mockTable3);

            // Act
            var values = configSheet.Values.ToList();

            // Assert
            Assert.Same(mockTable1, values[0]);
            Assert.Same(mockTable2, values[1]);
            Assert.Same(mockTable3, values[2]);
        }

        [Fact]
        public void Values_PartialIteration_DoesNotThrow()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);
            configSheet.Add("table3", mockTable3);

            // Act
            var enumerator = configSheet.Values.GetEnumerator();
            var firstValue = enumerator.MoveNext() ? enumerator.Current : null;

            // Assert
            Assert.Same(mockTable1, firstValue);
        }

        [Fact]
        public void Values_WithSingleItem_ReturnsOneValue()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("table1", mockTable);

            // Act
            var values = configSheet.Values.ToList();

            // Assert
            Assert.Single(values);
            Assert.Same(mockTable, values[0]);
        }

        [Fact]
        public void Values_ConcurrentEnumerators_WorkIndependently()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var enumerator1 = configSheet.Values.GetEnumerator();
            var enumerator2 = configSheet.Values.GetEnumerator();
            enumerator1.MoveNext();
            enumerator2.MoveNext();
            var value1 = enumerator1.Current;
            var value2 = enumerator2.Current;

            // Assert
            Assert.Same(value1, value2);
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

        [Fact]
        public void GetEnumerator_MultipleEnumerations_WorksCorrectly()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var items1 = configSheet.ToList();
            var items2 = configSheet.ToList();

            // Assert
            Assert.Equal(2, items1.Count);
            Assert.Equal(2, items2.Count);
            Assert.Equal(items1[0].Key, items2[0].Key);
            Assert.Equal(items1[1].Key, items2[1].Key);
        }

        [Fact]
        public void GetEnumerator_PreservesInsertionOrder()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("first", mockTable1);
            configSheet.Add("second", mockTable2);
            configSheet.Add("third", mockTable3);

            // Act
            var items = configSheet.ToList();

            // Assert
            Assert.Equal("first", items[0].Key);
            Assert.Same(mockTable1, items[0].Value);
            Assert.Equal("second", items[1].Key);
            Assert.Same(mockTable2, items[1].Value);
            Assert.Equal("third", items[2].Key);
            Assert.Same(mockTable3, items[2].Value);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_WithEmptySheet_WorksCorrectly()
        {
            // Arrange
            var configSheet = new ConfigSheet();

            // Act
            var enumerator = ((System.Collections.IEnumerable)configSheet).GetEnumerator();
            var hasItems = enumerator.MoveNext();

            // Assert
            Assert.False(hasItems);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_WithMultipleItems_WorksCorrectly()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var enumerator = ((System.Collections.IEnumerable)configSheet).GetEnumerator();
            var items = new System.Collections.Generic.List<object>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            // Assert
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void GetEnumerator_ConcurrentEnumerators_WorkIndependently()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);

            // Act
            var enumerator1 = configSheet.GetEnumerator();
            var enumerator2 = configSheet.GetEnumerator();
            enumerator1.MoveNext();
            enumerator2.MoveNext();
            var item1 = enumerator1.Current;
            var item2 = enumerator2.Current;

            // Assert
            Assert.Equal(item1.Key, item2.Key);
            Assert.Same(item1.Value, item2.Value);
        }

        [Fact]
        public void GetEnumerator_PartialIteration_DoesNotThrow()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("table1", mockTable1);
            configSheet.Add("table2", mockTable2);
            configSheet.Add("table3", mockTable3);

            // Act
            var enumerator = configSheet.GetEnumerator();
            var hasFirst = enumerator.MoveNext();
            var first = enumerator.Current;

            // Assert
            Assert.True(hasFirst);
            Assert.Equal("table1", first.Key);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_PreservesInsertionOrder()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable1 = CreateMockConfigTable("key1");
            var mockTable2 = CreateMockConfigTable("key2");
            var mockTable3 = CreateMockConfigTable("key3");
            configSheet.Add("first", mockTable1);
            configSheet.Add("second", mockTable2);
            configSheet.Add("third", mockTable3);

            // Act
            var enumerator = ((System.Collections.IEnumerable)configSheet).GetEnumerator();
            var items = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, ConfigTable>>();
            while (enumerator.MoveNext())
            {
                items.Add((System.Collections.Generic.KeyValuePair<string, ConfigTable>)enumerator.Current);
            }

            // Assert
            Assert.Equal("first", items[0].Key);
            Assert.Equal("second", items[1].Key);
            Assert.Equal("third", items[2].Key);
        }

        [Fact]
        public void GetEnumerator_WithSingleItem_ReturnsOneItem()
        {
            // Arrange
            var configSheet = new ConfigSheet();
            var mockTable = CreateMockConfigTable("key1");
            configSheet.Add("singleKey", mockTable);

            // Act
            var items = configSheet.ToList();

            // Assert
            Assert.Single(items);
            Assert.Equal("singleKey", items[0].Key);
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
