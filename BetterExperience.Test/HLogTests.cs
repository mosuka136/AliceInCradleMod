using BepInEx.Logging;
using BetterExperience.BConfigManager;
using BetterExperience.HProvider;
using BepLogLevel = BepInEx.Logging.LogLevel;
using HLogLevel = BetterExperience.HLog.LogLevel;

namespace BetterExperience.Test
{
    public class HLogTests : IDisposable
    {
        private readonly List<string> _tempDirectories = new List<string>();
        private readonly List<string> _tempFiles = new List<string>();

        public void Dispose()
        {
            HLog.Shutdown();

            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }

            foreach (var directory in _tempDirectories.OrderByDescending(path => path.Length))
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                }
            }
        }

        [Fact]
        public void Initialize_EnableHarmonyLogDisabled_DoesNotCreateDirectory()
        {
            // Arrange
            InitializeConfig(false);
            var logDirectory = CreateMissingDirectoryPath();

            // Act
            HLog.Initialize(logDirectory, "test.log", new UnityProvider());

            // Assert
            Assert.False(Directory.Exists(logDirectory));
        }

        [Fact]
        public void Initialize_EnableHarmonyLogEnabled_CreatesDirectoryAndLogFile()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();

            // Act
            HLog.Initialize(logDirectory, "test.log", new UnityProvider());
            HLog.Shutdown();

            // Assert
            Assert.True(Directory.Exists(logDirectory));
            var logFile = GetSingleLogFilePath(logDirectory, "test-*.log");
            var content = File.ReadAllText(logFile);
            Assert.Contains("LOG-START-", content);
            Assert.Contains("LOG-END-", content);
        }

        [Fact]
        public void Notice_AfterInitialize_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();
            HLog.Initialize(logDirectory, "notice.log", new UnityProvider(), null, HLogLevel.Debug, HLogLevel.Debug);

            // Act
            var exception = Record.Exception(() => HLog.Notice("notice message"));
            HLog.Shutdown();

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void WriteLine_EnableHarmonyLogDisabled_DoesNotThrow()
        {
            // Act
            InitializeConfig(false);
            var exception = Record.Exception(() => HLog.WriteLine());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void WriteLine_WriterNotInitialized_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(true);

            // Act
            var exception = Record.Exception(() => HLog.WriteLine());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void WriteLine_WriterInitialized_AppendsBlankLine()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();
            HLog.Initialize(logDirectory, "line.log", new UnityProvider());

            // Act
            HLog.WriteLine();
            HLog.Shutdown();

            // Assert
            var logFile = GetSingleLogFilePath(logDirectory, "line-*.log");
            var content = File.ReadAllText(logFile);
            Assert.Contains("LOG-START-", content);
            Assert.Contains(Environment.NewLine + Environment.NewLine + "--------------------------------------------------LOG-END-", content);
        }

        [Fact]
        public void Write_EnableHarmonyLogDisabled_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(false);

            // Act
            var exception = Record.Exception(() => HLog.Write(HLogLevel.Warning, "disabled message", null, "Member", "File.cs", 12));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Write_WriterNotInitialized_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(true);

            // Act
            var exception = Record.Exception(() => HLog.Write(HLogLevel.Warning, "message", null, "Member", "File.cs", 12));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Write_AfterInitialize_SwallowsUnityEnvironmentExceptions()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();
            HLog.Initialize(logDirectory, "write.log", new UnityProvider(), null, HLogLevel.Debug, HLogLevel.Debug);

            // Act
            var exception = Record.Exception(() => HLog.Write(HLogLevel.Warning, "write message", new InvalidOperationException("boom"), "WriteCaller", @"C:\temp\caller.cs", 123));
            HLog.Shutdown();

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(HLogLevel.Error, BepLogLevel.Error)]
        [InlineData(HLogLevel.Warning, BepLogLevel.Warning)]
        [InlineData(HLogLevel.Info, BepLogLevel.Info)]
        [InlineData(HLogLevel.Debug, BepLogLevel.Debug)]
        [InlineData(HLogLevel.Notice, BepLogLevel.Info)]
        public void AdditionalLog_BepInExProviderPresent_UsesExpectedLogLevel(HLogLevel level, BepLogLevel expectedLevel)
        {
            // Arrange
            InitializeConfig(false);
            var source = new ManualLogSource("HLogTests.Additional");
            var capturedLogs = new List<CapturedLogEntry>();
            source.LogEvent += delegate(object sender, LogEventArgs args)
            {
                capturedLogs.Add(new CapturedLogEntry(args.Level, args.Data == null ? null : args.Data.ToString()));
            };
            var provider = new BepInExLoggerProvider(source);
            HLog.Initialize(CreateMissingDirectoryPath(), "provider.log", new UnityProvider(), provider);

            // Act
            HLog.AdditionalLog(level, "provider message");

            // Assert
            var capturedLog = Assert.Single(capturedLogs);
            Assert.Equal(expectedLevel, capturedLog.Level);
            Assert.Equal("provider message", capturedLog.Message);
        }

        [Fact]
        public void SafeSceneName_UnityProviderIsNull_ReturnsQuestionMark()
        {
            // Arrange
            InitializeConfig(false);
            HLog.Initialize(CreateMissingDirectoryPath(), "safe-scene-null.log", null);

            // Act
            var result = HLog.SafeSceneName();

            // Assert
            Assert.Equal("?", result);
        }

        [Fact]
        public void SafeSceneName_ActiveSceneNameIsBlank_ReturnsQuestionMark()
        {
            // Arrange
            InitializeConfig(false);
            HLog.Initialize(CreateMissingDirectoryPath(), "safe-scene-blank.log", new UnityProvider());

            // Act
            var result = HLog.SafeSceneName();

            // Assert
            Assert.Equal("?", result);
        }

        [Fact]
        public void Shutdown_WriterNotInitialized_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(false);

            // Act
            var exception = Record.Exception(() => HLog.Shutdown());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Shutdown_WriterInitialized_WritesEndMarkerOnceAndReleasesFile()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();
            HLog.Initialize(logDirectory, "shutdown.log", new UnityProvider());
            HLog.Info("before shutdown");
            var logFile = GetSingleLogFilePath(logDirectory, "shutdown-*.log");

            // Act
            var firstException = Record.Exception(() => HLog.Shutdown());
            var secondException = Record.Exception(() => HLog.Shutdown());
            var content = File.ReadAllText(logFile);
            File.Delete(logFile);

            // Assert
            Assert.Null(firstException);
            Assert.Null(secondException);
            Assert.Equal(1, CountOccurrences(content, "LOG-END-"));
            Assert.False(File.Exists(logFile));
        }

        private static int CountOccurrences(string content, string value)
        {
            var count = 0;
            var index = 0;

            while (true)
            {
                index = content.IndexOf(value, index, StringComparison.Ordinal);
                if (index < 0)
                {
                    return count;
                }

                count++;
                index += value.Length;
            }
        }




        private void InitializeConfig(bool enableHarmonyLog)
        {
            HLog.Shutdown();
            var configPath = CreateTempFilePath("cfg");
            ConfigManager.Initialize(configPath);
            ConfigManager.EnableHarmonyLog.Value = enableHarmonyLog;
        }

        private string CreateMissingDirectoryPath()
        {
            var path = CreateTempDirectoryPath();
            Directory.Delete(path);
            return path;
        }

        private string CreateTempDirectoryPath()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _tempDirectories.Add(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private string CreateTempFilePath(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "." + extension);
            _tempFiles.Add(path);
            return path;
        }

        private static string GetSingleLogFilePath(string directory, string searchPattern)
        {
            return Assert.Single(Directory.GetFiles(directory, searchPattern));
        }

        private sealed class CapturedLogEntry
        {
            public CapturedLogEntry(BepLogLevel level, string message)
            {
                Level = level;
                Message = message;
            }

            public BepLogLevel Level { get; private set; }

            public string Message { get; private set; }
        }
    }
}
