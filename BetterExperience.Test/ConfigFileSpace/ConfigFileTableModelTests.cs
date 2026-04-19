using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.ConfigFileSpace
{
    public class ConfigFileTableModelTests
    {
        [Theory]
        [InlineData("Table_1", true)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("table-name", false)]
        [InlineData("table.name", false)]
        public void IsValidTableNameShouldReturnExpectedResult(string tableName, bool expected)
        {
            var result = ConfigFileTableModel.IsValidTableName(tableName);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AddTableWhenDuplicateNameShouldFail()
        {
            var model = new ConfigFileSheetModel();
            var table1 = ConfigFileTableModel.Create("General", new Translator(english: "First")).Value;
            var table2 = ConfigFileTableModel.Create("General", new Translator(english: "Second")).Value;

            var firstResult = model.AddTable(table1);
            var secondResult = model.AddTable(table2);

            Assert.True(firstResult.Success);
            Assert.False(secondResult.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, secondResult.Errors[0].Code);
        }

        [Fact]
        public void EncodeTableWhenDescriptionAndEntriesExistShouldIncludeHeaderAndBody()
        {
            var table = new ConfigFileTableModel("General", new Translator(english: "Main settings"));
            table.AddEntry(new ConfigFileEntryModel { Key = "port", Value = "8080" });

            var result = table.EncodeTable();

            Assert.True(result.Success);
            var lines = result.Value.Replace("\r\n", "\n").Split('\n');
            Assert.Equal("## Main settings", lines[0]);
            Assert.Equal("[General]", lines[1]);
            Assert.Equal("", lines[2]);
            Assert.Equal("port = 8080", lines[3]);
        }

        [Fact]
        public void DecodeTableHeaderWhenDescriptionExistsShouldIgnoreDescriptionComments()
        {
            var content = new[] { "## line 1", "## line 2", "[General]" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("General", result.Value.Key);
            Assert.Equal(string.Empty, result.Value.Description);
            Assert.Equal(3, index);
        }

        [Fact]
        public void DecodeTableShouldStopAtNextTableHeader()
        {
            var content = new[] { "[General]", "port=8080", "[Advanced]", "mode=1" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("General", result.Value.Key);
            Assert.Single(result.Value.Table);
            var entry = (ConfigFileEntryModel)result.Value.Table[0];
            Assert.Equal("port", entry.Key);
            Assert.Equal(2, index);
        }

        [Fact]
        public void DecodeTablesWhenMultipleTablesExistShouldParseAllTables()
        {
            var content = new[] { "[General]", "port=8080", "[Advanced]", "mode=1" };
            int index = 0;

            var result = ConfigFileSheetModel.DecodeSheet(content, ref index);

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Sheet.Count);
            Assert.Equal(4, index);

            var general = (ConfigFileTableModel)result.Value.Sheet["General"];
            var advanced = (ConfigFileTableModel)result.Value.Sheet["Advanced"];
            Assert.Equal("8080", ((ConfigFileEntryModel)general.Table[0]).Value);
            Assert.Equal("1", ((ConfigFileEntryModel)advanced.Table[0]).Value);
        }

        [Fact]
        public void Constructor_WithInvalidTableName_ShouldThrowArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new ConfigFileTableModel("invalid-name", new Translator("测试", "test")));

            Assert.Equal("tableKey", exception.ParamName);
            Assert.Contains("Invalid table name", exception.Message);
        }

        [Fact]
        public void Constructor_WithValidTableName_ShouldSetKeyAndDescription()
        {
            var description = new Translator("测试", "test");

            var table = new ConfigFileTableModel("Valid_Table_1", description);

            Assert.Equal("Valid_Table_1", table.Key);
            Assert.Equal(description, table.Description);
        }

        [Fact]
        public void AddEntry_WhenEntryIsNull_ShouldReturnFailure()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));

            var result = table.AddEntry(null);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.EntryNotFound, result.Errors[0].Code);
            Assert.Contains("Entry cannot be null", result.Errors[0].Message);
        }

        [Fact]
        public void AddEntry_WhenDuplicateKey_ShouldReturnFailure()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));
            var entry1 = new ConfigFileEntryModel { Key = "port", Value = "8080" };
            var entry2 = new ConfigFileEntryModel { Key = "port", Value = "9090" };
            table.AddEntry(entry1);

            var result = table.AddEntry(entry2);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
            Assert.Contains("Duplicate key: port", result.Errors[0].Message);
        }

        [Fact]
        public void AddEntry_WhenValidEntry_ShouldAddSuccessfully()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));
            var entry = new ConfigFileEntryModel { Key = "port", Value = "8080" };

            var result = table.AddEntry(entry);

            Assert.True(result.Success);
            Assert.Single(table.Table);
            Assert.True(table.Table.Contains("port"));
        }

        [Fact]
        public void EncodeDescription_WhenDescriptionIsNull_ShouldReturnEmptyString()
        {
            var table = new ConfigFileTableModel("TestTable", null);

            var result = table.EncodeDescription();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Value);
        }

        [Fact]
        public void EncodeDescription_WhenDescriptionHasMultipleLines_ShouldPrefixWithDoubleHash()
        {
            var description = new Translator("第一行\r\n第二行", "Line 1\nLine 2");
            var table = new ConfigFileTableModel("TestTable", description);

            var result = table.EncodeDescription();

            Assert.True(result.Success);
            Assert.Contains("## 第一行", result.Value);
            Assert.Contains("## 第二行", result.Value);
            Assert.Contains("## Line 1", result.Value);
            Assert.Contains("## Line 2", result.Value);
        }

        [Fact]
        public void EncodeTableHeader_WhenKeyIsInvalid_ShouldReturnFailure()
        {
            var table = new ConfigFileTableModel("ValidTable", new Translator("测试", "test"));
            table.Key = "invalid-key";

            var result = table.EncodeTableHeader();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, result.Errors[0].Code);
            Assert.Contains("Invalid table name", result.Errors[0].Message);
        }

        [Fact]
        public void EncodeTableHeader_WhenKeyIsValid_ShouldReturnFormattedHeader()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));

            var result = table.EncodeTableHeader();

            Assert.True(result.Success);
            Assert.Equal("[TestTable]", result.Value);
        }

        [Fact]
        public void EncodeTable_WhenEntryEncodeFails_ShouldIncludeErrors()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));
            var entry = new ConfigFileEntryModel { Key = "port", Value = "" };
            table.AddEntry(entry);

            var result = table.EncodeTable();

            Assert.True(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidValue, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeTable_WhenTableHeaderIsInvalid_ShouldReturnFailure()
        {
            var table = new ConfigFileTableModel("ValidTable", new Translator("测试", "test"));
            table.Key = "invalid-key";

            var result = table.EncodeTable();

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, result.Errors[0].Code);
        }

        [Fact]
        public void EncodeTable_WhenEverythingIsValid_ShouldReturnFormattedTable()
        {
            var table = new ConfigFileTableModel("TestTable", new Translator("测试", "test"));
            table.AddEntry(new ConfigFileEntryModel { Key = "port", Value = "8080" });

            var result = table.EncodeTable();

            Assert.True(result.Success);
            Assert.Contains("[TestTable]", result.Value);
            Assert.Contains("port = 8080", result.Value);
        }

        [Fact]
        public void Create_WithInvalidTableName_ShouldReturnFailure()
        {
            var result = ConfigFileTableModel.Create("invalid-name", new Translator("测试", "test"));

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, result.Errors[0].Code);
            Assert.Contains("Invalid table name", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeTableHeader_WithInvalidTableNameInBrackets_ShouldReturnFailure()
        {
            var content = new[] { "[invalid-table-name]" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, result.Errors[0].Code);
            Assert.Contains("Invalid table name", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeTableHeader_WhenEndOfContentReached_ShouldReturnFailure()
        {
            var content = new[] { "# comment", "", "   " };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTableHeader(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.EndOfContent, result.Errors[0].Code);
            Assert.Contains("No more content to process", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeTable_WhenHeaderDecodeFails_ShouldReturnFailure()
        {
            var content = new[] { "not a valid header" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            Assert.False(result.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableHeader, result.Errors[0].Code);
        }

        [Fact]
        public void DecodeTable_WhenDuplicateEntryKey_ShouldAddError()
        {
            var content = new[] { "[TestTable]", "port=8080", "port=9090" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            Assert.True(result.Success);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ConfigFileErrorCode.InvalidKeyName, result.Errors[0].Code);
            Assert.Contains("Duplicate key", result.Errors[0].Message);
        }

        [Fact]
        public void DecodeTable_WhenEntryDecodeFailsWithInvalidFormat_ShouldAddError()
        {
            var content = new[] { "[TestTable]", "invalid entry without equals", "[NextTable]" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            Assert.True(result.Success);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void DecodeTable_WhenEntryDecodeFailsWithEndOfContent_ShouldBreakLoop()
        {
            var content = new[] { "[TestTable]", "port=8080" };
            int index = 0;

            var result = ConfigFileTableModel.DecodeTable(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("TestTable", result.Value.Key);
            Assert.Single(result.Value.Table);
        }
    }
}
