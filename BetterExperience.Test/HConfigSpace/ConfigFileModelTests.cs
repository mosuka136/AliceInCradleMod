using BetterExperience.HConfigSpace;
using System.Collections;

namespace BetterExperience.Test
{
    public class ConfigFileModelTests
    {
        [Fact]
        public void Encode_GenericValue_ReturnsEncodedResult()
        {
            // Arrange
            var value = 123;

            // Act
            var result = ConfigFileModel.Encode(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("123", result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Encode_NullType_ReturnsFailure()
        {
            // Arrange
            var value = new object();

            // Act
            var result = ConfigFileModel.Encode(value, null);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Type cannot be null", error.Message);
        }

        [Fact]
        public void Encode_AdapterReturnsFailure_ReturnsFailure()
        {
            // Arrange
            var adapter = new FailingEncodeAdapter();

            // Act
            var result = ConfigFileModel.Encode(adapter, typeof(FailingEncodeAdapter));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("encode failed", error.Message);
        }

        [Fact]
        public void Encode_AdapterThrows_ReturnsFailure()
        {
            // Arrange
            var adapter = new ThrowingEncodeAdapter();

            // Act
            var result = ConfigFileModel.Encode(adapter, typeof(ThrowingEncodeAdapter));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Contains("Failed to encode using IConfigEntryAdapter", error.Message);
            Assert.Contains("encode boom", error.Message);
        }

        [Theory]
        [InlineData((sbyte)-12, "-12")]
        [InlineData((short)-32000, "-32000")]
        [InlineData((byte)200, "200")]
        [InlineData((ushort)65000, "65000")]
        [InlineData((uint)4000000000, "4000000000")]
        [InlineData((ulong)18446744073709551615, "18446744073709551615")]
        [InlineData(12.5d, "12.5")]
        public void Encode_SupportedPrimitiveTypes_ReturnsInvariantString(object value, string expected)
        {
            // Arrange
            var type = value.GetType();

            // Act
            var result = ConfigFileModel.Encode(value, type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expected, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Encode_UnsupportedEnumerableType_ReturnsFailure()
        {
            // Arrange
            var value = new UnsupportedEnumerable();

            // Act
            var result = ConfigFileModel.Encode(value, typeof(UnsupportedEnumerable));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Unsupported collection type", error.Message);
        }

        [Fact]
        public void Encode_CollectionContainsUnsupportedElement_ReturnsFailure()
        {
            // Arrange
            var value = new List<UnsupportedValueType> { new UnsupportedValueType() };

            // Act
            var result = ConfigFileModel.Encode(value, typeof(List<UnsupportedValueType>));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Unsupported type", error.Message);
        }

        [Fact]
        public void Encode_ListOfInt_ReturnsJoinedCollectionString()
        {
            // Arrange
            var value = new List<int> { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.Encode(value, typeof(List<int>));

            // Assert
            Assert.True(result.Success);
            Assert.Equal("[1,2,3]", result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Encode_UnsupportedType_ReturnsFailure()
        {
            // Arrange
            var value = new UnsupportedValueType();

            // Act
            var result = ConfigFileModel.Encode(value, typeof(UnsupportedValueType));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Unsupported type", error.Message);
        }

        [Fact]
        public void Decode_GenericDecodeFails_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode<int>("not-an-int");

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid int value: not-an-int", error.Message);
        }

        [Fact]
        public void Decode_GenericResultCannotBeCast_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode<WrongTypeDecodeAdapter>("anything");

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal($"Decoded value cannot be converted to {typeof(WrongTypeDecodeAdapter).FullName}", error.Message);
        }

        [Fact]
        public void Decode_NullType_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("value", null);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Type cannot be null", error.Message);
        }

        [Fact]
        public void Decode_AdapterReturnsFailure_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("value", typeof(FailingDecodeAdapter));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("decode failed", error.Message);
        }

        [Fact]
        public void Decode_AdapterThrows_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("value", typeof(ThrowingDecodeAdapter));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Contains("Failed to decode using IConfigEntryAdapter", error.Message);
            Assert.Contains("decode boom", error.Message);
        }

        [Theory]
        [InlineData(typeof(sbyte), "-12", "System.SByte", "-12")]
        [InlineData(typeof(short), "-32000", "System.Int16", "-32000")]
        [InlineData(typeof(byte), "200", "System.Byte", "200")]
        [InlineData(typeof(ushort), "65000", "System.UInt16", "65000")]
        [InlineData(typeof(uint), "4000000000", "System.UInt32", "4000000000")]
        [InlineData(typeof(ulong), "18446744073709551615", "System.UInt64", "18446744073709551615")]
        [InlineData(typeof(double), "12.5", "System.Double", "12.5")]
        public void Decode_SupportedPrimitiveTypes_ReturnsParsedValue(Type type, string value, string expectedTypeName, string expectedValue)
        {
            // Act
            var result = ConfigFileModel.Decode(value, type);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedTypeName, result.Value.GetType().FullName);
            Assert.Equal(expectedValue, result.Value.ToString());
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(typeof(sbyte), "abc", "Invalid sbyte value: abc")]
        [InlineData(typeof(short), "abc", "Invalid short value: abc")]
        [InlineData(typeof(long), "abc", "Invalid long value: abc")]
        [InlineData(typeof(byte), "abc", "Invalid byte value: abc")]
        [InlineData(typeof(ushort), "abc", "Invalid ushort value: abc")]
        [InlineData(typeof(uint), "abc", "Invalid uint value: abc")]
        [InlineData(typeof(ulong), "abc", "Invalid ulong value: abc")]
        [InlineData(typeof(float), "abc", "Invalid float value: abc")]
        [InlineData(typeof(double), "abc", "Invalid double value: abc")]
        [InlineData(typeof(bool), "abc", "Invalid bool value: abc")]
        public void Decode_InvalidPrimitiveText_ReturnsFailure(Type type, string value, string expectedMessage)
        {
            // Act
            var result = ConfigFileModel.Decode(value, type);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal(expectedMessage, error.Message);
        }

        [Fact]
        public void Decode_InvalidEnumValue_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("Missing", typeof(SampleEnum));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Contains("Invalid enum value: Missing.", error.Message);
        }

        [Fact]
        public void Decode_UnsupportedEnumerableType_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("[1,2]", typeof(UnsupportedEnumerable));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Unsupported collection type", error.Message);
        }

