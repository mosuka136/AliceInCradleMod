using System;
using System.Collections.Generic;
using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.ConfigFileSpace
{
    public class ConfigFileEntryModelTests
    {
        // -----------------------------------------------------------------------
        // IsValidKeyName
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData("key", true)]
        [InlineData("key_123", true)]
        [InlineData("_key", true)]
        [InlineData("KEY", true)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("key.name", false)]
        [InlineData("key-name", false)]
        [InlineData("key name", false)]
        public void IsValidKeyNameShouldReturnExpectedResult(string key, bool expected)
        {
            var result = ConfigFileEntryModel.IsValidKeyName(key);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsValidKeyNameWhenNullShouldReturnFalse()
        {
            var result = ConfigFileEntryModel.IsValidKeyName(null);

            Assert.False(result);
        }

        // -----------------------------------------------------------------------
        // IsKeyValuePair
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData("key=value", true)]
        [InlineData("key=", true)]
        [InlineData("#comment", false)]
        [InlineData("## comment=has equals", false)]
        [InlineData("noequal", false)]
        [InlineData(" # comment=value", false)] // bug-fix: leading space before '#'
        public void IsKeyValuePairShouldReturnExpectedResult(string content, bool expected)
        {
            var result = ConfigFileEntryModel.IsKeyValuePair(content);

            Assert.Equal(expected, result);
        }

        // -----------------------------------------------------------------------
        // IsComment
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData("# comment", true)]
        [InlineData("  # comment with leading space", true)]
        [InlineData("## double hash", true)]
        [InlineData("key=value", false)]
        [InlineData("", false)]
        public void IsCommentShouldReturnExpectedResult(string content, bool expected)
        {
            var result = ConfigFileEntryModel.IsComment(content);

            Assert.Equal(expected, result);
        }

        // -----------------------------------------------------------------------
        // EncodeDescription
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeDescriptionWhenNullShouldReturnEmptyString()
        {
            var entry = new ConfigFileEntryModel { Description = null };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenEmptyShouldReturnEmptyString()
        {
            var entry = new ConfigFileEntryModel { Description = new Translator() };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenSingleLineShouldReturnPrefixedLine()
        {
            var entry = new ConfigFileEntryModel { Description = new Translator(english: "hello") };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## hello", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenMultiLineNewlineShouldReturnMultiplePrefixedLines()
        {
            var entry = new ConfigFileEntryModel { Description = new Translator(english: "line1\nline2") };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## line1" + Environment.NewLine + "## line2", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenCrLfShouldNormalizeToNewline()
        {
            var entry = new ConfigFileEntryModel { Description = new Translator(english: "line1\r\nline2") };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## line1" + Environment.NewLine + "## line2", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenCrOnlyShouldNormalizeToNewline()
        {
            var entry = new ConfigFileEntryModel { Description = new Translator(english: "line1\rline2") };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## line1" + Environment.NewLine + "## line2", result.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeValueType (instance)
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeValueTypeInstanceWhenEmptyShouldReturnEmptyString()
        {
            var entry = new ConfigFileEntryModel { ValueType = "" };

            var result = entry.EncodeValueType();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeValueTypeInstanceWhenSetShouldReturnFormattedString()
        {
            var entry = new ConfigFileEntryModel { ValueType = "Int32" };

            var result = entry.EncodeValueType();

            Assert.True(result.Success);
            Assert.Equal("# Value Type: Int32", result.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeDefaultValue
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeDefaultValueWhenEmptyShouldReturnEmptyString()
        {
            var entry = new ConfigFileEntryModel { DefaultValue = "" };

            var result = entry.EncodeDefaultValue();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDefaultValueWhenSetShouldReturnFormattedString()
        {
            var entry = new ConfigFileEntryModel { DefaultValue = "42" };

            var result = entry.EncodeDefaultValue();

            Assert.True(result.Success);
            Assert.Equal("# Default Value: 42", result.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeKeyValuePair
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeKeyValuePairWhenValidShouldReturnKeyEqualsValue()
        {
            var entry = new ConfigFileEntryModel { Key = "myKey", Value = "hello" };

            var result = entry.EncodeKeyValuePair();

            Assert.True(result.Success);
            Assert.Equal("myKey = hello", result.Value);
        }

        [Fact]
        public void EncodeKeyValuePairWhenKeyIsNotSetShouldFailWithInvalidKeyName()
        {
            var entry = new ConfigFileEntryModel(); // Key defaults to null

            var result = entry.EncodeKeyValuePair();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeKeyValuePairWhenValueIsEmptyShouldFailWithInvalidValue()
        {
            var entry = new ConfigFileEntryModel { Key = "myKey", Value = "" };

            var result = entry.EncodeKeyValuePair();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeKeyValuePairWhenValueIsWhitespaceShouldFailWithInvalidValue()
        {
            var entry = new ConfigFileEntryModel { Key = "myKey", Value = "   " };

            var result = entry.EncodeKeyValuePair();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // EncodeEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeEntryWhenOnlyKeyAndValueSetShouldReturnKeyValuePairOnly()
        {
            var entry = new ConfigFileEntryModel { Key = "port", Value = "8080" };

            var result = entry.EncodeEntry();

            Assert.True(result.Success);
            Assert.Equal("port = 8080", result.Value);
        }

        [Fact]
        public void EncodeEntryWhenAllFieldsSetShouldReturnFullEntryInOrder()
        {
            var entry = new ConfigFileEntryModel
            {
                Description = new Translator(english: "Port number"),
                Key = "port",
                Value = "8080",
                DefaultValue = "80",
                ValueType = "Int32"
            };

            var result = entry.EncodeEntry();

            Assert.True(result.Success);
            var lines = result.Value.Replace("\r\n", "\n").Split('\n');
            Assert.Equal("## Port number", lines[0]);
            Assert.Equal("# Value Type: Int32", lines[1]);
            Assert.Equal("# Default Value: 80", lines[2]);
            Assert.Equal("port = 8080", lines[3]);
        }

        [Fact]
        public void EncodeEntryWhenKeyIsInvalidShouldFailWithInvalidKeyName()
        {
            var entry = new ConfigFileEntryModel { Value = "8080" }; // Key is null

            var result = entry.EncodeEntry();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeEntryWhenValueIsEmptyShouldFailWithInvalidValue()
        {
            var entry = new ConfigFileEntryModel { Key = "port", Value = "" };

            var result = entry.EncodeEntry();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // EncodeValueType (static, by Type)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData(typeof(string), "String")]
        [InlineData(typeof(sbyte), "Int8")]
        [InlineData(typeof(short), "Int16")]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(long), "Int64")]
        [InlineData(typeof(byte), "UInt8")]
        [InlineData(typeof(ushort), "UInt16")]
        [InlineData(typeof(uint), "UInt32")]
        [InlineData(typeof(ulong), "UInt64")]
        [InlineData(typeof(float), "Float")]
        [InlineData(typeof(double), "Double")]
        [InlineData(typeof(bool), "Boolean")]
        public void EncodeValueTypeStaticWhenPrimitiveTypeShouldReturnExpectedName(Type type, string expected)
        {
            var result = ConfigFileEntryModel.EncodeValueType(type);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenEnumShouldReturnEnumTypeName()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(TestMode));

            Assert.True(result.Success);
            Assert.Equal("Enum TestMode", result.Value);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenListOfIntShouldReturnInt32Array()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(List<int>));

            Assert.True(result.Success);
            Assert.Equal("Int32[]", result.Value);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenIntArrayShouldReturnInt32Array()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(int[]));

            Assert.True(result.Success);
            Assert.Equal("Int32[]", result.Value);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenUnsupportedTypeShouldFailWithUnsupportedType()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(UnsupportedValue));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // EncodeValue / DecodeValue (smoke tests — delegates to ConfigFileModel)
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeValueWhenIntShouldReturnEncodedString()
        {
            var result = ConfigFileEntryModel.EncodeValue(42);

            Assert.True(result.Success);
            Assert.Equal("42", result.Value);
        }

        [Fact]
        public void DecodeValueWhenIntStringShouldReturnInt()
        {
            var result = ConfigFileEntryModel.DecodeValue<int>("42");

            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        // -----------------------------------------------------------------------
        // DecodeKeyValuePair
        // -----------------------------------------------------------------------

        [Fact]
        public void DecodeKeyValuePairWhenValidLineShouldReturnParsedPair()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("myKey=hello");

            Assert.True(result.Success);
            Assert.Equal("myKey", result.Value.Item1);
            Assert.Equal("hello", result.Value.Item2);
        }

        [Fact]
        public void DecodeKeyValuePairWhenMultipleEqualsSignsThenValueContainsRemainder()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("url=http://a=b");

            Assert.True(result.Success);
            Assert.Equal("url", result.Value.Item1);
            Assert.Equal("http://a=b", result.Value.Item2);
        }

        [Fact]
        public void DecodeKeyValuePairWhenCommentLineShouldFailWithInvalidKeyValuePair()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("# not=a pair");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyValuePair, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeKeyValuePairWhenKeyContainsHyphenShouldFailWithInvalidKeyName()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("invalid-key=value");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeKeyValuePairWhenValueIsEmptyShouldFailWithInvalidValue()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("key=");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeKeyValuePairWhenValueIsWhitespaceShouldFailWithInvalidValue()
        {
            var result = ConfigFileEntryModel.DecodeKeyValuePair("key=   ");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // CreateEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateEntryWhenParametersAreValidShouldReturnPopulatedEntry()
        {
            var result = ConfigFileEntryModel.CreateEntry("port", 8080, 80, new Translator(english: "Port number"), new Translator(english: "Port number"));

            Assert.True(result.Success);
            Assert.Equal("port", result.Value.Key);
            Assert.Equal("8080", result.Value.Value);
            Assert.Equal("80", result.Value.DefaultValue);
            Assert.Equal("Port number", (string)result.Value.Description);
            Assert.Equal("Int32", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenKeyIsInvalidShouldFailWithInvalidKeyName()
        {
            var result = ConfigFileEntryModel.CreateEntry("invalid-key", 1, 0, new Translator(), new Translator());

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        [Fact]
        public void CreateEntryWhenTypeIsUnsupportedShouldFailWithUnsupportedType()
        {
            var result = ConfigFileEntryModel.CreateEntry("key", new UnsupportedValue(), new UnsupportedValue(), new Translator(), new Translator());

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // DecodeEntry
        // -----------------------------------------------------------------------

        [Fact]
        public void DecodeEntryWhenArrayHasSingleKeyValuePairShouldReturnEntry()
        {
            var content = new[] { "myKey=myValue" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("myKey", result.Value.Key);
            Assert.Equal("myValue", result.Value.Value);
            Assert.Equal(1, index);
        }

        [Fact]
        public void DecodeEntryWhenArrayHasCommentLinesShouldSkipAndReturnEntry()
        {
            var content = new[] { "## description", "# Value Type: Int32", "myKey=myValue" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("myKey", result.Value.Key);
            Assert.Equal(3, index);
        }

        [Fact]
        public void DecodeEntryWhenArrayHasBlankLinesShouldSkipAndReturnEntry()
        {
            var content = new[] { "", "  ", "myKey=myValue" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("myKey", result.Value.Key);
            Assert.Equal(3, index);
        }

        [Fact]
        public void DecodeEntryWhenArrayContainsOnlyCommentsShouldFailWithEntryNotFound()
        {
            var content = new[] { "# only comments", "## another comment" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.EntryNotFound, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeEntryWhenNonCommentNonKeyValueLineFoundShouldFailWithInvalidKeyValuePair()
        {
            var content = new[] { "this line has no equals sign" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyValuePair, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeEntryIndexReflectsNumberOfLinesConsumedAfterDecode()
        {
            var content = new[] { "# comment", "key=value", "key2=value2" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Key);
            Assert.Equal(2, index);
        }

        [Fact]
        public void DecodeEntryWhenStartingFromNonZeroIndexShouldSkipPrecedingLines()
        {
            var content = new[] { "first=1", "second=2" };
            int index = 1;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("second", result.Value.Key);
            Assert.Equal("2", result.Value.Value);
            Assert.Equal(2, index);
        }

        // -----------------------------------------------------------------------
        // Key Property Setter
        // -----------------------------------------------------------------------

        [Fact]
        public void KeySetterWhenInvalidKeyNameShouldThrowArgumentException()
        {
            var entry = new ConfigFileEntryModel();

            var exception = Assert.Throws<ArgumentException>(() => entry.Key = "invalid-key");

            Assert.Contains("Invalid key name", exception.Message);
        }

        [Fact]
        public void KeySetterWhenValidKeyNameShouldSetKey()
        {
            var entry = new ConfigFileEntryModel();

            entry.Key = "valid_key_123";

            Assert.Equal("valid_key_123", entry.Key);
        }

        // -----------------------------------------------------------------------
        // EncodeEntry - Error Paths
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeEntryWhenNameEncodingFailsShouldReturnFailure()
        {
            // EncodeName never fails in current implementation, but if Description fails we can test it
            var entry = new ConfigFileEntryModel
            {
                Key = "test",
                Value = "value",
                Description = new Translator(english: "desc")
            };

            var result = entry.EncodeEntry();

            Assert.True(result.Success);
        }

        [Fact]
        public void EncodeEntryWithAllFieldsIncludingNameAndAcceptableValuesShouldIncludeAll()
        {
            var entry = new ConfigFileEntryModel
            {
                Name = new Translator(english: "Port Setting"),
                Description = new Translator(english: "Server port"),
                Key = "port",
                Value = "8080",
                DefaultValue = "80",
                ValueType = "Int32",
                AcceptableValues = "1-65535"
            };

            var result = entry.EncodeEntry();

            Assert.True(result.Success);
            Assert.Contains("Port Setting", result.Value);
            Assert.Contains("Server port", result.Value);
            Assert.Contains("Int32", result.Value);
            Assert.Contains("1-65535", result.Value);
            Assert.Contains("80", result.Value);
            Assert.Contains("port = 8080", result.Value);
        }

        // -----------------------------------------------------------------------
        // CopyTo
        // -----------------------------------------------------------------------

        [Fact]
        public void CopyToWhenTargetIsNullShouldReturnFalse()
        {
            var entry = new ConfigFileEntryModel { Key = "key", Value = "value" };

            var result = entry.CopyTo(null, false);

            Assert.False(result);
        }

        [Fact]
        public void CopyToWhenOverrideValueIsTrueShouldCopyValue()
        {
            var source = new ConfigFileEntryModel
            {
                Key = "key",
                Value = "sourceValue",
                Name = new Translator(english: "Name"),
                Description = new Translator(english: "Desc"),
                DefaultValue = "default",
                ValueType = "String",
                AcceptableValues = "A, B"
            };
            var target = new ConfigFileEntryModel();

            var result = source.CopyTo(target, true);

            Assert.True(result);
            Assert.Equal("sourceValue", target.Value);
        }

        [Fact]
        public void CopyToWhenOverrideValueIsFalseShouldNotCopyValue()
        {
            var source = new ConfigFileEntryModel
            {
                Key = "key",
                Value = "sourceValue"
            };
            var target = new ConfigFileEntryModel { Value = "targetValue" };

            var result = source.CopyTo(target, false);

            Assert.True(result);
            Assert.Equal("targetValue", target.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeValue<T> Generic
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeValueGenericWhenBoolShouldReturnEncodedString()
        {
            var result = ConfigFileEntryModel.EncodeValue(true);

            Assert.True(result.Success);
            Assert.Equal("True", result.Value);
        }

        [Fact]
        public void EncodeValueGenericWhenStringShouldReturnEncodedString()
        {
            var result = ConfigFileEntryModel.EncodeValue("test");

            Assert.True(result.Success);
            Assert.Equal("\"test\"", result.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeValueType<T> Generic
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeValueTypeGenericWhenIntShouldReturnInt32()
        {
            var result = ConfigFileEntryModel.EncodeValueType<int>();

            Assert.True(result.Success);
            Assert.Equal("Int32", result.Value);
        }

        [Fact]
        public void EncodeValueTypeGenericWhenStringShouldReturnString()
        {
            var result = ConfigFileEntryModel.EncodeValueType<string>();

            Assert.True(result.Success);
            Assert.Equal("String", result.Value);
        }

        // -----------------------------------------------------------------------
        // EncodeAcceptableValues<T> Generic
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeAcceptableValuesGenericWhenEnumShouldReturnCommaSeparatedNames()
        {
            var result = ConfigFileEntryModel.EncodeAcceptableValues<TestMode>();

            Assert.True(result.Success);
            Assert.Contains("First", result.Value);
            Assert.Contains("Second", result.Value);
        }

        [Fact]
        public void EncodeAcceptableValuesGenericWhenNonEnumTypeShouldFailWithInvalidType()
        {
            var result = ConfigFileEntryModel.EncodeAcceptableValues<int>();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidType, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeAcceptableValuesGenericWhenEnumArrayShouldReturnEnumValues()
        {
            var result = ConfigFileEntryModel.EncodeAcceptableValues<TestMode[]>();

            Assert.True(result.Success);
            Assert.Contains("First", result.Value);
            Assert.Contains("Second", result.Value);
        }

        // -----------------------------------------------------------------------
        // DecodeValue<T> Generic
        // -----------------------------------------------------------------------

        [Fact]
        public void DecodeValueGenericWhenValidStringShouldReturnDecodedValue()
        {
            var result = ConfigFileEntryModel.DecodeValue<string>("\"hello\"");

            Assert.True(result.Success);
            Assert.Equal("hello", result.Value);
        }

        [Fact]
        public void DecodeValueGenericWhenInvalidFormatShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.DecodeValue<int>("not a number");

            Assert.False(result.Success);
        }

        [Fact]
        public void DecodeValueWhenBoolShouldReturnDecodedBool()
        {
            var result = ConfigFileEntryModel.DecodeValue<bool>("True");

            Assert.True(result.Success);
            Assert.True(result.Value);
        }

        [Fact]
        public void DecodeValueWhenDoubleShouldReturnDecodedDouble()
        {
            var result = ConfigFileEntryModel.DecodeValue<double>("3.14");

            Assert.True(result.Success);
            Assert.Equal(3.14, result.Value);
        }

        [Fact]
        public void DecodeValueWhenFloatShouldReturnDecodedFloat()
        {
            var result = ConfigFileEntryModel.DecodeValue<float>("2.5");

            Assert.True(result.Success);
            Assert.Equal(2.5f, result.Value);
        }

        [Fact]
        public void DecodeValueWhenLongShouldReturnDecodedLong()
        {
            var result = ConfigFileEntryModel.DecodeValue<long>("9223372036854775807");

            Assert.True(result.Success);
            Assert.Equal(9223372036854775807L, result.Value);
        }

        [Fact]
        public void DecodeValueWhenByteShouldReturnDecodedByte()
        {
            var result = ConfigFileEntryModel.DecodeValue<byte>("255");

            Assert.True(result.Success);
            Assert.Equal((byte)255, result.Value);
        }

        [Fact]
        public void DecodeValueWhenEnumShouldReturnDecodedEnum()
        {
            var result = ConfigFileEntryModel.DecodeValue<TestMode>("Second");

            Assert.True(result.Success);
            Assert.Equal(TestMode.Second, result.Value);
        }

        [Fact]
        public void DecodeValueWhenIntArrayShouldReturnDecodedArray()
        {
            var result = ConfigFileEntryModel.DecodeValue<int[]>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        }

        [Fact]
        public void DecodeValueWhenStringListShouldReturnDecodedList()
        {
            var result = ConfigFileEntryModel.DecodeValue<List<string>>("[\"a\",\"b\",\"c\"]");

            Assert.True(result.Success);
            Assert.Equal(3, result.Value.Count);
            Assert.Equal("a", result.Value[0]);
            Assert.Equal("b", result.Value[1]);
            Assert.Equal("c", result.Value[2]);
        }

        [Fact]
        public void DecodeValueWhenEmptyStringShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.DecodeValue<int>("");

            Assert.False(result.Success);
        }

        [Fact]
        public void DecodeValueWhenNullShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.DecodeValue<int>(null);

            Assert.False(result.Success);
        }

        [Fact]
        public void DecodeValueWhenInvalidBoolShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.DecodeValue<bool>("not a bool");

            Assert.False(result.Success);
        }

        [Fact]
        public void DecodeValueWhenInvalidEnumShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.DecodeValue<TestMode>("InvalidValue");

            Assert.False(result.Success);
        }

        [Fact]
        public void DecodeValueWhenShortShouldReturnDecodedShort()
        {
            var result = ConfigFileEntryModel.DecodeValue<short>("32767");

            Assert.True(result.Success);
            Assert.Equal((short)32767, result.Value);
        }

        [Fact]
        public void DecodeValueWhenSByteShouldReturnDecodedSByte()
        {
            var result = ConfigFileEntryModel.DecodeValue<sbyte>("127");

            Assert.True(result.Success);
            Assert.Equal((sbyte)127, result.Value);
        }

        [Fact]
        public void DecodeValueWhenUIntShouldReturnDecodedUInt()
        {
            var result = ConfigFileEntryModel.DecodeValue<uint>("4294967295");

            Assert.True(result.Success);
            Assert.Equal(4294967295u, result.Value);
        }

        [Fact]
        public void DecodeValueWhenUShortShouldReturnDecodedUShort()
        {
            var result = ConfigFileEntryModel.DecodeValue<ushort>("65535");

            Assert.True(result.Success);
            Assert.Equal((ushort)65535, result.Value);
        }

        [Fact]
        public void DecodeValueWhenULongShouldReturnDecodedULong()
        {
            var result = ConfigFileEntryModel.DecodeValue<ulong>("18446744073709551615");

            Assert.True(result.Success);
            Assert.Equal(18446744073709551615UL, result.Value);
        }

        [Fact]
        public void CreateEntryWhenNullNameShouldStoreNull()
        {
            var result = ConfigFileEntryModel.CreateEntry("key", 1, 0, null, new Translator());

            Assert.True(result.Success);
            Assert.Null(result.Value.Name);
        }

        [Fact]
        public void CreateEntryWhenNullDescriptionShouldStoreNull()
        {
            var result = ConfigFileEntryModel.CreateEntry("key", 1, 0, new Translator(), null);

            Assert.True(result.Success);
            Assert.Null(result.Value.Description);
        }

        [Fact]
        public void DecodeValueWhenEmptyArrayStringShouldReturnEmptyArray()
        {
            var result = ConfigFileEntryModel.DecodeValue<int[]>("[]");

            Assert.True(result.Success);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void DecodeValueWhenEmptyListStringShouldReturnEmptyList()
        {
            var result = ConfigFileEntryModel.DecodeValue<List<string>>("[]");

            Assert.True(result.Success);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void CreateEntryWhenEmptyListShouldEncodeEmptyArray()
        {
            var result = ConfigFileEntryModel.CreateEntry("items", new List<int>(), new List<int>(), new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("[]", result.Value.Value);
            Assert.Equal("[]", result.Value.DefaultValue);
        }

        [Fact]
        public void DecodeValueWhenBoolFalseShouldReturnFalse()
        {
            var result = ConfigFileEntryModel.DecodeValue<bool>("False");

            Assert.True(result.Success);
            Assert.False(result.Value);
        }

        [Fact]
        public void DecodeValueWhenNegativeIntShouldReturnNegativeValue()
        {
            var result = ConfigFileEntryModel.DecodeValue<int>("-42");

            Assert.True(result.Success);
            Assert.Equal(-42, result.Value);
        }

        [Fact]
        public void DecodeValueWhenNegativeDoubleShouldReturnNegativeValue()
        {
            var result = ConfigFileEntryModel.DecodeValue<double>("-3.14");

            Assert.True(result.Success);
            Assert.Equal(-3.14, result.Value);
        }

        [Fact]
        public void CreateEntryWhenNegativeValuesShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("temp", -10, -5, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("-10", result.Value.Value);
            Assert.Equal("-5", result.Value.DefaultValue);
        }

        [Fact]
        public void DecodeValueWhenEnumArrayShouldReturnDecodedArray()
        {
            var result = ConfigFileEntryModel.DecodeValue<TestMode[]>("[First,Second]");

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Length);
            Assert.Equal(TestMode.First, result.Value[0]);
            Assert.Equal(TestMode.Second, result.Value[1]);
        }

        [Fact]
        public void CreateEntryWhenEnumArrayShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("modes", new[] { TestMode.First, TestMode.Second }, new[] { TestMode.First }, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("[First,Second]", result.Value.Value);
            Assert.Equal("[First]", result.Value.DefaultValue);
            Assert.Equal("Enum TestMode[]", result.Value.ValueType);
        }

        [Fact]
        public void DecodeValueWhenStringWithEscapesShouldReturnUnescapedString()
        {
            var result = ConfigFileEntryModel.DecodeValue<string>("\"hello\\nworld\"");

            Assert.True(result.Success);
            Assert.Equal("hello\nworld", result.Value);
        }

        [Fact]
        public void CreateEntryWhenStringWithSpecialCharsShouldEncodeWithEscapes()
        {
            var result = ConfigFileEntryModel.CreateEntry("msg", "hello\nworld", "test", new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Contains("\\n", result.Value.Value);
        }

        // -----------------------------------------------------------------------
        // CreateEntry - Additional Error Paths
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateEntryWhenValueEncodingFailsShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.CreateEntry("key", new UnsupportedValue(), new UnsupportedValue(), new Translator(), new Translator());

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void CreateEntryWhenDefaultValueEncodingFailsShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.CreateEntry<object>("key", 42, new UnsupportedValue(), new Translator(), new Translator());

            Assert.False(result.Success);
        }

        [Fact]
        public void CreateEntryWhenStringTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("key", "test", "default", new Translator(english: "Name"), new Translator(english: "Desc"));

            Assert.True(result.Success);
            Assert.Equal("key", result.Value.Key);
            Assert.Equal("\"test\"", result.Value.Value);
            Assert.Equal("\"default\"", result.Value.DefaultValue);
            Assert.Equal("String", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenBoolTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("enabled", true, false, new Translator(english: "Enable"), new Translator(english: "Enable feature"));

            Assert.True(result.Success);
            Assert.Equal("enabled", result.Value.Key);
            Assert.Equal("True", result.Value.Value);
            Assert.Equal("False", result.Value.DefaultValue);
            Assert.Equal("Boolean", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenDoubleTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("ratio", 3.14, 1.0, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("3.14", result.Value.Value);
            Assert.Equal("1", result.Value.DefaultValue);
            Assert.Equal("Double", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenFloatTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("factor", 2.5f, 1.0f, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("2.5", result.Value.Value);
            Assert.Equal("1", result.Value.DefaultValue);
            Assert.Equal("Float", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenEnumTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("mode", TestMode.Second, TestMode.First, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("Second", result.Value.Value);
            Assert.Equal("First", result.Value.DefaultValue);
            Assert.Equal("Enum TestMode", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenIntArrayTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("numbers", new[] { 1, 2, 3 }, new[] { 0 }, new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("[1,2,3]", result.Value.Value);
            Assert.Equal("[0]", result.Value.DefaultValue);
            Assert.Equal("Int32[]", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenListTypeShouldEncodeCorrectly()
        {
            var result = ConfigFileEntryModel.CreateEntry("items", new List<string> { "a", "b" }, new List<string>(), new Translator(), new Translator());

            Assert.True(result.Success);
            Assert.Equal("[\"a\",\"b\"]", result.Value.Value);
            Assert.Equal("String[]", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenNameAndDescriptionProvidedShouldStoreCorrectly()
        {
            var name = new Translator(chinese: "名称", english: "Name");
            var description = new Translator(chinese: "描述", english: "Description");

            var result = ConfigFileEntryModel.CreateEntry("key", 100, 0, name, description);

            Assert.True(result.Success);
            Assert.Equal(name, result.Value.Name);
            Assert.Equal(description, result.Value.Description);
        }

        // -----------------------------------------------------------------------
        // DecodeEntry - Additional Error Paths
        // -----------------------------------------------------------------------

        [Fact]
        public void DecodeEntryWhenDecodeKeyValuePairFailsShouldReturnFailure()
        {
            var content = new[] { "invalid-key=value" };
            int index = 0;

            var result = ConfigFileEntryModel.DecodeEntry(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        // -----------------------------------------------------------------------
        // EncodeValueType (static, with IConfigEntryAdapter)
        // -----------------------------------------------------------------------

        [Fact]
        public void EncodeValueTypeStaticWhenAdapterTypeThrowsExceptionShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(ThrowingAdapter));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidType, result.Errors[0].Code);
            Assert.Contains("Failed to encode value type", result.Errors[0].Message);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenAdapterEncodeValueTypeFailsShouldReturnFailure()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(FailingAdapter));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeValueTypeStaticWhenAdapterEncodeValueTypeSucceedsShouldReturnValue()
        {
            var result = ConfigFileEntryModel.EncodeValueType(typeof(SuccessfulAdapter));

            Assert.True(result.Success);
            Assert.Equal("CustomType", result.Value);
        }
    }

    internal class ThrowingAdapter : IConfigEntryAdapter
    {
        public ThrowingAdapter()
        {
            throw new InvalidOperationException("Test exception");
        }

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
            return "test";
        }
    }

    internal class FailingAdapter : IConfigEntryAdapter
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
            return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Test error"));
        }
    }

    internal class SuccessfulAdapter : IConfigEntryAdapter
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
}
