using BetterExperience.BLogSpace;
using BetterExperience.HProvider;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Xunit.Abstractions;
using Xunit.Sdk;
using static BetterExperience.BLogSpace.BLog;

namespace BetterExperience.Test.HLogSpace
{
    [TestCaseOrderer("BetterExperience.Test.HLogSpace.HLogTests+AlphabeticalOrderer", "BetterExperience.Test")]
    public class HLogTests
    {
        [Fact]
        public void FlushQueue_01_WhenNotInitialized_DoesNotDispatchQueuedEntries()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();
            var entries = new List<LogEntry>();
            Action<LogEntry> handler = entry => entries.Add(entry);

            BLog.DisposeWriter();
            BLog.EnableLog = true;
            BLog.LogDirectory = logDirectory;
            BLog.LogFileName = "queued-before-init.log";
            BLog.HLogLevel = LogLevel.Debug;
            BLog.UnityProvider = new UnityProvider();
            BLog.OnLogAdd += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => BLog.WriteQueue(
                    LogLevel.Info,
                    "queued-before-init",
                    null,
                    nameof(FlushQueue_01_WhenNotInitialized_DoesNotDispatchQueuedEntries),
                    "before-init.cs",
                    11));

                // Assert
                Assert.Null(exception);
                Assert.Empty(entries);
                Assert.False(Directory.Exists(logDirectory));
            }
            finally
            {
                BLog.OnLogAdd -= handler;
                BLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void Initialize_02_WhenEnableLogIsTrue_SetsConfigurationCreatesWriterAndSubscribesDisposeHandler()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();
            var logFileName = "initialize.log";
            var unityProvider = new UnityProvider();

            BLog.EnableLog = true;

            try
            {
                // Act
                BLog.Initialize(logDirectory, logFileName, LogLevel.Warning, unityProvider);
                BLog.FlushQueue();

                // Assert
                Assert.Equal(logDirectory, BLog.LogDirectory);
                Assert.Equal(logFileName, BLog.LogFileName);
                Assert.Equal(LogLevel.Warning, BLog.HLogLevel);
                Assert.Same(unityProvider, BLog.UnityProvider);

                var logFilePath = GetSingleLogFilePath(logDirectory);
                BLog.DisposeWriter();
                var contentAfterInitialize = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-START", contentAfterInitialize);

                BLog.InitializeWriter();
                GameQuitManager.Dispose();

                var contentAfterDispose = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-END", contentAfterDispose);
            }
            finally
            {
                BLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void Initialize_03_WhenAlreadyInitialized_UpdatesConfigurationWithoutCreatingNewWriter()
        {
            // Arrange
            var initialLogDirectory = CreateLogDirectory();
            var newLogDirectory = CreateLogDirectory();
            var newLogFileName = "reconfigure.log";
            var unityProvider = new UnityProvider();

            BLog.Dispose();
            BLog.EnableLog = false;
            BLog.Initialize(initialLogDirectory, "initial.log", LogLevel.Debug, new UnityProvider());
            BLog.EnableLog = true;

            try
            {
                // Act
                BLog.Initialize(newLogDirectory, newLogFileName, LogLevel.Error, unityProvider);

                // Assert
                Assert.Equal(newLogDirectory, BLog.LogDirectory);
                Assert.Equal(newLogFileName, BLog.LogFileName);
                Assert.Equal(LogLevel.Error, BLog.HLogLevel);
                Assert.Same(unityProvider, BLog.UnityProvider);
                Assert.False(Directory.Exists(newLogDirectory));
            }
            finally
            {
                BLog.Dispose();
                DeleteDirectoryIfExists(initialLogDirectory);
                DeleteDirectoryIfExists(newLogDirectory);
            }
        }

        [Fact]
        public void InitializeWriter_04_WhenCalledTwice_DisposesExistingWriterAndAppendsLifecycleMarkers()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();

            ConfigureLogger(logDirectory, "writer.log", LogLevel.Debug, true);
            BLog.DisposeWriter();

            try
            {
                // Act
                BLog.InitializeWriter();
                BLog.InitializeWriter();
                BLog.DisposeWriter();

                // Assert
                var content = ReadAllTextShared(GetSingleLogFilePath(logDirectory));
                Assert.Equal(2, CountOccurrences(content, "LOG-START"));
                Assert.Equal(2, CountOccurrences(content, "LOG-END"));
            }
            finally
            {
                BLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void FlushQueue_08_WhenLoggingIsDisabled_DequeuesEntriesWithoutWritingOrRaisingEvent()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();
            var eventRaised = false;
            Action<LogEntry> handler = _ => eventRaised = true;

            BLog.DisposeWriter();
            BLog.EnableLog = false;
            BLog.Initialize(logDirectory, "disabled.log", LogLevel.Debug, new UnityProvider());
            BLog.EnableLog = true;
            BLog.InitializeWriter();
            var logFilePath = GetSingleLogFilePath(logDirectory);
            BLog.DisposeWriter();
            var contentBefore = ReadAllTextShared(logFilePath);
            BLog.EnableLog = false;
            BLog.OnLogAdd += handler;

            try
            {
                // Act
                BLog.WriteQueue(LogLevel.Error, "disabled-message", null, "DisabledMember", "disabled.cs", 55);

                // Assert
                Assert.False(eventRaised);
                Assert.Equal(contentBefore, ReadAllTextShared(logFilePath));
            }
            finally
            {
                BLog.OnLogAdd -= handler;
                BLog.EnableLog = true;
                BLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void InitializeWriter_09_WhenPathIsInvalid_SwallowsException()
        {
            // Arrange
            BLog.LogDirectory = "bad\0path";
            BLog.LogFileName = "invalid.log";

            // Act
            var exception = Record.Exception(BLog.InitializeWriter);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void WriteLog_10_WhenAHandlerThrows_ContinuesInvokingRemainingHandlers()
        {
            // Arrange
            var entry = new LogEntry(1, new DateTime(2026, 6, 13, 12, 34, 56, 789), 2, 3, "SceneX", LogLevel.Info, "message", "file.cs", 10, "Member", null);
            var entries = new List<LogEntry>();
            Action<LogEntry> throwingHandler = _ => throw new InvalidOperationException("boom");
            Action<LogEntry> recordingHandler = loggedEntry => entries.Add(loggedEntry);

            BLog.EnableLog = true;
            BLog.HLogLevel = LogLevel.Debug;
            BLog.OnLogAdd += throwingHandler;
            BLog.OnLogAdd += recordingHandler;

            try
            {
                // Act
                var exception = Record.Exception(() => BLog.WriteLog(entry));

                // Assert
                Assert.Null(exception);
                Assert.Same(entry, Assert.Single(entries));
            }
            finally
            {
                BLog.OnLogAdd -= recordingHandler;
                BLog.OnLogAdd -= throwingHandler;
            }
        }

        [Fact]
        public void WriteLog_11_WhenThereAreNoHandlers_DoesNotThrow()
        {
            // Arrange
            var entry = new LogEntry(2, new DateTime(2026, 6, 13, 12, 34, 56, 789), 2, 3, "SceneY", LogLevel.Warning, "message", "file.cs", 20, "Member", null);
            BLog.EnableLog = true;
            BLog.HLogLevel = LogLevel.Debug;

            // Act
            var exception = Record.Exception(() => BLog.WriteLog(entry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void DisposeWriter_12_WhenWriterIsNull_ReturnsWithoutThrowing()
        {
            // Arrange
            BLog.DisposeWriter();

            // Act
            var exception = Record.Exception(BLog.DisposeWriter);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void DisposeWriter_13_WhenWriterExists_WritesEndMarkerAndDisposesWriter()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();

            ConfigureLogger(logDirectory, "dispose.log", LogLevel.Debug, true);
            BLog.DisposeWriter();
            BLog.InitializeWriter();

            try
            {
                var logFilePath = GetSingleLogFilePath(logDirectory);

                // Act
                var exception = Record.Exception(BLog.DisposeWriter);

                // Assert
                Assert.Null(exception);
                var content = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-END", content);
            }
            finally
            {
                BLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task DisposeWriter_14_WhenDisposedConcurrently_SwallowsInternalExceptions()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();
            var start = new System.Threading.ManualResetEventSlim(false);

            ConfigureLogger(logDirectory, "dispose-throw.log", LogLevel.Debug, true);
            BLog.DisposeWriter();
            BLog.InitializeWriter();

            var tasks = Enumerable.Range(0, 16)
                .Select(_ => System.Threading.Tasks.Task.Run(() =>
                {
                    start.Wait();
                    return Record.Exception(BLog.DisposeWriter);
                }))
                .ToArray();

            try
            {
                // Act
                start.Set();
                var exceptions = await System.Threading.Tasks.Task.WhenAll(tasks);

                // Assert
                Assert.All(exceptions, Assert.Null);
            }
            finally
            {
                BLog.DisposeWriter();
                start.Dispose();
                DeleteDirectoryIfExists(logDirectory);
            }
        }




        private static void ConfigureLogger(string logDirectory, string logFileName, LogLevel logLevel, bool enableLog)
        {
            BLog.LogDirectory = logDirectory;
            BLog.LogFileName = logFileName;
            BLog.HLogLevel = logLevel;
            BLog.EnableLog = enableLog;
            BLog.UnityProvider = new UnityProvider();
        }

        private static string CreateLogDirectory()
        {
            return Path.Combine(Path.GetTempPath(), "BetterExperience.Tests", nameof(HLogTests), Guid.NewGuid().ToString("N"));
        }

        private static int GetCurrentLineNumber([System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            return line;
        }

        private static string ReadAllTextShared(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static string GetSingleLogFilePath(string logDirectory)
        {
            return Assert.Single(Directory.GetFiles(logDirectory, "*.log"));
        }

        private static int CountOccurrences(string value, string subString)
        {
            var count = 0;
            var index = 0;

            while ((index = value.IndexOf(subString, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += subString.Length;
            }

            return count;
        }


        private sealed class ActiveSceneNamePatchScope : IDisposable
        {
            private static readonly object SyncRoot = new object();

            private readonly Harmony _harmony;

            private ActiveSceneNamePatchScope(Harmony harmony)
            {
                _harmony = harmony;
            }

            public static ActiveSceneNamePatchScope Create(string sceneName)
            {
                lock (SyncRoot)
                {
                    var harmony = new Harmony($"BetterExperience.Test.HLogSpace.HLogTests.{Guid.NewGuid():N}");
                    var original = typeof(UnityProvider)
                        .GetProperty(nameof(global::BetterExperience.HProvider.UnityProvider.ActiveScene))
                        ?.GetMethod;
                    var prefixMethod = typeof(ActiveSceneNamePatchScope).GetMethod(nameof(ActiveScenePrefix));
                    SceneName = sceneName;
                    harmony.Patch(original, prefix: new HarmonyMethod(prefixMethod));
                    return new ActiveSceneNamePatchScope(harmony);
                }
            }

            public static string SceneName { get; set; }

            public void Dispose()
            {
                lock (SyncRoot)
                {
                    _harmony.UnpatchSelf();
                    SceneName = null;
                }
            }

            public static bool ActiveScenePrefix(ref Scene __result)
            {
                __result = default;
                __result.name = SceneName;
                return false;
            }
        }


        private static void DeleteDirectoryIfExists(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }

        private sealed class AlphabeticalOrderer : ITestCaseOrderer
        {
            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
                where TTestCase : ITestCase
            {
                return testCases.OrderBy(testCase => testCase.TestMethod.Method.Name, StringComparer.Ordinal);
            }
        }
    }
}
