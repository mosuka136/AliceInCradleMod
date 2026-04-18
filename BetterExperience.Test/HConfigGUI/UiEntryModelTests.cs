using BetterExperience.HConfigFileSpace;
using BetterExperience.HConfigGUI;
using BetterExperience.HTranslatorSpace;
using Moq;
using System;
using Xunit;

namespace BetterExperience.Test
{
    public class UiEntryModelTests
    {
        private Mock<IConfigEntry> CreateMockEntry()
        {
            var mockEntry = new Mock<IConfigEntry>();
            mockEntry.Setup(e => e.Key).Returns("SetLootDropRatio");
            return mockEntry;
        }

        [Fact]
        public void Name_WhenAccessed_ReturnsEntryName()
        {
            var expectedName = new Translator("中文", "English");
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.Name).Returns(expectedName);
            var model = new UiEntryModel(mockEntry.Object);

            var result = model.Name;

            Assert.Same(expectedName, result);
        }

        [Fact]
        public void Description_WhenAccessed_ReturnsEntryDescription()
        {
            var expectedDescription = new Translator("描述", "Description");
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.Description).Returns(expectedDescription);
            var model = new UiEntryModel(mockEntry.Object);

            var result = model.Description;

            Assert.Same(expectedDescription, result);
        }

        [Fact]
        public void ValueType_WhenAccessed_ReturnsEntryValueType()
        {
            var expectedType = typeof(int);
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.ValueType).Returns(expectedType);
            var model = new UiEntryModel(mockEntry.Object);

            var result = model.ValueType;

            Assert.Same(expectedType, result);
        }

        [Fact]
        public void Value_Get_ReturnsEntryBoxedValue()
        {
            var expectedValue = 42;
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.BoxedValue).Returns(expectedValue);
            var model = new UiEntryModel(mockEntry.Object);

            var result = model.Value;

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Value_SetWithNonNullValue_UpdatesEntryBoxedValue()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            var newValue = 100;

            model.Value = newValue;

            mockEntry.VerifySet(e => e.BoxedValue = newValue, Times.Once);
        }

        [Fact]
        public void Value_SetWithNonNullValue_SetsCacheValueToNull()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            model.CacheValue = "someValue";

            model.Value = 100;

            Assert.Null(model.CacheValue);
        }

        [Fact]
        public void Value_SetWithNonNullValue_SetsCacheValueStringToEmpty()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            model.CacheValueString = "someString";

            model.Value = 100;

            Assert.Equal(string.Empty, model.CacheValueString);
        }

        [Fact]
        public void Value_SetWithNull_DoesNotUpdateEntryBoxedValue()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);

            model.Value = null;

            mockEntry.VerifySet(e => e.BoxedValue = It.IsAny<object>(), Times.Never);
        }

        [Fact]
        public void Value_SetWithNull_DoesNotChangeCacheValue()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            var originalCacheValue = "originalValue";
            model.CacheValue = originalCacheValue;

            model.Value = null;

            Assert.Equal(originalCacheValue, model.CacheValue);
        }

        [Fact]
        public void Value_SetWithNull_DoesNotChangeCacheValueString()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            var originalCacheString = "originalString";
            model.CacheValueString = originalCacheString;

            model.Value = null;

            Assert.Equal(originalCacheString, model.CacheValueString);
        }

        [Fact]
        public void DefaultValue_WhenAccessed_ReturnsEntryBoxedDefaultValue()
        {
            var expectedDefaultValue = 10;
            var mockEntry = CreateMockEntry();
            mockEntry.Setup(e => e.BoxedDefaultValue).Returns(expectedDefaultValue);
            var model = new UiEntryModel(mockEntry.Object);

            var result = model.DefaultValue;

            Assert.Equal(expectedDefaultValue, result);
        }

        [Fact]
        public void Constructor_WhenOnValueChangedBaseRaised_SetsCacheValueToNull()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            model.CacheValue = "someValue";

            mockEntry.Raise(e => e.OnValueChangedBase += null, EventArgs.Empty);

            Assert.Null(model.CacheValue);
        }

        [Fact]
        public void Constructor_WhenOnValueChangedBaseRaised_SetsCacheValueStringToEmpty()
        {
            var mockEntry = CreateMockEntry();
            var model = new UiEntryModel(mockEntry.Object);
            model.CacheValueString = "someString";

            mockEntry.Raise(e => e.OnValueChangedBase += null, EventArgs.Empty);

            Assert.Equal(string.Empty, model.CacheValueString);
        }
    }
}
