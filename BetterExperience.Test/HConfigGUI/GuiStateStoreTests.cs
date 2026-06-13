using BetterExperience.HConfigGUI;
using BetterExperience.HConfigGUI.Bindings;
using Moq;
using Xunit;

namespace BetterExperience.Test.HConfigGUI
{
    public class GuiStateStoreTests
    {
        [Fact]
        public void IsPopupOpenKey_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "IsPopupOpen";

            // Act
            var result = GuiStateStore.IsPopupOpenKey;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SelectedTableIndexKey_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "SelectedTableIndex";

            // Act
            var result = GuiStateStore.SelectedTableIndexKey;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TableButtonWidthKey_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "TableButtonWidth";

            // Act
            var result = GuiStateStore.TableButtonWidthKey;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LeadingBlankWidthKey_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "LeadingBlankWidth";

            // Act
            var result = GuiStateStore.LeadingBlankWidthKey;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RearBlankWidthKey_WhenAccessed_ReturnsExpectedValue()
        {
            // Arrange
            const string expected = "RearBlankWidth";

            // Act
            var result = GuiStateStore.RearBlankWidthKey;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetKey_WhenSuffixProvided_ReturnsConcatenatedKey()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");

            // Act
            var result = store.GetKey(entryMock.Object, "Suffix");

            // Assert
            Assert.Equal("EntryKey+Suffix", result);
        }


        [Fact]
        public void GetKey_WhenSuffixNotProvided_ReturnsKeyWithSeparator()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");

            // Act
            var result = store.GetKey(entryMock.Object);

            // Assert
            Assert.Equal("EntryKey+", result);
        }

        [Fact]
        public void GetText_WhenKeyDoesNotExist_ReturnsAndStoresDefaultValue()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            var result = store.GetText("TextKey", "DefaultValue");
            var storedValue = store.GetText("TextKey", "OtherValue");

