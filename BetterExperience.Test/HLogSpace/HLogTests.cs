using BetterExperience.HLogSpace;
using BetterExperience.HProvider;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using Xunit.Abstractions;
using Xunit.Sdk;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.Test
{
    [TestCaseOrderer("BetterExperience.Test.HLogTests+AlphabeticalOrderer", "BetterExperience.Test")]
    public class HLogTests
    {
        [Fact]
        public void FlushQueue_01_WhenNotInitialized_DoesNotDispatchQueuedEntries()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();
            var entries = new List<LogEntry>();
            Action<LogEntry> handler = entry => entries.Add(entry);

            HLog.DisposeWriter();
            HLog.EnableLog = true;
            HLog.LogDirectory = logDirectory;
            HLog.LogFileName = "queued-before-init.log";
            HLog.HLogLevel = LogLevel.Debug;
            HLog.UnityProvider = new UnityProvider();
            HLog.OnLogAdd += handler;

            try
            {
                // Act
                var exception = Record.Exception(() => HLog.Write(
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
                HLog.OnLogAdd -= handler;
                HLog.DisposeWriter();
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

            HLog.EnableLog = true;

            try
            {
                // Act
                HLog.Initialize(logDirectory, logFileName, LogLevel.Warning, unityProvider);
                HLog.FlushQueue();

                // Assert
                Assert.Equal(logDirectory, HLog.LogDirectory);
                Assert.Equal(logFileName, HLog.LogFileName);
                Assert.Equal(LogLevel.Warning, HLog.HLogLevel);
                Assert.Same(unityProvider, HLog.UnityProvider);

                var logFilePath = GetSingleLogFilePath(logDirectory);
                HLog.DisposeWriter();
                var contentAfterInitialize = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-START", contentAfterInitialize);

                HLog.InintializeWriter();
                GameQuitManager.Dispose();

                var contentAfterDispose = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-END", contentAfterDispose);
            }
            finally
            {
                HLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void Initialize_03_WhenAlreadyInitialized_UpdatesConfigurationWithoutCreatingNewWriter()
        {
            // Arrange
            var newLogDirectory = CreateLogDirectory();
            var newLogFileName = "reconfigure.log";
            var unityProvider = new UnityProvider();

            HLog.EnableLog = true;
            HLog.DisposeWriter();

            try
            {
                // Act
                HLog.Initialize(newLogDirectory, newLogFileName, LogLevel.Error, unityProvider);

                // Assert
                Assert.Equal(newLogDirectory, HLog.LogDirectory);
                Assert.Equal(newLogFileName, HLog.LogFileName);
                Assert.Equal(LogLevel.Error, HLog.HLogLevel);
                Assert.Same(unityProvider, HLog.UnityProvider);
                Assert.False(Directory.Exists(newLogDirectory));
            }
            finally
            {
                HLog.DisposeWriter();
                DeleteDirectoryIfExists(newLogDirectory);
            }
        }

        [Fact]
        public void InintializeWriter_04_WhenCalledTwice_DisposesExistingWriterAndAppendsLifecycleMarkers()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();

            ConfigureLogger(logDirectory, "writer.log", LogLevel.Debug, true);
            HLog.DisposeWriter();

            try
            {
                // Act
                HLog.InintializeWriter();
                HLog.InintializeWriter();
                HLog.DisposeWriter();

                // Assert
                var content = ReadAllTextShared(GetSingleLogFilePath(logDirectory));
                Assert.Equal(2, CountOccurrences(content, "LOG-START"));
                Assert.Equal(2, CountOccurrences(content, "LOG-END"));
            }
            finally
            {
                HLog.DisposeWriter();
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

            HLog.DisposeWriter();
            HLog.EnableLog = false;
            HLog.Initialize(logDirectory, "disabled.log", LogLevel.Debug, new UnityProvider());
            HLog.EnableLog = true;
            HLog.InintializeWriter();
            var logFilePath = GetSingleLogFilePath(logDirectory);
            HLog.DisposeWriter();
            var contentBefore = ReadAllTextShared(logFilePath);
            HLog.EnableLog = false;
            HLog.OnLogAdd += handler;

            try
            {
                // Act
                HLog.Write(LogLevel.Error, "disabled-message", null, "DisabledMember", "disabled.cs", 55);

                // Assert
                Assert.False(eventRaised);
                Assert.Equal(contentBefore, ReadAllTextShared(logFilePath));
            }
            finally
            {
                HLog.OnLogAdd -= handler;
                HLog.EnableLog = true;
                HLog.DisposeWriter();
                DeleteDirectoryIfExists(logDirectory);
            }
        }

        [Fact]
        public void InintializeWriter_09_WhenPathIsInvalid_SwallowsException()
        {
            // Arrange
            HLog.LogDirectory = "bad\0path";
            HLog.LogFileName = "invalid.log";

            // Act
            var exception = Record.Exception(HLog.InintializeWriter);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void InvokeOnLogAdd_10_WhenAHandlerThrows_ContinuesInvokingRemainingHandlers()
        {
            // Arrange
            var entry = new LogEntry(1, "12:34:56.789", 2, 3, "SceneX", LogLevel.Info, "message", "file.cs", 10, "Member", null);
            var entries = new List<LogEntry>();
            Action<LogEntry> throwingHandler = _ => throw new InvalidOperationException("boom");
            Action<LogEntry> recordingHandler = loggedEntry => entries.Add(loggedEntry);

            HLog.OnLogAdd += throwingHandler;
            HLog.OnLogAdd += recordingHandler;

            try
            {
                // Act
                var exception = Record.Exception(() => HLog.InvokeOnLogAdd(entry));

                // Assert
                Assert.Null(exception);
                Assert.Same(entry, Assert.Single(entries));
            }
            finally
            {
                HLog.OnLogAdd -= recordingHandler;
                HLog.OnLogAdd -= throwingHandler;
            }
        }

        [Fact]
        public void InvokeOnLogAdd_11_WhenThereAreNoHandlers_DoesNotThrow()
        {
            // Arrange
            var entry = new LogEntry(2, "12:34:56.789", 2, 3, "SceneY", LogLevel.Warning, "message", "file.cs", 20, "Member", null);

            // Act
            var exception = Record.Exception(() => HLog.InvokeOnLogAdd(entry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void DisposeWriter_12_WhenWriterIsNull_ReturnsWithoutThrowing()
        {
            // Arrange
            HLog.DisposeWriter();

            // Act
            var exception = Record.Exception(HLog.DisposeWriter);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void DisposeWriter_13_WhenWriterExists_WritesEndMarkerAndDisposesWriter()
        {
            // Arrange
            var logDirectory = CreateLogDirectory();

            ConfigureLogger(logDirectory, "dispose.log", LogLevel.Debug, true);
            HLog.DisposeWriter();
            HLog.InintializeWriter();

            try
            {
                var logFilePath = GetSingleLogFilePath(logDirectory);

                // Act
                var exception = Record.Exception(HLog.DisposeWriter);

                // Assert
                Assert.Null(exception);
                var content = ReadAllTextShared(logFilePath);
                Assert.Contains("LOG-END", content);
            }
            finally
            {
                HLog.DisposeWriter();
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
            HLog.DisposeWriter();
            HLog.InintializeWriter();

            var tasks = Enumerable.Range(0, 16)
                .Select(_ => System.Threading.Tasks.Task.Run(() =>
                {
                    start.Wait();
                    return Record.Exception(HLog.DisposeWriter);
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
                HLog.DisposeWriter();
                start.Dispose();
                DeleteDirectoryIfExists(logDirectory);
            }
        }




        private static void ConfigureLogger(string logDirectory, string logFileName, LogLevel logLevel, bool enableLog)
        {
            HLog.LogDirectory = logDirectory;
            HLog.LogFileName = logFileName;
            HLog.HLogLevel = logLevel;
            HLog.EnableLog = enableLog;
            HLog.UnityProvider = new UnityProvider();
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
                    var harmony = new Harmony($"BetterExperience.Test.HLogTests.{Guid.NewGuid():N}");
                    var original = typeof(global::BetterExperience.HProvider.UnityProvider)
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