        [Fact]
        public void Decode_InvalidCollectionFormat_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("1,2", typeof(List<int>));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Collection string must start with '[' and end with ']'", error.Message);
        }

        [Fact]
        public void Decode_CollectionContainsInvalidElement_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("[1,abc]", typeof(List<int>));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid int value: abc", error.Message);
        }

        [Fact]
        public void Decode_IntArray_ReturnsArray()
        {
            // Act
            var result = ConfigFileModel.Decode("[1,2,3]", typeof(int[]));

            // Assert
            Assert.True(result.Success);
            var values = Assert.IsType<int[]>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, values);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Decode_ListOfInt_ReturnsList()
        {
            // Act
            var result = ConfigFileModel.Decode("[1,2,3]", typeof(List<int>));

            // Assert
            Assert.True(result.Success);
            var values = Assert.IsType<List<int>>(result.Value);
            Assert.Equal(new List<int> { 1, 2, 3 }, values);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Decode_UnsupportedType_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.Decode("value", typeof(UnsupportedValueType));

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Decoding not implemented", error.Message);
        }

        [Fact]
        public void GetCollectionElementType_GenericEnumerable_ReturnsGenericArgument()
        {
            // Act
            var result = ConfigFileModel.GetCollectionElementType(typeof(IEnumerable<int>));

            // Assert
            Assert.Equal(typeof(int), result);
        }


        [Fact]
        public void EncodeString_TrimEscapeAndQuoteEnabled_ReturnsTrimmedEscapedQuotedValue()
        {
            // Arrange
            var value = "  a\\\"\n\r\tb  ";

            // Act
            var result = ConfigFileModel.EncodeString(value, quote: true, trim: true, escape: true);

            // Assert
            Assert.Equal("\"a\\\\\\\"\\n\\r\\tb\"", result);
        }

        [Fact]
        public void EncodeString_WithoutQuoteOrEscape_PreservesWhitespaceAndSpecialCharacters()
        {
            // Arrange
            var value = " a\\\"\n\r\t ";

            // Act
            var result = ConfigFileModel.EncodeString(value, quote: false, trim: false, escape: false);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void EncodeString_TrimEnabledWithoutEscapeOrQuote_ReturnsTrimmedRawValue()
        {
            // Arrange
            var value = "  a\"b  ";

            // Act
            var result = ConfigFileModel.EncodeString(value, quote: false, trim: true, escape: false);

            // Assert
            Assert.Equal("a\"b", result);
        }

        [Fact]
        public void DecodeString_ValueIsWhitespace_ReturnsEmptyString()
        {
            // Arrange
            var value = " \t \r\n ";

            // Act
            var result = ConfigFileModel.DecodeString(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void DecodeString_QuotedValueContainsEscapes_ReturnsUnescapedValue()
        {
            // Arrange
            var value = "  \"line1\\nline2\\t\"  ";

            // Act
            var result = ConfigFileModel.DecodeString(value, quote: true, trim: true, escape: true);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("line1\nline2\t", result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void DecodeString_QuoteRequiredButMissing_ReturnsFailure()
        {
            // Arrange
            var value = "value";

            // Act
            var result = ConfigFileModel.DecodeString(value, quote: true, trim: true, escape: false);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("String must start and end with a quote", error.Message);
        }

        [Fact]
        public void DecodeString_InvalidEscapeSequence_ReturnsFailure()
        {
            // Arrange
            var value = "\"bad\\x\"";

            // Act
            var result = ConfigFileModel.DecodeString(value, quote: true, trim: true, escape: true);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid escape sequence: \\x", error.Message);
        }

        [Fact]
        public void SplitCollectionString_ValueIsWhitespace_ReturnsFailure()
        {
            // Arrange
            var value = "   ";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Value cannot be null or whitespace", error.Message);
        }

        [Fact]
        public void SplitCollectionString_MissingOuterBrackets_ReturnsFailure()
        {
            // Arrange
            var value = "a,b";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Collection string must start with '[' and end with ']'", error.Message);
        }

        [Fact]
        public void SplitCollectionString_EmptyCollection_ReturnsEmptyArray()
        {
            // Arrange
            var value = " [   ] ";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void SplitCollectionString_ContainsQuotesEscapesAndNestedCollection_ReturnsTopLevelElements()
        {
            // Arrange
            var value = "[\"a\\\\n,b\", [1,2], plain]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(new[] { "\"a\\\\n,b\"", "[1,2]", "plain" }, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void SplitCollectionString_InvalidEscapeAtEndOfQuotedString_ReturnsFailure()
        {
            // Arrange
            var value = "[\"abc\\]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid escape sequence at end of string", error.Message);
        }

        [Fact]
        public void SplitCollectionString_InvalidEscapeSequenceInQuotedString_ReturnsFailure()
        {
            // Arrange
            var value = "[\"abc\\x\"]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid escape sequence: \\x", error.Message);
        }

        [Fact]
        public void SplitCollectionString_UnexpectedClosingBracket_ReturnsFailure()
        {
            // Arrange
            var value = "[a]]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Unexpected closing bracket in collection", error.Message);
        }

        [Fact]
        public void SplitCollectionString_ContainsEmptyElementBetweenCommas_ReturnsFailure()
        {
            // Arrange
            var value = "[a,,b]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Collection contains empty element", error.Message);
        }

        [Fact]
        public void SplitCollectionString_ContainsTrailingComma_ReturnsFailure()
        {
            // Arrange
            var value = "[a,]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Collection contains empty element", error.Message);
        }

        [Fact]
        public void SplitCollectionString_UnclosedQuotedString_ReturnsFailure()
        {
            // Arrange
            var value = "[\"abc]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Unclosed quoted string in collection", error.Message);
        }

        [Fact]
        public void SplitCollectionString_UnbalancedNestedCollectionBrackets_ReturnsFailure()
        {
            // Arrange
            var value = "[[1,2]";

            // Act
            var result = ConfigFileModel.SplitCollectionString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Unbalanced nested collection brackets", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_TypeIsNull_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object>();

            // Act
            var result = ConfigFileModel.CreateCollectionResult(null, typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Type cannot be null", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_ElementTypeIsNull_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object>();

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(List<int>), null, elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Element type cannot be null", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_ElementsIsNull_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(List<int>), typeof(int), null);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Elements cannot be null", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_ElementsContainInvalidType_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object> { 1, "two" };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(List<int>), typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Element at index 1 is not assignable to System.Int32. Actual type: System.String", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_ArrayCreationFails_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object>();

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(List<int>), typeof(void), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to create array for element type System.Void.", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_TargetTypeIsArray_ReturnsTypedArray()
        {
            // Arrange
            var elements = new List<object> { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(int[]), typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var array = Assert.IsType<int[]>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, array);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateCollectionResult_TargetTypeAssignableFromHelperList_ReturnsList()
        {
            // Arrange
            var elements = new List<object> { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(IList<int>), typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var list = Assert.IsType<List<int>>(result.Value);
            Assert.Equal(new List<int> { 1, 2, 3 }, list);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateCollectionResult_TargetTypeHasCompatibleConstructor_ReturnsConstructedCollection()
        {
            // Arrange
            var elements = new List<object> { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(ConstructorListCollection), typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var collection = Assert.IsType<ConstructorListCollection>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, collection.Items);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateCollectionResult_CompatibleConstructorThrows_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object> { 1, 2 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(ThrowingConstructorCollection), typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Failed to construct collection type BetterExperience.Test.ConfigFileModelTests+ThrowingConstructorCollection. Error: ctor boom", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_TargetTypeSupportsAddMethod_ReturnsPopulatedCollection()
        {
            // Arrange
            var elements = new List<object> { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(AddOnlyCollection), typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var collection = Assert.IsType<AddOnlyCollection>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, collection.Items);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateCollectionResult_AddMethodThrows_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object> { 1, 2 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(ThrowingAddCollection), typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Failed to add element at index 1 to collection type BetterExperience.Test.ConfigFileModelTests+ThrowingAddCollection. Error: add boom", error.Message);
        }

        [Fact]
        public void CreateCollectionResult_TargetTypeUnsupported_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object> { 1, 2 };

            // Act
            var result = ConfigFileModel.CreateCollectionResult(typeof(UnsupportedCustomCollection), typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.UnsupportedType, error.Code);
            Assert.Equal("Unsupported collection type: BetterExperience.Test.ConfigFileModelTests+UnsupportedCustomCollection", error.Message);
        }

        [Fact]
        public void ValidateCollectionElements_AllElementsValid_ReturnsValidatedArray()
        {
            // Arrange
            var elements = new List<object> { 1, null, 3 };

            // Act
            var result = ConfigFileModel.ValidateCollectionElements(typeof(int?), elements);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Value.Length);
            Assert.Equal(1, result.Value[0]);
            Assert.Null(result.Value[1]);
            Assert.Equal(3, result.Value[2]);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCollectionElements_EncounterInvalidElement_ReturnsFailure()
        {
            // Arrange
            var elements = new List<object> { 1, "two" };

            // Act
            var result = ConfigFileModel.ValidateCollectionElements(typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Element at index 1 is not assignable to System.Int32. Actual type: System.String", error.Message);
        }

        [Fact]
        public void ValidateCollectionElement_NullReferenceType_ReturnsSuccessWithNull()
        {
            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(string), null, 0);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCollectionElement_NullNullableValueType_ReturnsSuccessWithNull()
        {
            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int?), null, 2);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCollectionElement_NullNonNullableValueType_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int), null, 3);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Element at index 3 cannot be null for value type System.Int32", error.Message);
        }

        [Fact]
        public void ValidateCollectionElement_AssignableInstance_ReturnsElement()
        {
            // Arrange
            object element = "value";

            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(object), element, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Same(element, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCollectionElement_NullableUnderlyingTypeMatches_ReturnsElement()
        {
            // Arrange
            object element = 5;

            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int?), element, 4);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCollectionElement_IncompatibleType_ReturnsFailure()
        {
            // Act
            var result = ConfigFileModel.ValidateCollectionElement(typeof(int), "two", 1);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Element at index 1 is not assignable to System.Int32. Actual type: System.String", error.Message);
        }

        [Fact]
        public void CreateTypedArray_CompatibleElements_ReturnsTypedArray()
        {
            // Arrange
            var elements = new object[] { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateTypedArray(typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var array = Assert.IsType<int[]>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, array);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateTypedArray_IncompatibleElement_ReturnsFailure()
        {
            // Arrange
            var elements = new object[] { 1, "two" };

            // Act
            var result = ConfigFileModel.CreateTypedArray(typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to create array for element type System.Int32. Error:", error.Message);
        }

        [Fact]
        public void CreateTypedList_CompatibleElements_ReturnsTypedList()
        {
            // Arrange
            var elements = new object[] { 1, 2, 3 };

            // Act
            var result = ConfigFileModel.CreateTypedList(typeof(int), elements);

            // Assert
            Assert.True(result.Success);
            var list = Assert.IsType<List<int>>(result.Value);
            Assert.Equal(new List<int> { 1, 2, 3 }, list);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CreateTypedList_IncompatibleElement_ReturnsFailure()
        {
            // Arrange
            var elements = new object[] { 1, "two" };

            // Act
            var result = ConfigFileModel.CreateTypedList(typeof(int), elements);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to populate helper list for element type System.Int32. Error:", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_ListConstructorMatches_ReturnsConstructedInstance()
        {
            // Arrange
            IList list = new List<int> { 1, 2, 3 };
            Array array = new[] { 1, 2, 3 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ConstructorListCollection), list, array, out var result);

            // Assert
            Assert.True(handled);
            Assert.True(result.Success);
            var collection = Assert.IsType<ConstructorListCollection>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, collection.Items);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_ArrayConstructorMatches_ReturnsConstructedInstance()
        {
            // Arrange
            IList list = new List<int> { 1, 2, 3 };
            Array array = new[] { 1, 2, 3 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ConstructorArrayCollection), list, array, out var result);

            // Assert
            Assert.True(handled);
            Assert.True(result.Success);
            var collection = Assert.IsType<ConstructorArrayCollection>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, collection.Items);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_NoCompatibleConstructor_ReturnsFalse()
        {
            // Arrange
            IList list = new List<int> { 1, 2, 3 };
            Array array = new[] { 1, 2, 3 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ConstructorStringCollection), list, array, out var result);

            // Assert
            Assert.False(handled);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_ConstructorThrows_ReturnsFailureAndTrue()
        {
            // Arrange
            IList list = new List<int> { 1, 2 };
            Array array = new[] { 1, 2 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(ThrowingConstructorCollection), list, array, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Failed to construct collection type BetterExperience.Test.ConfigFileModelTests+ThrowingConstructorCollection. Error: ctor boom", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromConstructor_AbstractType_ReturnsFailureAndTrue()
        {
            // Arrange
            IList list = new List<int> { 1 };
            Array array = new[] { 1 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromConstructor(typeof(AbstractConstructorCollection), list, array, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to construct collection type BetterExperience.Test.ConfigFileModelTests+AbstractConstructorCollection. Error:", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_NoDefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var elements = new object[] { 1, 2 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(ParameterizedCtorAddCollection), typeof(int), elements, out var result);

            // Assert
            Assert.False(handled);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_NoCompatibleAddMethod_ReturnsFalse()
        {
            // Arrange
            var elements = new object[] { 1, 2 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(WrongAddTypeCollection), typeof(int), elements, out var result);

            // Assert
            Assert.False(handled);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_ConstructorThrows_ReturnsFailureAndTrue()
        {
            // Arrange
            var elements = new object[] { 1 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(ThrowingParameterlessAddCollection), typeof(int), elements, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Failed to create collection type BetterExperience.Test.ConfigFileModelTests+ThrowingParameterlessAddCollection. Error: create boom", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_AbstractType_ReturnsFailureAndTrue()
        {
            // Arrange
            var elements = new object[] { 1 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(AbstractAddCollection), typeof(int), elements, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to create collection type BetterExperience.Test.ConfigFileModelTests+AbstractAddCollection. Error:", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_CompatibleAddMethod_ReturnsPopulatedInstance()
        {
            // Arrange
            var elements = new object[] { 1, 2, 3 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(AddOnlyCollection), typeof(int), elements, out var result);

            // Assert
            Assert.True(handled);
            Assert.True(result.Success);
            var collection = Assert.IsType<AddOnlyCollection>(result.Value);
            Assert.Equal(new[] { 1, 2, 3 }, collection.Items);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_IncompatibleElementDuringAdd_ReturnsFailureAndTrue()
        {
            // Arrange
            var elements = new object[] { 1, "two" };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(AddOnlyCollection), typeof(int), elements, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.StartsWith("Failed to add element at index 1 to collection type BetterExperience.Test.ConfigFileModelTests+AddOnlyCollection. Error:", error.Message);
        }

        [Fact]
        public void TryCreateCollectionFromAddMethod_AddMethodThrows_ReturnsFailureAndTrue()
        {
            // Arrange
            var elements = new object[] { 1, 2 };

            // Act
            var handled = ConfigFileModel.TryCreateCollectionFromAddMethod(typeof(ThrowingAddCollection), typeof(int), elements, out var result);

            // Assert
            Assert.True(handled);
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Failed to add element at index 1 to collection type BetterExperience.Test.ConfigFileModelTests+ThrowingAddCollection. Error: add boom", error.Message);
        }

        [Fact]
        public void FindAddMethod_ParameterTypeMatchesExactly_ReturnsMethod()
        {
            // Act
            var method = ConfigFileModel.FindAddMethod(typeof(AddOnlyCollection), typeof(int));

            // Assert
            Assert.NotNull(method);
            Assert.Equal("Add", method.Name);
            var parameter = Assert.Single(method.GetParameters());
            Assert.Equal(typeof(int), parameter.ParameterType);
        }

        [Fact]
        public void FindAddMethod_ParameterTypeAssignableFromElementType_ReturnsMethod()
        {
            // Act
            var method = ConfigFileModel.FindAddMethod(typeof(ObjectAddCollection), typeof(string));

            // Assert
            Assert.NotNull(method);
            Assert.Equal("Add", method.Name);
            var parameter = Assert.Single(method.GetParameters());
            Assert.Equal(typeof(object), parameter.ParameterType);
        }

        [Fact]
        public void FindAddMethod_ElementTypeIsNullableAndUnderlyingTypeMatches_ReturnsMethod()
        {
            // Act
            var method = ConfigFileModel.FindAddMethod(typeof(NullableUnderlyingAddCollection), typeof(int?));

            // Assert
            Assert.NotNull(method);
            Assert.Equal("Add", method.Name);
            var parameter = Assert.Single(method.GetParameters());
            Assert.Equal(typeof(int), parameter.ParameterType);
        }

        [Fact]
        public void FindAddMethod_NoCompatibleSingleParameterAddMethod_ReturnsNull()
        {
            // Act
            var method = ConfigFileModel.FindAddMethod(typeof(NoSingleParameterAddCollection), typeof(int));

            // Assert
            Assert.Null(method);
        }

        [Fact]
        public void UnescapeString_ValueEndsWithEscapeCharacter_ReturnsFailure()
        {
            // Arrange
            var value = "abc\\";

            // Act
            var result = ConfigFileModel.UnescapeString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid escape sequence at end of string", error.Message);
        }

        [Fact]
        public void UnescapeString_ValueContainsSupportedEscapeSequences_ReturnsUnescapedText()
        {
            // Arrange
            var value = string.Concat("prefix", "\\\\", "\\\"", "\\n", "\\r", "\\t", "suffix");

            // Act
            var result = ConfigFileModel.UnescapeString(value);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("prefix\\\"\n\r\tsuffix", result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void UnescapeString_ValueContainsUnsupportedEscapeSequence_ReturnsFailure()
        {
            // Arrange
            var value = "bad\\x";

            // Act
            var result = ConfigFileModel.UnescapeString(value);

            // Assert
            Assert.False(result.Success);
            var error = Assert.Single(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, error.Code);
            Assert.Equal("Invalid escape sequence: \\x", error.Message);
        }






        private enum SampleEnum
        {
            First,
            Second
        }

        private class UnsupportedValueType
        {
        }

        private class UnsupportedEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return 1;
            }
        }

        private class ConstructorListCollection
        {
            public ConstructorListCollection(List<int> items)
            {
                Items = items.ToArray();
            }

            public int[] Items { get; }
        }

        private class ConstructorArrayCollection
        {
            public ConstructorArrayCollection(int[] items)
            {
                Items = items;
            }

            public int[] Items { get; }
        }

        private class ConstructorStringCollection
        {
            public ConstructorStringCollection(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        private abstract class AbstractConstructorCollection
        {
            public AbstractConstructorCollection(List<int> items)
            {
                Items = items.ToArray();
            }

            public int[] Items { get; }
        }

        private class ObjectAddCollection
        {
            public void Add(object value)
            {
            }
        }

        private class NullableUnderlyingAddCollection
        {
            public void Add(int value)
            {
            }
        }

        private class NoSingleParameterAddCollection
        {
            public void Add()
            {
            }

            public void Add(int first, int second)
            {
            }
        }



        private class ThrowingConstructorCollection
        {
            public ThrowingConstructorCollection(List<int> items)
            {
                throw new InvalidOperationException("ctor boom");
            }
        }

        private class AddOnlyCollection
        {
            private readonly List<int> items = new List<int>();

            public int[] Items => items.ToArray();

            public void Add(int value)
            {
                items.Add(value);
            }
        }

        private class ThrowingAddCollection
        {
            private readonly List<int> items = new List<int>();

            public void Add(int value)
            {
                if (items.Count == 1)
                    throw new InvalidOperationException("add boom");

                items.Add(value);
            }
        }

        private class ParameterizedCtorAddCollection
        {
            public ParameterizedCtorAddCollection(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public void Add(int value)
            {
            }
        }

        private class WrongAddTypeCollection
        {
            public void Add(string value)
            {
            }
        }

        private class ThrowingParameterlessAddCollection
        {
            public ThrowingParameterlessAddCollection()
            {
                throw new InvalidOperationException("create boom");
            }

            public void Add(int value)
            {
            }
        }

        private abstract class AbstractAddCollection
        {
            public AbstractAddCollection()
            {
            }

            public void Add(int value)
            {
            }
        }


        private class UnsupportedCustomCollection
        {
        }

        private class FailingEncodeAdapter : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "encode failed"));
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return ConfigFileResult<object>.Ok(content);
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Ok("FailingEncodeAdapter");
            }
        }

        private class ThrowingEncodeAdapter : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                throw new InvalidOperationException("encode boom");
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return ConfigFileResult<object>.Ok(content);
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Ok("ThrowingEncodeAdapter");
            }
        }

        private class FailingDecodeAdapter : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return ConfigFileResult<string>.Ok("value");
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "decode failed"));
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Ok("FailingDecodeAdapter");
            }
        }

        private class ThrowingDecodeAdapter : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return ConfigFileResult<string>.Ok("value");
            }

            public ConfigFileResult<object> Decode(string content)
            {
                throw new InvalidOperationException("decode boom");
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Ok("ThrowingDecodeAdapter");
            }
        }

        private class WrongTypeDecodeAdapter : IConfigEntryAdapter
        {
            public ConfigFileResult<string> Encode()
            {
                return ConfigFileResult<string>.Ok("value");
            }

            public ConfigFileResult<object> Decode(string content)
            {
                return ConfigFileResult<object>.Ok("text");
            }

            public ConfigFileResult<string> EncodeValueType()
            {
                return ConfigFileResult<string>.Ok("WrongTypeDecodeAdapter");
            }
        }
    }
}
