using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using System.Collections;

namespace BetterExperience.Test.HConfigSpace
{
    public class ConfigEntryTests
    {
        [Fact]
        public void Name_WhenEntryHasName_ReturnsEntryName()
        {
            // Arrange
            var expectedName = new Translator(chinese: "名称", english: "Name");
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "\"test\"",
                Name = expectedName
            };
            var entry = new ConfigEntry<string>("General", model, "default");

            // Act
            var actualName = entry.Name;

            // Assert
            Assert.Same(expectedName, actualName);
        }

        [Fact]
        public void Description_WhenEntryHasDescription_ReturnsEntryDescription()
        {
            // Arrange
            var expectedDescription = new Translator(chinese: "描述", english: "Description");
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "\"test\"",
                Description = expectedDescription
            };
            var entry = new ConfigEntry<string>("General", model, "default");

            // Act
            var actualDescription = entry.Description;

            // Assert
            Assert.Same(expectedDescription, actualDescription);
        }

        [Fact]
        public void ValueType_WhenCreated_ReturnsTypeOfGenericParameter()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);

            // Act
            var valueType = entry.ValueType;

            // Assert
            Assert.Equal(typeof(int), valueType);
        }

        [Fact]
        public void ValueType_WhenStringType_ReturnsStringType()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "\"test\""
            };
            var entry = new ConfigEntry<string>("General", model, "default");

            // Act
            var valueType = entry.ValueType;

            // Assert
            Assert.Equal(typeof(string), valueType);
        }

        [Fact]
        public void RebindEntry_WhenEntryIsNull_ReturnsWithoutException()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);

            // Act
            entry.RebindEntry(null);

            // Assert - no exception thrown
        }

        [Fact]
        public void RebindEntry_WhenValidEntry_UpdatesEntryAndValue()
        {
            // Arrange
            var initialModel = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", initialModel, 0);
            
            var newModel = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "99"
            };

            // Act
            entry.RebindEntry(newModel);

            // Assert
            Assert.Same(newModel, entry.Entry);
            Assert.Equal(99, entry.Value);
        }

        [Fact]
        public void RebindEntry_WhenDecodeValueFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var initialModel = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", initialModel, 0);
            
            var newModel = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "invalid_integer"
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => entry.RebindEntry(newModel));
        }

        [Fact]
        public void Value_WhenSetToNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "\"test\""
            };
            var entry = new ConfigEntry<string>("General", model, "default");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => entry.Value = null);
            Assert.Contains("Failed to encode value for key", exception.Message);
            Assert.Contains("TestKey", exception.Message);
        }

        [Fact]
        public void BoxedValue_WhenGetting_ReturnsValue()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);
            IConfigEntry iEntry = entry;

            // Act
            var boxedValue = iEntry.BoxedValue;

            // Assert
            Assert.Equal(42, boxedValue);
        }

        [Fact]
        public void BoxedValue_WhenSetting_UpdatesValue()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);
            IConfigEntry iEntry = entry;

            // Act
            iEntry.BoxedValue = 100;

            // Assert
            Assert.Equal(100, entry.Value);
            Assert.Equal(100, iEntry.BoxedValue);
        }

        [Fact]
        public void BoxedDefaultValue_WhenGetting_ReturnsDefaultValue()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "\"test\""
            };
            var entry = new ConfigEntry<string>("General", model, "defaultValue");
            IConfigEntry iEntry = entry;

            // Act
            var boxedDefaultValue = iEntry.BoxedDefaultValue;

            // Assert
            Assert.Equal("defaultValue", boxedDefaultValue);
        }

        [Fact]
        public void OnValueChangedBase_WhenAdding_SubscribesToOnValueChanged()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);
            IConfigEntry iEntry = entry;
            EventHandler handler = (s, e) => { };

            // Act
            iEntry.OnValueChangedBase += handler;
            entry.Value = 100;

            // Assert - no exception thrown, event chain is established
        }

        [Fact]
        public void OnValueChangedBase_WhenRemoving_UnsubscribesFromOnValueChanged()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var entry = new ConfigEntry<int>("General", model, 0);
            IConfigEntry iEntry = entry;
            EventHandler handler = (s, e) => { };

            // Act
            iEntry.OnValueChangedBase += handler;
            iEntry.OnValueChangedBase -= handler;
            entry.Value = 100;

            // Assert - no exception thrown
        }

        [Fact]
        public void Constructor_WhenParameterless_CreatesInstance()
        {
            // Arrange & Act
            var entry = new ConfigEntry<int>();

            // Assert
            Assert.NotNull(entry);
        }

        [Fact]
        public void Constructor_WithThreeParameters_InitializesProperties()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42",
                Name = new Translator(chinese: "名称", english: "Name"),
                Description = new Translator(chinese: "描述", english: "Description")
            };

            // Act
            var entry = new ConfigEntry<int>("General", model, 10);

            // Assert
            Assert.Equal("General", entry.TableName);
            Assert.Equal(10, entry.DefaultValue);
            Assert.Same(model, entry.Entry);
            Assert.Equal(42, entry.Value);
        }

        [Fact]
        public void Constructor_WhenEntryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigEntry<int>("General", null, 0, name, description));
        }

        [Fact]
        public void Constructor_WhenInvalidTableName_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "42"
            };
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new ConfigEntry<int>("Invalid-Table-Name", model, 0, name, description));
            Assert.Contains("Invalid table name", exception.Message);
        }

        [Fact]
        public void Constructor_WhenUnsupportedValueType_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "dummy"
            };
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new ConfigEntry<UnsupportedType>("General", model, new UnsupportedType(), name, description));
            Assert.Contains("Failed to encode value type", exception.Message);
        }

        [Fact]
        public void Constructor_WhenDefaultValueEncodingFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "null"
            };
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new ConfigEntry<string>("General", model, null, name, description));
            Assert.Contains("Failed to encode default value", exception.Message);
        }

        [Fact]
        public void EqualBoxed_WhenBothNull_ReturnsTrue()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(null, null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenFirstNull_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(null, 42);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenSecondNull_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(42, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenDifferentTypes_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(42, "42");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenPrimitivesEqual_ReturnsTrue()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(42, 42);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenPrimitivesNotEqual_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(42, 43);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenStringsEqual_ReturnsTrue()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed("test", "test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenStringsNotEqual_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed("test", "other");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenArraysEqual_ReturnsTrue()
        {
            // Arrange
            var array1 = new[] { 1, 2, 3 };
            var array2 = new[] { 1, 2, 3 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(array1, array2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenArraysNotEqual_ReturnsFalse()
        {
            // Arrange
            var array1 = new[] { 1, 2, 3 };
            var array2 = new[] { 1, 2, 4 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(array1, array2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenArraysDifferentLength_ReturnsFalse()
        {
            // Arrange
            var array1 = new[] { 1, 2, 3 };
            var array2 = new[] { 1, 2 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(array1, array2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenEnumerablesEqual_ReturnsTrue()
        {
            // Arrange
            var list1 = new System.Collections.Generic.List<int> { 1, 2, 3 };
            var list2 = new System.Collections.Generic.List<int> { 1, 2, 3 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(list1, list2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenEnumerablesNotEqual_ReturnsFalse()
        {
            // Arrange
            var list1 = new System.Collections.Generic.List<int> { 1, 2, 3 };
            var list2 = new System.Collections.Generic.List<int> { 1, 2, 4 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(list1, list2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenEnumerablesDifferentLength_ReturnsFalse()
        {
            // Arrange
            var list1 = new System.Collections.Generic.List<int> { 1, 2, 3 };
            var list2 = new System.Collections.Generic.List<int> { 1, 2 };

            // Act
            var result = ConfigEntry<int>.EqualBoxed(list1, list2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenNonComparableType_ReturnsFalse()
        {
            // Arrange
            var obj1 = new object();
            var obj2 = new object();

            // Act
            var result = ConfigEntry<int>.EqualBoxed(obj1, obj2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EntryValueChangedEventArgs_Constructor_SetsValue()
        {
            // Act
            var args = new EntryValueChangedEventArgs<int>(42);

            // Assert
            Assert.Equal(42, args.Value);
        }

        [Fact]
        public void Constructor_WhenEnumType_SetsAcceptableValues()
        {
            // Arrange
            var model = new ConfigFileEntry
            {
                Key = "TestKey",
                Value = "Value1"
            };
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act
            var entry = new ConfigEntry<TestEnum>("General", model, TestEnum.Value1, name, description);

            // Assert
            Assert.NotNull(entry.Entry.AcceptableValues);
            Assert.Contains("Value1", entry.Entry.AcceptableValues);
        }

        [Fact]
        public void Constructor_WhenInvalidKeyName_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new ConfigFileEntry();
            model.GetType().GetField("_key", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(model, "Invalid.Key");
            model.Value = "42";
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new ConfigEntry<int>("General", model, 0, name, description));
            Assert.Contains("Invalid key name", exception.Message);
        }

        [Fact]
        public void EqualBoxed_WhenEnumsEqual_ReturnsTrue()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(TestEnum.Value1, TestEnum.Value1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EqualBoxed_WhenEnumsNotEqual_ReturnsFalse()
        {
            // Act
            var result = ConfigEntry<int>.EqualBoxed(TestEnum.Value1, TestEnum.Value2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualBoxed_WhenEnumerableWithNullEnumerator_ReturnsFalse()
        {
            // Arrange
            var enumerable = new EnumerableWithNullEnumerator();

            // Act
            var result = ConfigEntry<int>.EqualBoxed(enumerable, enumerable);

            // Assert
            Assert.False(result);
        }
    }

    public class UnsupportedType
    {
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    public class EnumerableWithNullEnumerator : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }
}
