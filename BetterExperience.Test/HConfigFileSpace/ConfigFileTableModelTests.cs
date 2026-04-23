using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test
{
    public class ConfigFileTableModelTests
    {
        [Fact]
        public void Constructor_WithValidTableKey_SetsKeyAndDescription()
        {
            // Arrange
            var tableKey = "ValidTable";
            var description = new Translator("描述", "Description");

            // Act
            var model = new ConfigFileTableModel(tableKey, description);

            // Assert
            Assert.Equal(tableKey, model.Key);
            Assert.Equal(description, model.Description);
        }

        [Fact]
        public void Constructor_WithInvalidTableKey_ThrowsArgumentException()
        {
            // Arrange
            var invalidTableKey = "Invalid Key!";
            var description = new Translator("描述", "Description");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ConfigFileTableModel(invalidTableKey, description));
            Assert.Contains("Invalid table name", exception.Message);
            Assert.Equal("tableKey", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullTableKey_ThrowsArgumentException()
        {
            // Arrange
            string nullTableKey = null;
            var description = new Translator("描述", "Description");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ConfigFileTableModel(nullTableKey, description));
            Assert.Contains("Invalid table name", exception.Message);
            Assert.Equal("tableKey", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithEmptyTableKey_ThrowsArgumentException()
        {
            // Arrange
            var emptyTableKey = "";
            var description = new Translator("描述", "Description");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ConfigFileTableModel(emptyTableKey, description));
            Assert.Contains("Invalid table name", exception.Message);
            Assert.Equal("tableKey", exception.ParamName);
        }

        [Fact]
        public void AddEntry_WithNullEntry_ReturnsFailureResult()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            ConfigFileEntryModel nullEntry = null;

            // Act
            var result = model.AddEntry(nullEntry);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.EntryNotFound, result.Errors[0].Code);
            Assert.Contains("Entry cannot be null", result.Errors[0].Message);
        }

        [Fact]
        public void AddEntry_WithDuplicateKey_ReturnsFailureResult()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            var entry1 = new ConfigFileEntryModel { Key = "TestKey", Value = "Value1" };
            var entry2 = new ConfigFileEntryModel { Key = "TestKey", Value = "Value2" };
            model.AddEntry(entry1);

            // Act
            var result = model.AddEntry(entry2);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
            Assert.Contains("Duplicate key", result.Errors[0].Message);
        }

        [Fact]
        public void AddEntry_WithValidEntry_AddsEntryAndReturnsSuccess()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            var entry = new ConfigFileEntryModel { Key = "TestKey", Value = "TestValue" };

            // Act
            var result = model.AddEntry(entry);

            // Assert
            Assert.True(result.Success);
            Assert.True(model.Table.Contains("TestKey"));
            Assert.Equal(entry, model.Table["TestKey"]);
        }

        [Fact]
        public void EncodeName_WithNullName_ReturnsEmptyString()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            model.Name = null;

            // Act
            var result = model.EncodeName();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeName_WithEmptyStringInName_SkipsEmptyString()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            model.Name = new Translator("", "TestName");

            // Act
            var result = model.EncodeName();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("TestName", result.Value);
            Assert.DoesNotContain(", ,", result.Value);
        }

        [Fact]
        public void EncodeName_WithValidName_ReturnsFormattedName()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            model.Name = new Translator("测试名称", "TestName");

            // Act
            var result = model.EncodeName();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("# Name:", result.Value);
            Assert.Contains("测试名称", result.Value);
            Assert.Contains("TestName", result.Value);
        }

        [Fact]
        public void EncodeDescription_WithNullDescription_ReturnsEmptyString()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", null);

            // Act
            var result = model.EncodeDescription();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDescription_WithValidDescription_ReturnsFormattedDescription()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator("测试描述", "Test Description"));

            // Act
            var result = model.EncodeDescription();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("##", result.Value);
            Assert.Contains("测试描述", result.Value);
            Assert.Contains("Test Description", result.Value);
        }

        [Fact]
        public void EncodeDescription_WithEmptyStringInDescription_SkipsEmptyString()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator("", "Description"));

            // Act
            var result = model.EncodeDescription();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Description", result.Value);
        }

        [Fact]
        public void EncodeDescription_WithMultilineDescription_FormatsEachLine()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator("Line1\nLine2\rLine3\r\nLine4", "Description"));

            // Act
            var result = model.EncodeDescription();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("## Line1", result.Value);
            Assert.Contains("## Line2", result.Value);
            Assert.Contains("## Line3", result.Value);
            Assert.Contains("## Line4", result.Value);
        }

        [Fact]
        public void EncodeTable_WithValidTableHeaderFailure_ReturnsFailureResult()
        {
            // Arrange
            var model = new ConfigFileTableModel("ValidTable", new Translator());
            model.Key = null;

            // Act
            var result = model.EncodeTable();

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EncodeTable_WithEntryEncodingFailure_CollectsErrorsButContinues()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            var entry = new ConfigFileEntryModel { Key = "TestKey", Value = "" };
            model.AddEntry(entry);

            // Act
            var result = model.EncodeTable();

            // Assert
            // This tests line 112: result.AddError(entryResult.Errors);
            // The entry with empty Value should cause encoding to fail
            Assert.NotNull(result.Value);
            Assert.Contains("[TestTable]", result.Value);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void EncodeTable_WithNoNameAndNoDescription_EncodesTableHeaderOnly()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());

            // Act
            var result = model.EncodeTable();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("[TestTable]", result.Value);
        }

        [Fact]
        public void EncodeTable_WithNameAndDescription_EncodesAll()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator("描述", "Description"));
            model.Name = new Translator("名称", "Name");

            // Act
            var result = model.EncodeTable();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("# Name:", result.Value);
            Assert.Contains("##", result.Value);
            Assert.Contains("[TestTable]", result.Value);
        }

        [Fact]
        public void EncodeTable_WithEntries_EncodesEntriesWithBlankLines()
        {
            // Arrange
            var model = new ConfigFileTableModel("TestTable", new Translator());
            var entry1 = new ConfigFileEntryModel { Key = "Key1", Value = "Value1" };
            var entry2 = new ConfigFileEntryModel { Key = "Key2", Value = "Value2" };
            model.AddEntry(entry1);
            model.AddEntry(entry2);

            // Act
            var result = model.EncodeTable();

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Key1", result.Value);
            Assert.Contains("Key2", result.Value);
        }

        [Fact]
        public void DecodeTableHeader_WithInvalidTableName_ReturnsFailureResult()
        {
            // Arrange
            var content = new[] { "[Invalid Table!]" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeTableHeader_WithValidTableName_ReturnsSuccess()
        {
            // Arrange
            var content = new[] { "[ValidTable]" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("ValidTable", result.Value.Key);
        }

        [Fact]
        public void DecodeTableHeader_WithEmptyContent_ReturnsEndOfContentError()
        {
            // Arrange
            var content = new string[] { };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.EndOfContent, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeTableHeader_WithOnlyCommentsAndWhitespace_ReturnsEndOfContentError()
        {
            // Arrange
            var content = new[] { "# Comment", "  ", "## Another comment" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.EndOfContent, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeTableHeader_WithInvalidHeaderLine_ReturnsInvalidTableHeaderError()
        {
            // Arrange
            var content = new[] { "NotATableHeader" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidTableHeader, result.Errors[0].Code);
            Assert.Contains("Invalid table header", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeTableHeader_SkipsCommentsAndWhitespace_FindsTableHeader()
        {
            // Arrange
            var content = new[] { "# Comment", "  ", "[TestTable]" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.Equal(3, index);
        }

        [Fact]
        public void DecodeTable_WithValidTableAndEntries_ReturnsTableWithEntries()
        {
            // Arrange
            var content = new[]
            {
                "[TestTable]",
                "Key1 = Value1",
                "Key2 = Value2"
            };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.True(result.Value.Table.Contains("Key1"));
            Assert.True(result.Value.Table.Contains("Key2"));
        }

        [Fact]
        public void DecodeTable_WithInvalidHeader_ReturnsFailureResult()
        {
            // Arrange
            var content = new[] { "NotATableHeader" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void DecodeTable_WithDuplicateEntryKeys_AddsErrorsButContinues()
        {
            // Arrange
            var content = new[]
            {
                "[TestTable]",
                "Key1 = Value1",
                "Key1 = Value2"
            };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.True(result.Value.Table.Contains("Key1"));
            Assert.NotEmpty(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == ConfigFileErrorCode.InvalidKeyName);
        }

        [Fact]
        public void DecodeTable_WithEndOfContentError_BreaksLoop()
        {
            // Arrange
            var content = new[]
            {
                "[TestTable]",
                "Key1 = Value1"
            };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.True(result.Value.Table.Contains("Key1"));
        }

        [Fact]
        public void DecodeTable_WithEmptyTable_ReturnsTableWithNoEntries()
        {
            // Arrange
            var content = new[] { "[EmptyTable]" };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("EmptyTable", result.Value.Key);
            Assert.Empty(result.Value.Table);
        }

        [Fact]
        public void DecodeTable_WithMultipleTables_StopsAtNextTableHeader()
        {
            // Arrange
            var content = new[]
            {
                "[Table1]",
                "Key1 = Value1",
                "[Table2]",
                "Key2 = Value2"
            };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Table1", result.Value.Key);
            Assert.True(result.Value.Table.Contains("Key1"));
            Assert.False(result.Value.Table.Contains("Key2"));
            Assert.Equal(2, index);
        }

        [Fact]
        public void DecodeTable_WithInvalidEntries_CollectsErrorsAndContinues()
        {
            // Arrange
            var content = new[]
            {
                "[TestTable]",
                "InvalidEntry",
                "Key1 = Value1"
            };
            var index = 0;

            // Act
            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.NotEmpty(result.Errors);
        }
    }
}
