using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.HConfigSpace
{
    public class ConfigFileEntryModelTests
    {
        [Fact]
        public void EncodeValueType_WithSByte_ReturnsInt8()
        {
            // Arrange
            var type = typeof(sbyte);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int8", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithShort_ReturnsInt16()
        {
            // Arrange
            var type = typeof(short);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int16", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithByte_ReturnsUInt8()
        {
            // Arrange
            var type = typeof(byte);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("UInt8", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithUShort_ReturnsUInt16()
        {
            // Arrange
            var type = typeof(ushort);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("UInt16", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithUInt_ReturnsUInt32()
        {
            // Arrange
            var type = typeof(uint);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("UInt32", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithULong_ReturnsUInt64()
        {
            // Arrange
            var type = typeof(ulong);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("UInt64", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithDouble_ReturnsDouble()
        {
            // Arrange
            var type = typeof(double);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Double", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithIEnumerable_ReturnsArrayType()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int32[]", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithAdapterThatFails_ReturnsFailure()
        {
            // Arrange
            var type = typeof(TestAdapterWithFailure);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EncodeValueType_WithAdapterThatThrows_ReturnsFailure()
        {
            // Arrange
            var type = typeof(TestAdapterThatThrows);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidType);
        }

        [Fact]
        public void DecodeKeyValuePair_WithInvalidFormat_ReturnsFailure()
        {
            // Arrange
            var content = "InvalidContentWithoutEquals";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyValuePair);
        }

        [Fact]
        public void DecodeKeyValuePair_WithInvalidKeyName_ReturnsFailure()
        {
            // Arrange
            var content = "Invalid Key! = value";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyName);
        }

        [Fact]
        public void DecodeKeyValuePair_WithEmptyValue_ReturnsFailure()
        {
            // Arrange
            var content = "validKey =   ";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidValue);
        }

        [Fact]
        public void EncodeAcceptableValues_WithEnumType_ReturnsEnumValues()
        {
            // Arrange & Act
            var result = ConfigFileEntry.EncodeAcceptableValues<TestEnum>();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Value1", result.Value);
            Assert.Contains("Value2", result.Value);
            Assert.Contains("Value3", result.Value);
        }

        [Fact]
        public void DecodeValue_WithValidString_ReturnsDecodedValue()
        {
            // Arrange
            var value = "42";

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void CreateEntry_WithValidParameters_ReturnsEntry()
        {
            // Arrange
            var key = "TestKey";
            var value = 42;
            var defaultValue = 10;
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(key, result.Value.Key);
            Assert.Equal(name, result.Value.Name);
            Assert.Equal(description, result.Value.Description);
        }

        [Fact]
        public void DecodeKeyValuePair_WithComment_ReturnsFailure()
        {
            // Arrange
            var content = "# key = value";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyValuePair);
        }

        [Fact]
        public void DecodeKeyValuePair_WithValidKeyValuePair_ReturnsSuccess()
        {
            // Arrange
            var content = "key = value";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Item1);
            Assert.Equal("value", result.Value.Item2);
        }

        [Fact]
        public void EncodeValueType_WithString_ReturnsString()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("String", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithInt_ReturnsInt32()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int32", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithLong_ReturnsInt64()
        {
            // Arrange
            var type = typeof(long);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int64", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithFloat_ReturnsFloat()
        {
            // Arrange
            var type = typeof(float);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Float", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithBool_ReturnsBoolean()
        {
            // Arrange
            var type = typeof(bool);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Boolean", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithEnum_ReturnsEnumType()
        {
            // Arrange
            var type = typeof(TestEnum);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Enum TestEnum", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithUnsupportedType_ReturnsFailure()
        {
            // Arrange
            var type = typeof(ConfigFileEntryModelTests);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.UnsupportedType);
        }

        [Fact]
        public void EncodeValueType_WithSuccessfulAdapter_ReturnsAdapterResult()
        {
            // Arrange
            var type = typeof(TestAdapterWithSuccess);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("CustomType", result.Value);
        }

        [Fact]
        public void CreateEntry_WithInvalidKey_ReturnsFailure()
        {
            // Arrange
            var key = "Invalid Key!";
            var value = 42;
            var defaultValue = 10;
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyName);
        }

        [Fact]
        public void DecodeKeyValuePair_WithWhitespaceAroundKeyValue_TrimsProperly()
        {
            // Arrange
            var content = "  key  =  value  ";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Item1);
            Assert.Equal("value", result.Value.Item2);
        }

        [Fact]
        public void DecodeKeyValuePair_WithMultipleEquals_SplitsOnFirst()
        {
            // Arrange
            var content = "key = value = extra";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Item1);
            Assert.Equal("value = extra", result.Value.Item2);
        }

        [Fact]
        public void EncodeValueType_WithArray_ReturnsArrayType()
        {
            // Arrange
            var type = typeof(int[]);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int32[]", result.Value);
        }

        [Fact]
        public void EncodeAcceptableValues_WithNonEnumType_ReturnsFailure()
        {
            // Arrange & Act
            var result = ConfigFileEntry.EncodeAcceptableValues<int>();

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidType);
        }

        [Fact]
        public void DecodeValue_WithInvalidValue_ReturnsFailure()
        {
            // Arrange
            var value = "not_a_number";

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EncodeValueType_WithNestedArray_ReturnsNestedArrayType()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("String[]", result.Value);
        }

        [Fact]
        public void CreateEntry_WithEmptyKey_ReturnsFailure()
        {
            // Arrange
            var key = "";
            var value = 42;
            var defaultValue = 10;
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyName);
        }

        [Fact]
        public void DecodeValue_WithNullValue_ReturnsFailure()
        {
            // Arrange
            string value = null;

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void DecodeValue_WithWhitespaceValue_ReturnsFailure()
        {
            // Arrange
            var value = "   ";

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void DecodeKeyValuePair_WithEmptyString_ReturnsFailure()
        {
            // Arrange
            var content = "";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyValuePair);
        }

        [Fact]
        public void DecodeKeyValuePair_WithOnlyEquals_ReturnsFailure()
        {
            // Arrange
            var content = "=";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EncodeAcceptableValues_WithEnumInArray_ReturnsEnumValues()
        {
            // Arrange & Act
            var result = ConfigFileEntry.EncodeAcceptableValues<List<TestEnum>>();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Value1", result.Value);
            Assert.Contains("Value2", result.Value);
            Assert.Contains("Value3", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithEnumArray_ReturnsEnumArrayType()
        {
            // Arrange
            var type = typeof(TestEnum[]);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Enum TestEnum[]", result.Value);
        }

        [Fact]
        public void DecodeValue_WithValidBool_ReturnsDecodedValue()
        {
            // Arrange
            var value = "true";

            // Act
            var result = ConfigFileEntry.DecodeValue<bool>(value);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Value);
        }

        [Fact]
        public void DecodeValue_WithValidString_ReturnsStringValue()
        {
            // Arrange
            var value = "\"test string\"";

            // Act
            var result = ConfigFileEntry.DecodeValue<string>(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("test string", result.Value);
        }

        [Fact]
        public void CreateEntry_WithNullName_ReturnsEntry()
        {
            // Arrange
            var key = "TestKey";
            var value = 42;
            var defaultValue = 10;
            Translator name = null;
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(key, result.Value.Key);
            Assert.Null(result.Value.Name);
        }

        [Fact]
        public void CreateEntry_WithNullDescription_ReturnsEntry()
        {
            // Arrange
            var key = "TestKey";
            var value = 42;
            var defaultValue = 10;
            var name = new Translator("名称", "Name");
            Translator description = null;

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(key, result.Value.Key);
            Assert.Null(result.Value.Description);
        }

        [Fact]
        public void DecodeKeyValuePair_WithKeyContainingNumbers_ReturnsSuccess()
        {
            // Arrange
            var content = "key123 = value";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key123", result.Value.Item1);
            Assert.Equal("value", result.Value.Item2);
        }

        [Fact]
        public void DecodeKeyValuePair_WithKeyContainingUnderscore_ReturnsSuccess()
        {
            // Arrange
            var content = "key_test = value";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key_test", result.Value.Item1);
            Assert.Equal("value", result.Value.Item2);
        }

        [Fact]
        public void EncodeValueType_GenericMethod_WithInt_ReturnsInt32()
        {
            // Arrange & Act
            var result = ConfigFileEntry.EncodeValueType<int>();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int32", result.Value);
        }

        [Fact]
        public void EncodeValueType_GenericMethod_WithString_ReturnsString()
        {
            // Arrange & Act
            var result = ConfigFileEntry.EncodeValueType<string>();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("String", result.Value);
        }

        [Fact]
        public void DecodeValue_WithZero_ReturnsZero()
        {
            // Arrange
            var value = "0";

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void DecodeValue_WithNegativeNumber_ReturnsNegative()
        {
            // Arrange
            var value = "-42";

            // Act
            var result = ConfigFileEntry.DecodeValue<int>(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(-42, result.Value);
        }

        [Fact]
        public void CreateEntry_WithBoolType_ReturnsEntry()
        {
            // Arrange
            var key = "TestBool";
            var value = true;
            var defaultValue = false;
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(key, result.Value.Key);
            Assert.Equal("Boolean", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntry_WithStringType_ReturnsEntry()
        {
            // Arrange
            var key = "TestString";
            var value = "test";
            var defaultValue = "default";
            var name = new Translator("名称", "Name");
            var description = new Translator("描述", "Description");

            // Act
            var result = ConfigFileEntry.CreateEntry(key, value, defaultValue, name, description);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(key, result.Value.Key);
            Assert.Equal("String", result.Value.ValueType);
        }

        [Fact]
        public void EncodeValueType_WithIntArray_ReturnsIntArrayType()
        {
            // Arrange
            var type = typeof(int[]);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Int32[]", result.Value);
        }

        [Fact]
        public void EncodeValueType_WithStringList_ReturnsStringArrayType()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = ConfigFileEntry.EncodeValueType(type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("String[]", result.Value);
        }

        [Fact]
        public void DecodeKeyValuePair_WithSpacesInValue_PreservesSpaces()
        {
            // Arrange
            var content = "key = value with spaces";

            // Act
            var result = ConfigFileEntry.DecodeKeyValuePair(content);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Item1);
            Assert.Equal("value with spaces", result.Value.Item2);
        }

        [Fact]
        public void DecodeEntry_WithValidKeyValuePair_ReturnsEntry()
        {
            // Arrange
            var content = new[] { "key = value" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Key);
            Assert.Equal("value", result.Value.Value);
        }

        [Fact]
        public void DecodeEntry_WithInvalidKeyInKeyValuePair_ReturnsFailure()
        {
            // Arrange
            var content = new[] { "Invalid Key! = value" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyName);
        }

        [Fact]
        public void DecodeEntry_WithLineNotKeyValuePair_ReturnsFailure()
        {
            // Arrange
            var content = new[] { "SomeInvalidLineWithoutEquals" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyValuePair);
        }

        [Fact]
        public void DecodeEntry_WithEmptyContent_ReturnsEntryNotFound()
        {
            // Arrange
            var content = new string[0];
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.EntryNotFound);
        }

        [Fact]
        public void DecodeEntry_WithOnlyComments_ReturnsEntryNotFound()
        {
            // Arrange
            var content = new[] { "# Comment 1", "## Comment 2" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.EntryNotFound);
        }

        [Fact]
        public void DecodeEntry_WithOnlyWhitespace_ReturnsEntryNotFound()
        {
            // Arrange
            var content = new[] { "   ", "\t", "" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.EntryNotFound);
        }

        [Fact]
        public void DecodeEntry_WithCommentsBeforeValidEntry_SkipsCommentsAndReturnsEntry()
        {
            // Arrange
            var content = new[] { "# Comment", "", "key = value" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Key);
            Assert.Equal("value", result.Value.Value);
        }

        [Fact]
        public void DecodeEntry_WithStartingIndexBeyondContent_ReturnsEntryNotFound()
        {
            // Arrange
            var content = new[] { "key = value" };
            var index = 5;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.EntryNotFound);
        }

        [Fact]
        public void DecodeEntry_WithEmptyValueInKeyValuePair_ReturnsFailure()
        {
            // Arrange
            var content = new[] { "key = " };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidValue);
        }

        [Fact]
        public void DecodeEntry_UpdatesIndexAfterValidEntry()
        {
            // Arrange
            var content = new[] { "key = value", "next line" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, index);
        }

        [Fact]
        public void DecodeEntry_UpdatesIndexAfterInvalidLine()
        {
            // Arrange
            var content = new[] { "InvalidLine", "next line" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(1, index);
        }

        [Fact]
        public void DecodeEntry_WithMultipleValidEntries_ReturnsFirstEntry()
        {
            // Arrange
            var content = new[] { "key1 = value1", "key2 = value2" };
            var index = 0;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key1", result.Value.Key);
            Assert.Equal("value1", result.Value.Value);
        }

        [Fact]
        public void DecodeEntry_WithNonZeroStartIndex_ReturnsCorrectEntry()
        {
            // Arrange
            var content = new[] { "key1 = value1", "key2 = value2" };
            var index = 1;

            // Act
            var result = ConfigFileEntry.DecodeEntry(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("key2", result.Value.Key);
            Assert.Equal("value2", result.Value.Value);
        }

        private class TestAdapterWithSuccess : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return "test";
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return new object();
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return "CustomType";
            }
        }

        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        private class TestAdapterWithFailure : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return "test";
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return new object();
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidType, "Test failure"));
            }
        }

        private class TestAdapterThatThrows : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                throw new NotImplementedException();
            }

            public ConfigFileResult<object> Decode(string content)
            {
                throw new NotImplementedException();
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                throw new InvalidOperationException("Test exception");
            }
        }
    }
}
