using System;
using System.Collections.Generic;
using BetterExperience.ConfigFileSpace;

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
            var entry = new ConfigFileEntryModel { Description = "" };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenSingleLineShouldReturnPrefixedLine()
        {
            var entry = new ConfigFileEntryModel { Description = "hello" };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## hello", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenMultiLineNewlineShouldReturnMultiplePrefixedLines()
        {
            var entry = new ConfigFileEntryModel { Description = "line1\nline2" };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## line1" + Environment.NewLine + "## line2", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenCrLfShouldNormalizeToNewline()
        {
            var entry = new ConfigFileEntryModel { Description = "line1\r\nline2" };

            var result = entry.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal("## line1" + Environment.NewLine + "## line2", result.Value);
        }

        [Fact]
        public void EncodeDescriptionWhenCrOnlyShouldNormalizeToNewline()
        {
            var entry = new ConfigFileEntryModel { Description = "line1\rline2" };

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
                Description = "Port number",
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
            var result = ConfigFileEntryModel.CreatEntry("port", 8080, 80, "Port number");

            Assert.True(result.Success);
            Assert.Equal("port", result.Value.Key);
            Assert.Equal("8080", result.Value.Value);
            Assert.Equal("80", result.Value.DefaultValue);
            Assert.Equal("Port number", result.Value.Description);
            Assert.Equal("Int32", result.Value.ValueType);
        }

        [Fact]
        public void CreateEntryWhenKeyIsInvalidShouldFailWithInvalidKeyName()
        {
            var result = ConfigFileEntryModel.CreatEntry("invalid-key", 1, 0, "");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
        }

        [Fact]
        public void CreateEntryWhenTypeIsUnsupportedShouldFailWithUnsupportedType()
        {
            var result = ConfigFileEntryModel.CreatEntry("key", new UnsupportedValue(), new UnsupportedValue(), "");

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
    }
}
