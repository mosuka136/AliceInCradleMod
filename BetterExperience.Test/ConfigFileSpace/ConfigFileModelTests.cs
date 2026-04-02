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
}
