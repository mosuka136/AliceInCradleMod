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
            var result = ConfigFileTablesModel.Table.IsValidTableName(tableName);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AddTableWhenDuplicateNameShouldFail()
        {
            var model = new ConfigFileTablesModel();
            var table1 = ConfigFileTablesModel.Table.Create("General", new Translator(english: "First")).Value;
            var table2 = ConfigFileTablesModel.Table.Create("General", new Translator(english: "Second")).Value;

            var firstResult = model.AddTable(table1);
            var secondResult = model.AddTable(table2);

            Assert.True(firstResult.Success);
            Assert.False(secondResult.Success);
            Assert.Equal(ConfigFileErrorCode.InvalidTableName, secondResult.Errors[0].Code);
        }

        [Fact]
        public void EncodeTableWhenDescriptionAndEntriesExistShouldIncludeHeaderAndBody()
        {
            var table = new ConfigFileTablesModel.Table("General", new Translator(english: "Main settings"));
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

            var result = ConfigFileTablesModel.Table.DecodeTableHeader(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("General", result.Value.TableKey);
            Assert.Equal(string.Empty, result.Value.Description);
            Assert.Equal(3, index);
        }

        [Fact]
        public void DecodeTableShouldStopAtNextTableHeader()
        {
            var content = new[] { "[General]", "port=8080", "[Advanced]", "mode=1" };
            int index = 0;

            var result = ConfigFileTablesModel.Table.DecodeTable(content, ref index);

            Assert.True(result.Success);
            Assert.Equal("General", result.Value.TableKey);
            Assert.Single(result.Value.Entries);
            var entry = (ConfigFileEntryModel)result.Value.Entries[0];
            Assert.Equal("port", entry.Key);
            Assert.Equal(2, index);
        }

        [Fact]
        public void DecodeTablesWhenMultipleTablesExistShouldParseAllTables()
        {
            var content = new[] { "[General]", "port=8080", "[Advanced]", "mode=1" };
            int index = 0;

            var result = ConfigFileTablesModel.DecodeTables(content, ref index);

            Assert.True(result.Success);
            Assert.Equal(2, result.Value.Tables.Count);
            Assert.Equal(4, index);

            var general = (ConfigFileTablesModel.Table)result.Value.Tables["General"];
            var advanced = (ConfigFileTablesModel.Table)result.Value.Tables["Advanced"];
            Assert.Equal("8080", ((ConfigFileEntryModel)general.Entries[0]).Value);
            Assert.Equal("1", ((ConfigFileEntryModel)advanced.Entries[0]).Value);
        }
    }
}