            // Assert
            Assert.Equal("DefaultValue", result);
            Assert.Equal("DefaultValue", storedValue);
        }

        [Fact]
        public void GetText_WhenKeyExists_ReturnsStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetText("TextKey", "StoredValue");

            // Act
            var result = store.GetText("TextKey", "DefaultValue");

            // Assert
            Assert.Equal("StoredValue", result);
        }

        [Fact]
        public void SetText_WhenCalled_StoresValueForKey()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            store.SetText("TextKey", "StoredValue");
            var result = store.GetText("TextKey", "DefaultValue");

            // Assert
            Assert.Equal("StoredValue", result);
        }

        [Fact]
        public void DeleteText_WhenKeyExists_RemovesStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetText("TextKey", "StoredValue");

            // Act
            store.DeleteText("TextKey");
            var result = store.GetText("TextKey", "DefaultValue");

            // Assert
            Assert.Equal("DefaultValue", result);
        }

        [Fact]
        public void DeleteText_WhenKeyDoesNotExist_DoesNothing()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            store.DeleteText("MissingKey");
            var result = store.GetText("OtherKey", "DefaultValue");

            // Assert
            Assert.Equal("DefaultValue", result);
        }

        [Fact]
        public void DeleteText_WhenEntryProvided_RemovesAllMatchingKeysAndKeepsOthers()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");
            store.SetText("EntryKey", "ExactValue");
            store.SetText(store.GetKey(entryMock.Object, "SuffixA"), "ValueA");
            store.SetText(store.GetKey(entryMock.Object, "SuffixB"), "ValueB");
            store.SetText("OtherKey", "OtherValue");

            // Act
            store.DeleteText(entryMock.Object);
            var exactResult = store.GetText("EntryKey", "DeletedExact");
            var suffixAResult = store.GetText(store.GetKey(entryMock.Object, "SuffixA"), "DeletedA");
            var suffixBResult = store.GetText(store.GetKey(entryMock.Object, "SuffixB"), "DeletedB");
            var otherResult = store.GetText("OtherKey", "DeletedOther");

            // Assert
            Assert.Equal("DeletedExact", exactResult);
            Assert.Equal("DeletedA", suffixAResult);
            Assert.Equal("DeletedB", suffixBResult);
            Assert.Equal("OtherValue", otherResult);
        }

        [Fact]
        public void GetBool_WhenKeyDoesNotExist_ReturnsAndStoresDefaultValue()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            var result = store.GetBool("BoolKey", true);
            var storedValue = store.GetBool("BoolKey", false);

            // Assert
            Assert.True(result);
            Assert.True(storedValue);
        }
        [Fact]
        public void GetBool_WhenKeyExists_ReturnsStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetBool("BoolKey", false);

            // Act
            var result = store.GetBool("BoolKey", true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SetBool_WhenCalled_StoresValueForKey()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            store.SetBool("BoolKey", true);
            var result = store.GetBool("BoolKey", false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteBool_WhenKeyExists_RemovesStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetBool("BoolKey", true);

            // Act
            store.DeleteBool("BoolKey");
            var result = store.GetBool("BoolKey", false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteBool_WhenKeyDoesNotExist_DoesNothing()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetBool("OtherKey", true);

            // Act
            store.DeleteBool("MissingKey");
            var result = store.GetBool("OtherKey", false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DeleteBool_WhenEntryProvided_RemovesAllMatchingKeysAndKeepsOthers()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");
            store.SetBool("EntryKey", true);
            store.SetBool(store.GetKey(entryMock.Object, "SuffixA"), false);
            store.SetBool(store.GetKey(entryMock.Object, "SuffixB"), true);
            store.SetBool("OtherKey", true);

            // Act
            store.DeleteBool(entryMock.Object);
            var exactResult = store.GetBool("EntryKey", false);
            var suffixAResult = store.GetBool(store.GetKey(entryMock.Object, "SuffixA"), true);
            var suffixBResult = store.GetBool(store.GetKey(entryMock.Object, "SuffixB"), false);
            var otherResult = store.GetBool("OtherKey", false);

            // Assert
            Assert.False(exactResult);
            Assert.True(suffixAResult);
            Assert.False(suffixBResult);
            Assert.True(otherResult);
        }

        [Fact]
        public void GetInt_WhenKeyDoesNotExist_ReturnsAndStoresDefaultValue()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            var result = store.GetInt("IntKey", 42);
            var storedValue = store.GetInt("IntKey", 0);

            // Assert
            Assert.Equal(42, result);
            Assert.Equal(42, storedValue);
        }

        [Fact]
        public void GetInt_WhenKeyExists_ReturnsStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetInt("IntKey", 7);

            // Act
            var result = store.GetInt("IntKey", 42);

            // Assert
            Assert.Equal(7, result);
        }

        [Fact]
        public void SetInt_WhenCalled_StoresValueForKey()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            store.SetInt("IntKey", 99);
            var result = store.GetInt("IntKey", 0);

            // Assert
            Assert.Equal(99, result);
        }

        [Fact]
        public void DeleteInt_WhenKeyExists_RemovesStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetInt("IntKey", 7);

            // Act
            store.DeleteInt("IntKey");
            var result = store.GetInt("IntKey", 42);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void DeleteInt_WhenKeyDoesNotExist_DoesNothing()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetInt("OtherKey", 7);

            // Act
            store.DeleteInt("MissingKey");
            var result = store.GetInt("OtherKey", 42);

            // Assert
            Assert.Equal(7, result);
        }

        [Fact]
        public void DeleteInt_WhenEntryProvided_RemovesAllMatchingKeysAndKeepsOthers()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");
            store.SetInt("EntryKey", 1);
            store.SetInt(store.GetKey(entryMock.Object, "SuffixA"), 2);
            store.SetInt(store.GetKey(entryMock.Object, "SuffixB"), 3);
            store.SetInt("OtherKey", 4);

            // Act
            store.DeleteInt(entryMock.Object);
            var exactResult = store.GetInt("EntryKey", 10);
            var suffixAResult = store.GetInt(store.GetKey(entryMock.Object, "SuffixA"), 20);
            var suffixBResult = store.GetInt(store.GetKey(entryMock.Object, "SuffixB"), 30);
            var otherResult = store.GetInt("OtherKey", 40);

            // Assert
            Assert.Equal(10, exactResult);
            Assert.Equal(20, suffixAResult);
            Assert.Equal(30, suffixBResult);
            Assert.Equal(4, otherResult);
        }

        [Fact]
        public void GetFloat_WhenKeyDoesNotExist_ReturnsAndStoresDefaultValue()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            var result = store.GetFloat("FloatKey", 1.5f);
            var storedValue = store.GetFloat("FloatKey", 9.9f);

            // Assert
            Assert.Equal(1.5f, result);
            Assert.Equal(1.5f, storedValue);
        }

        [Fact]
        public void GetFloat_WhenKeyExists_ReturnsStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetFloat("FloatKey", 2.5f);

            // Act
            var result = store.GetFloat("FloatKey", 1.5f);

            // Assert
            Assert.Equal(2.5f, result);
        }

        [Fact]
        public void SetFloat_WhenCalled_StoresValueForKey()
        {
            // Arrange
            var store = new GuiStateStore();

            // Act
            store.SetFloat("FloatKey", 3.5f);
            var result = store.GetFloat("FloatKey", 1.5f);

            // Assert
            Assert.Equal(3.5f, result);
        }





        [Fact]
        public void DeleteFloat_WhenKeyExists_RemovesStoredValue()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetFloat("FloatKey", 2.5f);

            // Act
            store.DeleteFloat("FloatKey");
            var result = store.GetFloat("FloatKey", 1.5f);

            // Assert
            Assert.Equal(1.5f, result);
        }

        [Fact]
        public void DeleteFloat_WhenKeyDoesNotExist_DoesNothing()
        {
            // Arrange
            var store = new GuiStateStore();
            store.SetFloat("OtherKey", 2.5f);

            // Act
            store.DeleteFloat("MissingKey");
            var result = store.GetFloat("OtherKey", 1.5f);

            // Assert
            Assert.Equal(2.5f, result);
        }

        [Fact]
        public void DeleteFloat_WhenEntryProvided_RemovesAllMatchingKeysAndKeepsOthers()
        {
            // Arrange
            var store = new GuiStateStore();
            var entryMock = new Mock<IEntryBinding>();
            entryMock.SetupGet(x => x.Key).Returns("EntryKey");
            store.SetFloat("EntryKey", 1.1f);
            store.SetFloat(store.GetKey(entryMock.Object, "SuffixA"), 2.2f);
            store.SetFloat(store.GetKey(entryMock.Object, "SuffixB"), 3.3f);
            store.SetFloat("OtherKey", 4.4f);

            // Act
            store.DeleteFloat(entryMock.Object);
            var exactResult = store.GetFloat("EntryKey", 10.1f);
            var suffixAResult = store.GetFloat(store.GetKey(entryMock.Object, "SuffixA"), 20.2f);
            var suffixBResult = store.GetFloat(store.GetKey(entryMock.Object, "SuffixB"), 30.3f);
            var otherResult = store.GetFloat("OtherKey", 40.4f);

            // Assert
            Assert.Equal(10.1f, exactResult);
            Assert.Equal(20.2f, suffixAResult);
            Assert.Equal(30.3f, suffixBResult);
            Assert.Equal(4.4f, otherResult);
        }


    }
}
