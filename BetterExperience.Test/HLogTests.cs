using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using BetterExperience.HProvider;
using HLogLevel = BetterExperience.HLogSpace.HLog.LogLevel;
using System.Reflection;

namespace BetterExperience.Test
{
    public class HLogTests : IDisposable
    {
        private readonly List<string> _tempDirectories = new List<string>();
        private readonly List<string> _tempFiles = new List<string>();

        public HLogTests()
        {
            ResetHLogState();
        }

        public void Dispose()
        {
            HLog.DisposeWriter();
            ResetHLogState();

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
            HLog.Initialize(logDirectory, "test.log", HLogLevel.Info, null);

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
            HLog.Initialize(logDirectory, "test.log", HLogLevel.Info, null);
            HLog.DisposeWriter();

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
            HLog.Initialize(logDirectory, "notice.log", HLogLevel.Info, null);

            // Act
            var exception = Record.Exception(() => HLog.Notice("notice message"));
            HLog.DisposeWriter();

            // Assert
            Assert.Null(exception);
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
            HLog.Initialize(logDirectory, "write.log", HLogLevel.Info, null);

            // Act
            var exception = Record.Exception(() => HLog.Write(HLogLevel.Warning, "write message", new InvalidOperationException("boom"), "WriteCaller", @"C:\temp\caller.cs", 123));
            HLog.DisposeWriter();

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void InvokeOnLogAdd_HandlerThrows_StillInvokesRemainingHandlers()
        {
            // Arrange
            var invokeCount = 0;
            Action<LogEntry> throwingHandler = delegate(LogEntry _)
            {
                throw new InvalidOperationException("boom");
            };
            Action<LogEntry> succeedingHandler = delegate(LogEntry _)
            {
                invokeCount++;
            };

            HLog.OnLogAdd += throwingHandler;
            HLog.OnLogAdd += succeedingHandler;

            var entry = new LogEntry(1, "00:00:00.000", 1, 0, "?", HLogLevel.Info, "message", "file.cs", 1, "Member", null);

            // Act
            var exception = Record.Exception(() => HLog.InvokeOnLogAdd(entry));

            // Assert
            Assert.Null(exception);
            Assert.Equal(1, invokeCount);

            HLog.OnLogAdd -= throwingHandler;
            HLog.OnLogAdd -= succeedingHandler;
        }

        [Fact]
        public void Shutdown_WriterNotInitialized_DoesNotThrow()
        {
            // Arrange
            InitializeConfig(false);

            // Act
            var exception = Record.Exception(() => HLog.DisposeWriter());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Shutdown_WriterInitialized_WritesEndMarkerOnceAndReleasesFile()
        {
            // Arrange
            InitializeConfig(true);
            var logDirectory = CreateMissingDirectoryPath();
            HLog.Initialize(logDirectory, "shutdown.log", HLogLevel.Info, null);
            HLog.Info("before shutdown");
            var logFile = GetSingleLogFilePath(logDirectory, "shutdown-*.log");

            // Act
            var firstException = Record.Exception(() => HLog.DisposeWriter());
            var secondException = Record.Exception(() => HLog.DisposeWriter());
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
            HLog.DisposeWriter();
            var configPath = CreateTempFilePath("cfg");
            ConfigManager.Initialize(configPath);
            ConfigManager.EnableHLog.Value = enableHarmonyLog;
            HLog.EnableLog = ConfigManager.EnableHLog.Value;
            HLog.HLogLevel = ConfigManager.HLogLevel.Value;
        }

        private static void ResetHLogState()
        {
            SetStaticField(typeof(HLog), "_initialized", false);
            SetStaticField(typeof(HLog), "_seq", 0);
            SetStaticField(typeof(HLog), "_writer", null);
            SetStaticField(typeof(HLog), "<OnLogAdd>k__BackingField", null);
            SetStaticField(typeof(GameQuitManager), "<OnGameQuit>k__BackingField", null);

            var logEntriesField = typeof(HLog).GetField("_logEntries", BindingFlags.NonPublic | BindingFlags.Static);
            var logEntries = logEntriesField == null ? null : logEntriesField.GetValue(null) as System.Collections.IEnumerable;
            if (logEntries != null)
            {
                var tryDequeue = logEntries.GetType().GetMethod("TryDequeue");
                if (tryDequeue != null)
                {
                    var arguments = new object[] { null };
                    while ((bool)tryDequeue.Invoke(logEntries, arguments))
                    {
                        arguments[0] = null;
                    }
                }
            }

            HLog.EnableLog = true;
            HLog.LogDirectory = null;
            HLog.LogFileName = null;
            HLog.HLogLevel = HLogLevel.Info;
            HLog.UnityProvider = null;
        }

        private static void SetStaticField(Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                field.SetValue(null, value);
            }
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
    }
}
