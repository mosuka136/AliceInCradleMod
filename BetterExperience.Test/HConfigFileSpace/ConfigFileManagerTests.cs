using BetterExperience.HConfigFileSpace;
using BetterExperience.HTranslatorSpace;

namespace BetterExperience.Test.HConfigFileSpace
{
    public class ConfigFileManagerTests : IDisposable
    {
        private readonly List<string> _tempFiles = new List<string>();
        private readonly List<string> _tempDirectories = new List<string>();

        private string CreateTempConfigPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
            _tempFiles.Add(path);
            return path;
        }

        private string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempDirectories.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var path in _tempFiles)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    var tmpPath = path + ".tmp";
                    if (File.Exists(tmpPath))
                        File.Delete(tmpPath);
                }
                catch { }
            }

            foreach (var dir in _tempDirectories)
            {
                try
                {
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }
                catch { }
            }
        }

        [Fact]
        public void FileName_WhenFilePathSet_ReturnsFileName()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);

            var result = manager.FileName;

            Assert.Equal(Path.GetFileName(tempPath), result);
        }

        [Fact]
        public void Constructor_WhenFileDoesNotExist_CreatesEmptyFileSheet()
        {
            var tempPath = CreateTempConfigPath();

            var manager = new ConfigFileManager(tempPath);

            Assert.NotNull(manager.FileSheet);
            Assert.NotNull(manager.Sheet);
            Assert.Equal(tempPath, manager.FilePath);
        }

        [Fact]
        public void Read_WhenFileHasDecodeErrors_LogsErrorsAndReturnsTrue()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[InvalidTable\nKey = Value");

            var manager = new ConfigFileManager(tempPath);

            Assert.NotNull(manager.FileSheet);
        }

        [Fact]
        public void Read_WhenIOExceptionOccurs_LogsErrorAndReturnsFalse()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[Table]\nKey=Value");
            var manager = new ConfigFileManager(tempPath);
            
            using (var stream = File.Open(tempPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = manager.Read();
                Assert.False(result);
            }
        }

        [Fact]
        public void Write_WhenEncodeSucceeds_WritesFileAndReturnsTrue()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);

            var result = manager.Write();

            Assert.True(result);
            Assert.True(File.Exists(tempPath));
        }

        [Fact]
        public void Write_WhenEncodeHasErrors_LogsErrorsAndReturnsFalse()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);
            var table = new ConfigFileTableModel("Test", new Translator());
            manager.FileSheet.AddTable(table);
            table.Key = "Invalid-Table-Name!";

            var result = manager.Write();

            Assert.False(result);
        }

        [Fact]
        public void Write_WhenDirectoryDoesNotExist_CreatesDirectory()
        {
            var tempDir = CreateTempDirectory();
            var tempPath = Path.Combine(tempDir, "subdir", "test.cfg");
            _tempFiles.Add(tempPath);
            var manager = new ConfigFileManager(CreateTempConfigPath());
            manager.FilePath = tempPath;

            var result = manager.Write();

            Assert.True(result);
            Assert.True(Directory.Exists(Path.GetDirectoryName(tempPath)));
            Assert.True(File.Exists(tempPath));
        }

        [Fact]
        public void Write_WhenIOExceptionOccurs_LogsErrorAndReturnsFalse()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);
            
            using (var stream = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                var result = manager.Write();
                Assert.False(result);
            }
        }

        [Fact]
        public void CreateTable_WhenTableExistsInFileSheetButNotInSheet_AddsTableToSheet()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[ExistingTable]\n");
            var manager = new ConfigFileManager(tempPath);
            var tableName = new Translator("表名", "TableName");
            var description = new Translator("描述", "Description");

            manager.CreateTable("ExistingTable", tableName, description);

            Assert.True(manager.Sheet.Contains("ExistingTable"));
            Assert.Equal(1, manager.Sheet.Count);
        }

        [Fact]
        public void CreateTable_WhenTableNameInvalid_ThrowsInvalidOperationException()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);
            var tableName = new Translator("表名", "TableName");
            var invalidTableKey = "Invalid-Table!";

            var exception = Assert.Throws<InvalidOperationException>(() => 
                manager.CreateTable(invalidTableKey, tableName));

            Assert.Contains("Failed to create config table", exception.Message);
            Assert.Contains(invalidTableKey, exception.Message);
        }

        [Fact]
        public void Bind_WhenEntryExists_ReturnsBoundConfigEntry()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nTestKey = 123\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));

            var result = manager.Bind<int>("TestTable", "TestKey", 456, new Translator("测试键", "TestKey"), new Translator("描述", "Description"));

            Assert.NotNull(result);
            Assert.Equal("TestTable", result.TableName);
            Assert.Equal("TestKey", result.Key);
        }

        [Fact]
        public void Bind_WhenEntryDoesNotExist_CreatesNewEntry()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));

            var result = manager.Bind<string>("TestTable", "NewKey", "DefaultValue", new Translator("新键", "NewKey"), new Translator("描述", "Description"));

            Assert.NotNull(result);
            Assert.Equal("TestTable", result.TableName);
            Assert.Equal("NewKey", result.Key);
        }

        [Fact]
        public void Bind_WhenTableDoesNotExist_ThrowsArgumentException()
        {
            var tempPath = CreateTempConfigPath();
            var manager = new ConfigFileManager(tempPath);

            var exception = Assert.Throws<ArgumentException>(() =>
                manager.Bind<string>("NonExistentTable", "Key", "Value", new Translator("键", "Key"), new Translator("描述", "Description")));

            Assert.Contains("Config table not found", exception.Message);
            Assert.Contains("NonExistentTable", exception.Message);
            Assert.Equal("tableKey", exception.ParamName);
        }

        [Fact]
        public void Bind_WhenKeyNameInvalid_ThrowsArgumentException()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));

            var exception = Assert.Throws<ArgumentException>(() =>
                manager.Bind<string>("TestTable", "Invalid-Key!", "Value", new Translator("键", "Key"), new Translator("描述", "Description")));

            Assert.Contains("Invalid key name", exception.Message);
            Assert.Contains("TestTable.Invalid-Key!", exception.Message);
            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public void Read_WhenFileContainsMultipleInvalidTables_LogsAllErrors()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[Table1\nKey1=Value1\n[Table2\nKey2=Value2");

            var manager = new ConfigFileManager(tempPath);

            Assert.NotNull(manager.FileSheet);
        }

        [Fact]
        public void Read_WhenFileContainsDuplicateTables_LogsErrors()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nKey1=Value1\n[TestTable]\nKey2=Value2");

            var manager = new ConfigFileManager(tempPath);

            Assert.NotNull(manager.FileSheet);
        }

        [Fact]
        public void Constructor_CallsReadMethod()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nTestKey=TestValue\n");

            var manager = new ConfigFileManager(tempPath);

            Assert.NotNull(manager.FileSheet);
            Assert.NotNull(manager.Sheet);
            var tableResult = manager.FileSheet.GetTable("TestTable");
            Assert.True(tableResult.Success);
        }

        [Fact]
        public void Bind_WhenCreatingNewEntryWithComplexType_EncodesValue()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));

            var result = manager.Bind<bool>("TestTable", "BoolKey", true, new Translator("布尔键", "BoolKey"), new Translator("描述", "Description"));

            Assert.NotNull(result);
            Assert.Equal("TestTable", result.TableName);
            Assert.Equal("BoolKey", result.Key);
        }

        [Fact]
        public void Bind_AddsValueChangedHandler()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));

            var result = manager.Bind<string>("TestTable", "TestKey", "DefaultValue", new Translator("测试键", "TestKey"), new Translator("描述", "Description"));

            Assert.NotNull(result);
            Assert.True(manager.Sheet.Contains("TestTable"));
            Assert.Contains(result, manager.Sheet["TestTable"]);
        }

        [Fact]
        public void Reload_WhenCalledWithBoundEntries_UpdatesEntriesFromFile()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nTestKey = \"OriginalValue\"\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));
            var entry = manager.Bind<string>("TestTable", "TestKey", "DefaultValue", new Translator("测试键", "TestKey"), new Translator("描述", "Description"));

            File.WriteAllText(tempPath, "[TestTable]\nTestKey = \"UpdatedValue\"\n");
            manager.Reload();

            Assert.NotNull(manager.FileSheet);
        }

        [Fact]
        public void Reload_WhenSaveOnConfigSetIsTrue_PreservesAndRestoresIt()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nTestKey = \"Value\"\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));
            manager.Bind<string>("TestTable", "TestKey", "DefaultValue", new Translator("测试键", "TestKey"), new Translator("描述", "Description"));
            manager.SaveOnConfigSet = true;

            manager.Reload();

            Assert.True(manager.SaveOnConfigSet);
        }

        [Fact]
        public void Reload_WhenEntryDoesNotExistInFile_SkipsRebinding()
        {
            var tempPath = CreateTempConfigPath();
            File.WriteAllText(tempPath, "[TestTable]\nTestKey = \"Value\"\n");
            var manager = new ConfigFileManager(tempPath);
            manager.CreateTable("TestTable", new Translator("测试表", "TestTable"));
            manager.Bind<string>("TestTable", "TestKey", "DefaultValue", new Translator("测试键", "TestKey"), new Translator("描述", "Description"));

            File.WriteAllText(tempPath, "[TestTable]\n");
            manager.Reload();

            Assert.NotNull(manager.FileSheet);
        }
    }
}
