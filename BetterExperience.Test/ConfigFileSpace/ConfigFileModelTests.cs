using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BetterExperience.HConfigFileSpace;

namespace BetterExperience.Test.ConfigFileSpace
{
    public class ConfigFileModelTests
    {
        [Theory]
        [InlineData(123, "123")]
        [InlineData(-45, "-45")]
        [InlineData(0, "0")]
        public void EncodeIntShouldReturnInvariantString(int value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData(true, "True")]
        [InlineData(false, "False")]
        public void EncodeBoolShouldReturnInvariantString(bool value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("-45", -45)]
        [InlineData("0", 0)]
        public void DecodeIntShouldReturnTypedValue(string raw, int expected)
        {
            var result = ConfigFileModel.Decode<int>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData("True", true)]
        [InlineData("false", false)]
        public void DecodeBoolShouldReturnTypedValue(string raw, bool expected)
        {
            var result = ConfigFileModel.Decode<bool>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void EncodeStringAndDecodeStringShouldRoundTripEscapedContent()
        {
            const string original = "line1\nline2\t\"quoted\"\\tail";

            var encoded = ConfigFileModel.EncodeString(original);
            var decoded = ConfigFileModel.DecodeString(encoded);

            Assert.True(decoded.Success);
            Assert.Equal(original, decoded.Value);
        }

        [Fact]
        public void DecodeStringShouldPreserveLiteralBackslashSequence()
        {
            const string original = "prefix\\nsuffix";

            var encoded = ConfigFileModel.EncodeString(original);
            var decoded = ConfigFileModel.DecodeString(encoded);

            Assert.True(decoded.Success);
            Assert.Equal(original, decoded.Value);
        }

        [Fact]
        public void DecodeStringShouldReturnEmptyStringForWhitespaceInput()
        {
            var result = ConfigFileModel.DecodeString("   ");

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void DecodeStringShouldFailWhenQuotesAreMissing()
        {
            var result = ConfigFileModel.DecodeString("abc");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeStringShouldFailForInvalidEscapeSequence()
        {
            var result = ConfigFileModel.DecodeString("\"bad\\x\"");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeCollectionShouldEncodeElementValuesInsteadOfResultTypeName()
        {
            var values = new List<string> { "a,b", "c" };

            var result = ConfigFileModel.Encode(values, values.GetType());

            Assert.True(result.Success);
            Assert.Equal("[\"a,b\",\"c\"]", result.Value);
        }

        [Fact]
        public void EncodeNestedCollectionShouldReturnNestedCollectionString()
        {
            var values = new List<List<int>>
            {
                new List<int> { 1, 2 },
                new List<int> { 3, 4 }
            };

            var result = ConfigFileModel.Encode(values, values.GetType());

            Assert.True(result.Success);
            Assert.Equal("[[1,2],[3,4]]", result.Value);
        }

        [Fact]
        public void EncodeShouldFailForNullValue()
        {
            var result = ConfigFileModel.Encode(null, typeof(string));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeShouldFailForNullType()
        {
            var result = ConfigFileModel.Encode("abc", null);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeShouldFailForUnsupportedType()
        {
            var result = ConfigFileModel.Encode(new UnsupportedValue(), typeof(UnsupportedValue));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeGenericListShouldReturnTypedList()
        {
            var result = ConfigFileModel.Decode<List<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        }

        [Fact]
        public void DecodeEmptyGenericListShouldReturnEmptyList()
        {
            var result = ConfigFileModel.Decode<List<int>>("[]");

            Assert.True(result.Success);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void DecodeArrayShouldReturnTypedArray()
        {
            var result = ConfigFileModel.Decode<string[]>("[\"a,b\",\"c\"]");

            Assert.True(result.Success);
            Assert.Equal(new[] { "a,b", "c" }, result.Value);
        }

        [Fact]
        public void DecodeNestedGenericListShouldReturnNestedTypedLists()
        {
            var result = ConfigFileModel.Decode<List<List<int>>>("[[1,2],[3,4]]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2 }, result.Value[0]);
            Assert.Equal(new[] { 3, 4 }, result.Value[1]);
        }

        [Fact]
        public void DecodeIEnumerableShouldReturnAssignableTypedSequence()
        {
            var result = ConfigFileModel.Decode<IEnumerable<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToArray());
        }

        [Fact]
        public void DecodeICollectionShouldReturnAssignableTypedCollection()
        {
            var result = ConfigFileModel.Decode<ICollection<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToArray());
        }

        [Fact]
        public void DecodeIReadOnlyListShouldReturnListAssignableToInterface()
        {
            var result = ConfigFileModel.Decode<IReadOnlyList<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        }

        [Fact]
        public void DecodeReadOnlyCollectionShouldUseSingleArgumentConstructor()
        {
            var result = ConfigFileModel.Decode<ReadOnlyCollection<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value);
        }

        [Fact]
        public void DecodeQueueShouldUseEnumerableCompatibleConstructor()
        {
            var result = ConfigFileModel.Decode<Queue<int>>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value.ToArray());
        }

        [Fact]
        public void DecodeHashSetShouldUseDefaultConstructorAndAddMethod()
        {
            var result = ConfigFileModel.Decode<HashSet<int>>("[1,1,2]");

            Assert.True(result.Success);
            Assert.True(result.Value.SetEquals(new[] { 1, 2 }));
        }

        [Fact]
        public void DecodeCustomCollectionWithArrayConstructorShouldSucceed()
        {
            var result = ConfigFileModel.Decode<ArrayConstructorCollection>("[1,2,3]");

            Assert.True(result.Success);
            Assert.Equal(new[] { 1, 2, 3 }, result.Value.Items);
        }

        [Fact]
        public void DecodeEnumShouldIgnoreCase()
        {
            var result = ConfigFileModel.Decode<TestMode>("second");

            Assert.True(result.Success);
            Assert.Equal(TestMode.Second, result.Value);
        }

        [Fact]
        public void DecodeShouldFailForWhitespaceInput()
        {
            var result = ConfigFileModel.Decode<int>("   ");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeShouldFailForNullType()
        {
            var result = ConfigFileModel.Decode("1", null);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeShouldFailForInvalidInt()
        {
            var result = ConfigFileModel.Decode<int>("abc");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeShouldFailForInvalidEnum()
        {
            var result = ConfigFileModel.Decode<TestMode>("third");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeShouldFailForUnsupportedType()
        {
            var result = ConfigFileModel.Decode("anything", typeof(UnsupportedValue));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeCollectionOfUnsupportedElementTypeShouldFailGracefully()
        {
            var result = ConfigFileModel.Decode<List<UnsupportedValue>>("[anything]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void SplitCollectionStringShouldHandleQuotedCommasAndNestedCollections()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"a,b\",[1,2],true]");

            Assert.True(result.Success);
            Assert.Equal(new[] { "\"a,b\"", "[1,2]", "true" }, result.Value);
        }

        [Fact]
        public void SplitCollectionStringShouldReturnEmptyArrayForEmptyCollection()
        {
            var result = ConfigFileModel.SplitCollectionString("[]");

            Assert.True(result.Success);
            Assert.Empty(result.Value);
        }

        [Fact]
        public void SplitCollectionStringShouldFailWhenCollectionContainsEmptyElement()
        {
            var result = ConfigFileModel.SplitCollectionString("[1,,2]");

            Assert.False(result.Success);
            Assert.Contains(result.Errors, error => error.Code == ConfigFileErrorCode.InvalidValue);
        }

        [Fact]
        public void SplitCollectionStringShouldFailForUnclosedQuotedString()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"abc]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void SplitCollectionStringShouldFailForUnbalancedNestedCollection()
        {
            var result = ConfigFileModel.SplitCollectionString("[[1,2]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void SplitCollectionStringShouldFailForInvalidEscapeSequence()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"a\\x\"]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void GetCollectionElementTypeShouldReturnArrayElementType()
        {
            var result = ConfigFileModel.GetCollectionElementType(typeof(int[]));

            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void GetCollectionElementTypeShouldReturnGenericCollectionElementType()
        {
            var result = ConfigFileModel.GetCollectionElementType(typeof(List<string>));

            Assert.Equal(typeof(string), result);
        }

        [Fact]
        public void GetCollectionElementTypeShouldReturnNullForNonGenericCollection()
        {
            var result = ConfigFileModel.GetCollectionElementType(typeof(ArrayList));

            Assert.Null(result);
        }

        [Fact]
        public void DecodeUnsupportedCollectionShouldFailGracefully()
        {
            var result = ConfigFileModel.Decode<UnsupportedIntCollection>("[1,2]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeCollectionWithThrowingConstructorShouldFailGracefully()
        {
            var result = ConfigFileModel.Decode<ThrowingConstructorIntCollection>("[1,2]");

            Assert.False(result.Success);
            Assert.Contains("constructor failure", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeCollectionWithThrowingAddShouldFailGracefully()
        {
            var result = ConfigFileModel.Decode<ThrowingAddIntCollection>("[1,2]");

            Assert.False(result.Success);
            Assert.Contains("add failure", result.Errors[0].Message);
        }

        [Theory]
        [InlineData((sbyte)127, "127")]
        [InlineData((sbyte)-128, "-128")]
        [InlineData((sbyte)0, "0")]
        public void Encode_SByteValue_ReturnsInvariantString(sbyte value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData((short)32767, "32767")]
        [InlineData((short)-32768, "-32768")]
        [InlineData((short)0, "0")]
        public void Encode_ShortValue_ReturnsInvariantString(short value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData((byte)255, "255")]
        [InlineData((byte)0, "0")]
        public void Encode_ByteValue_ReturnsInvariantString(byte value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData((ushort)65535, "65535")]
        [InlineData((ushort)0, "0")]
        public void Encode_UShortValue_ReturnsInvariantString(ushort value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData(4294967295u, "4294967295")]
        [InlineData(0u, "0")]
        public void Encode_UIntValue_ReturnsInvariantString(uint value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData(18446744073709551615ul, "18446744073709551615")]
        [InlineData(0ul, "0")]
        public void Encode_ULongValue_ReturnsInvariantString(ulong value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData(123.45, "123.45")]
        [InlineData(-123.45, "-123.45")]
        [InlineData(0.0, "0")]
        public void Encode_DoubleValue_ReturnsInvariantString(double value, string expected)
        {
            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Theory]
        [InlineData("127", (sbyte)127)]
        [InlineData("-128", (sbyte)-128)]
        [InlineData("0", (sbyte)0)]
        public void Decode_SByteString_ReturnsTypedValue(string raw, sbyte expected)
        {
            var result = ConfigFileModel.Decode<sbyte>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidSByteString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<sbyte>("999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("32767", (short)32767)]
        [InlineData("-32768", (short)-32768)]
        [InlineData("0", (short)0)]
        public void Decode_ShortString_ReturnsTypedValue(string raw, short expected)
        {
            var result = ConfigFileModel.Decode<short>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidShortString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<short>("99999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("255", (byte)255)]
        [InlineData("0", (byte)0)]
        public void Decode_ByteString_ReturnsTypedValue(string raw, byte expected)
        {
            var result = ConfigFileModel.Decode<byte>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidByteString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<byte>("999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("65535", (ushort)65535)]
        [InlineData("0", (ushort)0)]
        public void Decode_UShortString_ReturnsTypedValue(string raw, ushort expected)
        {
            var result = ConfigFileModel.Decode<ushort>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidUShortString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<ushort>("99999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("4294967295", 4294967295u)]
        [InlineData("0", 0u)]
        public void Decode_UIntString_ReturnsTypedValue(string raw, uint expected)
        {
            var result = ConfigFileModel.Decode<uint>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidUIntString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<uint>("999999999999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("18446744073709551615", 18446744073709551615ul)]
        [InlineData("0", 0ul)]
        public void Decode_ULongString_ReturnsTypedValue(string raw, ulong expected)
        {
            var result = ConfigFileModel.Decode<ulong>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidULongString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<ulong>("99999999999999999999999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Theory]
        [InlineData("123.45", 123.45)]
        [InlineData("-123.45", -123.45)]
        [InlineData("0", 0.0)]
        public void Decode_DoubleString_ReturnsTypedValue(string raw, double expected)
        {
            var result = ConfigFileModel.Decode<double>(raw);

            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void Decode_InvalidDoubleString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<double>("not_a_number");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void Decode_InvalidLongString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<long>("99999999999999999999999");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void Decode_InvalidFloatString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<float>("not_a_number");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void Decode_InvalidBoolString_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<bool>("not_a_bool");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeString_WithTrimOption_TrimmedAndEscaped()
        {
            var result = ConfigFileModel.EncodeString("  test  ", quote: true, trim: true, escape: true);

            Assert.Equal("\"test\"", result);
        }

        [Fact]
        public void Encode_NullCollectionType_ReturnsFail()
        {
            var mockCollection = new MockNullTypeCollection();
            var result = ConfigFileModel.Encode(mockCollection, mockCollection.GetType());

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void Encode_CollectionWithFailedElement_ReturnsFail()
        {
            var collection = new List<UnsupportedValue> { new UnsupportedValue() };
            var result = ConfigFileModel.Encode(collection, collection.GetType());

            Assert.False(result.Success);
        }

        [Fact]
        public void Decode_NullCollectionType_ReturnsFail()
        {
            var result = ConfigFileModel.Decode("[1,2,3]", typeof(MockNullTypeCollection));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, result.Errors[0].Code);
        }

        [Fact]
        public void Decode_InvalidCollectionSplitResult_ReturnsFail()
        {
            var result = ConfigFileModel.Decode<List<int>>("[1,,2]");

            Assert.False(result.Success);
        }

        [Fact]
        public void SplitCollectionString_NullInput_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString(null);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("null or whitespace", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_WhitespaceInput_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("   ");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("null or whitespace", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_MissingOpeningBracket_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("1,2,3]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("must start with '[' and end with ']'", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_MissingClosingBracket_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("[1,2,3");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("must start with '[' and end with ']'", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_EscapeSequenceAtEnd_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"test\\]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Invalid escape sequence at end of string", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_ValidEscapeSequences_ReturnsSuccess()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"test\\\\value\",\"line\\nbreak\"]");

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Length);
            Assert.Equal("\"test\\\\value\"", result.Value[0]);
            Assert.Equal("\"line\\nbreak\"", result.Value[1]);
        }

        [Fact]
        public void SplitCollectionString_UnexpectedClosingBracket_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("[1],2]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Unexpected closing bracket", result.Errors[0].Message);
        }

        [Fact]
        public void SplitCollectionString_EmptyLastElement_ReturnsFail()
        {
            var result = ConfigFileModel.SplitCollectionString("[1,2, ]");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("empty element", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_NullType_ReturnsFail()
        {
            var result = ConfigFileModel.CreateCollectionResult(null, typeof(int), new List<object> { 1, 2 });

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Type cannot be null", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_NullElementType_ReturnsFail()
        {
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), null, new List<object> { 1, 2 });

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Element type cannot be null", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_NullElements_ReturnsFail()
        {
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), null);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Elements cannot be null", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_InvalidElementValidation_ReturnsFail()
        {
            var elements = new List<object> { "notAnInt" };
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), elements);

            Assert.False(result.Success);
        }

        [Fact]
        public void CreateCollectionResult_ArrayType_ReturnsArray()
        {
            var elements = new List<object> { 1, 2, 3 };
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), elements);

            Assert.True(result.Success);
            Assert.IsType<int[]>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, (int[])result.Value);
        }

        [Fact]
        public void ValidateCollectionElements_InvalidElement_ReturnsFail()
        {
            var elements = new List<object> { 1, "notAnInt", 3 };
            var result = ConfigFileModel.ValidateCollectionElements(typeof(int), elements);

            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateCollectionElement_NullForValueType_ReturnsFail()
        {
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int), null, 0);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("cannot be null for value type", result.Errors[0].Message);
        }

        [Fact]
        public void ValidateCollectionElement_NullForNullableType_ReturnsSuccess()
        {
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int?), null, 0);

            Assert.True(result.Success);
            Assert.Null(result.Value);
        }

        [Fact]
        public void ValidateCollectionElement_NullForReferenceType_ReturnsSuccess()
        {
            var result = ConfigFileModel.ValidateCollectionElement(typeof(string), null, 0);

            Assert.True(result.Success);
            Assert.Null(result.Value);
        }

        [Fact]
        public void ValidateCollectionElement_TypeMismatch_ReturnsFail()
        {
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int), "notAnInt", 0);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("not assignable", result.Errors[0].Message);
        }

        [Fact]
        public void ValidateCollectionElement_ValidForNullableType_ReturnsSuccess()
        {
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int?), 42, 0);

            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void ValidateCollectionElement_BoxedValueForNullableType_ReturnsSuccess()
        {
            object boxedValue = 42;
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int?), boxedValue, 0);

            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void CreateTypedArray_InvalidCast_ReturnsFail()
        {
            var elements = new object[] { "notAnInt" };
            var result = ConfigFileModel.CreateTypedArray(typeof(int), elements);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Failed to create array", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_ArrayCreationFails_ReturnsFail()
        {
            var elements = new List<object> { "notAnInt" };
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), elements);

            Assert.False(result.Success);
        }

        [Fact]
        public void SplitCollectionString_ValidEscapeInQuotes_ReturnsSuccess()
        {
            var result = ConfigFileModel.SplitCollectionString("[\"test\\\\n\",\"value\\\"quoted\\\"\"]");

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Length);
            Assert.Contains("\\\\n", result.Value[0]);
            Assert.Contains("\\\"", result.Value[1]);
        }
    }

    public class MockNullTypeCollection : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return 1;
        }
    }

    public class ConfigFileModelHelperMethodsTests
    {
        [Fact]
        public void UnescapeString_BackslashR_ReturnsCarriageReturn()
        {
            var result = ConfigFileModel.UnescapeString("test\\rvalue");

            Assert.True(result.Success);
            Assert.Equal("test\rvalue", result.Value);
        }

        [Fact]
        public void UnescapeString_TrailingBackslash_ReturnsError()
        {
            var result = ConfigFileModel.UnescapeString("test\\");

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Invalid escape sequence at end of string", result.Errors[0].Message);
        }

        [Fact]
        public void CreateTypedList_ValidElements_ReturnsOk()
        {
            var elements = new object[] { 1, 2, 3 };

            var result = ConfigFileModel.CreateTypedList(typeof(int), elements);

            Assert.True(result.Success);
            Assert.Equal(3, result.Value.Count);
            Assert.Equal(1, result.Value[0]);
            Assert.Equal(2, result.Value[1]);
            Assert.Equal(3, result.Value[2]);
        }

        [Fact]
        public void CreateTypedList_InvalidElementValue_ReturnsError()
        {
            var elements = new object[] { "not an int" };

            var result = ConfigFileModel.CreateTypedList(typeof(int), elements);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Failed to populate helper list", result.Errors[0].Message);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_ValidConstructor_ReturnsTrue()
        {
            var list = new List<int> { 1, 2, 3 };
            var array = new int[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ReadOnlyCollection<int>), list, array, out var result);

            Assert.True(success);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_NoCompatibleConstructor_ReturnsFalse()
        {
            var list = new List<string> { "a", "b" };
            var array = new string[] { "a", "b" };

            var success = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(int), list, array, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_ThrowingConstructor_ReturnsTrueWithError()
        {
            var list = new List<int> { 1, 2, 3 };
            var array = new int[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ThrowingConstructorCollection), list, array, out var result);

            Assert.True(success);
            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Failed to construct collection type", result.Errors[0].Message);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_ValidAddMethod_ReturnsTrue()
        {
            var elements = new object[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(List<int>), typeof(int), elements, out var result);

            Assert.True(success);
            Assert.True(result.Success);
            var list = result.Value as List<int>;
            Assert.NotNull(list);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_NoDefaultConstructor_ReturnsFalse()
        {
            var elements = new object[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(ReadOnlyCollection<int>), typeof(int), elements, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_NoAddMethod_ReturnsFalse()
        {
            var elements = new object[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(NoAddMethodCollection), typeof(int), elements, out var result);

            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_SuccessWithElements_ReturnsOk()
        {
            var elements = new object[] { 1, 2, 3 };

            var success = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(List<int>), typeof(int), elements, out var result);

            Assert.True(success);
            Assert.True(result.Success);
        }

        [Fact]
        public void FindAddMethod_ValidAddMethod_ReturnsMethod()
        {
            var method = ConfigFileModel.FindAddMethod(typeof(List<int>), typeof(int));

            Assert.NotNull(method);
            Assert.Equal("Add", method.Name);
        }

        [Fact]
        public void FindAddMethod_NoCompatibleAddMethod_ReturnsNull()
        {
            var method = ConfigFileModel.FindAddMethod(typeof(NoAddMethodCollection), typeof(int));

            Assert.Null(method);
        }

        [Fact]
        public void FindAddMethod_AddMethodWithMultipleParameters_ReturnsNull()
        {
            var method = ConfigFileModel.FindAddMethod(typeof(MultiParameterAddCollection), typeof(int));

            Assert.Null(method);
        }

        [Fact]
        public void FindAddMethod_NullableElementType_ReturnsMethod()
        {
            var method = ConfigFileModel.FindAddMethod(typeof(List<int>), typeof(int?));

            Assert.NotNull(method);
            Assert.Equal("Add", method.Name);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_EmptyElements_ReturnsOkWithEmptyCollection()
        {
            var elements = new object[0];

            var success = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(List<int>), typeof(int), elements, out var result);

            Assert.True(success);
            Assert.True(result.Success);
            var list = result.Value as List<int>;
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public void FindAddMethod_IncompatibleParameterType_ReturnsNull()
        {
            var method = ConfigFileModel.FindAddMethod(typeof(StringAddOnlyCollection), typeof(int));

            Assert.Null(method);
        }
    }

    public class ConfigFileResultTests
    {
        [Fact]
        public void OkShouldProduceSuccessfulResult()
        {
            var result = ConfigFileResult<int>.Ok(42);

            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void FailShouldUseEmptyErrorsWhenNullArrayIsProvided()
        {
            var result = ConfigFileResult<int>.Fail((ConfigFileError[])null);

            Assert.False(result.Success);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ExplicitCastShouldReturnValueForSuccessfulResult()
        {
            var result = ConfigFileResult<int>.Ok(42);

            var value = (int)result;

            Assert.Equal(42, value);
        }

        [Fact]
        public void ExplicitCastShouldThrowForFailedResult()
        {
            var result = ConfigFileResult<int>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "invalid"));

            Assert.Throws<InvalidOperationException>(() =>
            {
                var _ = (int)result;
            });
        }

        [Fact]
        public void ImplicitConversionFromValueShouldProduceSuccessfulResult()
        {
            ConfigFileResult<int> result = 42;

            Assert.True(result.Success);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void ImplicitConversionToObjectResultShouldPreserveState()
        {
            ConfigFileResult<string> source = ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "unsupported"));

            ConfigFileResult<object> converted = source;

            Assert.False(converted.Success);
            Assert.Single(converted.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, converted.Errors[0].Code);
        }

        [Fact]
        public void Encode_LongValue_ShouldReturnInvariantString()
        {
            long value = 9223372036854775807L;

            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal("9223372036854775807", result.Value);
        }

        [Fact]
        public void Encode_FloatValue_ShouldReturnInvariantString()
        {
            float value = 3.14159f;

            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal("3.14159", result.Value);
        }

        [Fact]
        public void Encode_EnumValue_ShouldReturnEnumString()
        {
            var value = TestMode.First;

            var result = ConfigFileModel.Encode(value);

            Assert.True(result.Success);
            Assert.Equal("First", result.Value);
        }

        [Fact]
        public void Encode_IConfigEntryAdapter_WithFailedEncode_ShouldReturnFail()
        {
            var adapter = new EncodeFailingAdapter();

            var result = ConfigFileModel.Encode(adapter);

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void Encode_IConfigEntryAdapter_WithException_ShouldReturnFail()
        {
            var adapter = new EncodeThrowingAdapter();

            var result = ConfigFileModel.Encode(adapter);

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Failed to encode using IConfigEntryAdapter", result.Errors[0].Message);
        }

        [Fact]
        public void Decode_LongString_ShouldReturnTypedValue()
        {
            var result = ConfigFileModel.Decode<long>("9223372036854775807");

            Assert.True(result.Success);
            Assert.Equal(9223372036854775807L, result.Value);
        }

        [Fact]
        public void Decode_FloatString_ShouldReturnTypedValue()
        {
            var result = ConfigFileModel.Decode<float>("3.14159");

            Assert.True(result.Success);
            Assert.Equal(3.14159f, result.Value);
        }

        [Fact]
        public void Decode_IConfigEntryAdapter_WithFailedDecode_ShouldReturnFail()
        {
            var result = ConfigFileModel.Decode("test", typeof(DecodeFailingAdapter));

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void Decode_IConfigEntryAdapter_WithException_ShouldReturnFail()
        {
            var result = ConfigFileModel.Decode("test", typeof(DecodeThrowingAdapter));

            Assert.False(result.Success);
            Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
            Assert.Contains("Failed to decode using IConfigEntryAdapter", result.Errors[0].Message);
        }

        [Fact]
        public void CreateCollectionResult_WithArrayCreationFailure_ShouldReturnFail()
        {
            var elements = new List<object> { "not an int" };

            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), elements);

            Assert.False(result.Success);
        }

        [Fact]
        public void CreateCollectionResult_WithListCreationFailure_ShouldReturnFail()
        {
            var elements = new List<object> { "not an int" };

            var result = ConfigFileModel.CreateCollectionResult(typeof(List<int>), typeof(int), elements);

            Assert.False(result.Success);
        }
    }

    public enum TestMode
    {
        First,
        Second
    }

    public class UnsupportedValue
    {
    }

    public class ArrayConstructorCollection : IEnumerable<int>
    {
        public ArrayConstructorCollection(int[] items)
        {
            Items = items;
        }

        public IReadOnlyList<int> Items { get; }

        public IEnumerator<int> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class UnsupportedIntCollection : ICollection<int>
    {
        private readonly List<int> _items = new List<int>();

        private UnsupportedIntCollection()
        {
        }

        int ICollection<int>.Count => _items.Count;
        bool ICollection<int>.IsReadOnly => false;

        void ICollection<int>.Add(int item)
        {
            _items.Add(item);
        }

        void ICollection<int>.Clear()
        {
            _items.Clear();
        }

        bool ICollection<int>.Contains(int item)
        {
            return _items.Contains(item);
        }

        void ICollection<int>.CopyTo(int[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        bool ICollection<int>.Remove(int item)
        {
            return _items.Remove(item);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class ThrowingConstructorIntCollection : ICollection<int>
    {
        private readonly List<int> _items = new List<int>();

        public ThrowingConstructorIntCollection()
        {
            throw new InvalidOperationException("constructor failure");
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(int item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(int item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public bool Remove(int item)
        {
            return _items.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class ThrowingAddIntCollection : ICollection<int>
    {
        private readonly List<int> _items = new List<int>();

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(int item)
        {
            throw new InvalidOperationException("add failure");
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(int item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public bool Remove(int item)
        {
            return _items.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class NonConstructibleType
    {
    }

    public class ThrowingConstructorCollection : IEnumerable<int>
    {
        public ThrowingConstructorCollection(IEnumerable<int> items)
        {
            throw new InvalidOperationException("constructor failure");
        }

        public IEnumerator<int> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NoAddMethodCollection : IEnumerable<int>
    {
        private readonly List<int> _items = new List<int>();

        public IEnumerator<int> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class MultiParameterAddCollection : IEnumerable<int>
    {
        private readonly List<int> _items = new List<int>();

        public void Add(int item, int priority)
        {
            _items.Add(item);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class StringAddOnlyCollection : IEnumerable<string>
    {
        private readonly List<string> _items = new List<string>();

        public void Add(string item)
        {
            _items.Add(item);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    internal class EncodeFailingAdapter : IConfigEntryAdapter
    {
        public ConfigFileResult<string> Encode()
        {
            return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Encode failed"));
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

    internal class EncodeThrowingAdapter : IConfigEntryAdapter
    {
        public ConfigFileResult<string> Encode()
        {
            throw new InvalidOperationException("Encode exception");
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

    internal class DecodeFailingAdapter : IConfigEntryAdapter
    {
        public ConfigFileResult<string> Encode()
        {
            return "test";
        }

        public ConfigFileResult<object> Decode(string content)
        {
            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Decode failed"));
        }

        public ConfigFileResult<string> EncodeValueType()
        {
            return "test";
        }
    }

    internal class DecodeThrowingAdapter : IConfigEntryAdapter
    {
        public ConfigFileResult<string> Encode()
        {
            return "test";
        }

        public ConfigFileResult<object> Decode(string content)
        {
            throw new InvalidOperationException("Decode exception");
        }

        public ConfigFileResult<string> EncodeValueType()
        {
            return "test";
        }
    }
}
