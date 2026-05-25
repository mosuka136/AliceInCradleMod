using System;
using System.Collections.Generic;
using BetterExperience.HLogSpace;
using BetterExperience.HProvider;
using UnityEngine;
using Xunit;
using BepInExLogEventArgs = BepInEx.Logging.LogEventArgs;
using BepInExLogLevel = BepInEx.Logging.LogLevel;
using BepInExManualLogSource = BepInEx.Logging.ManualLogSource;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.Test
{
    public class BepInExHLogTests : IDisposable
    {
        public void Dispose()
        {
            BepInExHLog.Initialize(HLog.LogLevel.Debug, null, null);
        }

        [Fact]
        public void Initialize_WhenCalled_SetsStaticProperties()
        {
            // Arrange
            var unityProvider = new UnityProvider();
            var loggerProvider = CreateLoggerProvider();

            // Act
            BepInExHLog.Initialize(HLog.LogLevel.Warning, unityProvider, loggerProvider);

            // Assert
            Assert.Equal(HLog.LogLevel.Warning, BepInExHLog.LogLevel);
            Assert.Same(unityProvider, BepInExHLog.UnityProvider);
            Assert.Same(loggerProvider, BepInExHLog.BepInExLogger);
        }

        [Fact]
        public void Log_WhenLevelBelowConfiguredThreshold_DoesNotWriteToBepInExLogger()
        {
            // Arrange
            var loggerProvider = CreateLoggerProvider();
            var entries = new List<BepInExLogEventArgs>();
            loggerProvider.Logger.LogEvent += (sender, args) => entries.Add(args);
            BepInExHLog.Initialize(HLog.LogLevel.Warning, null, loggerProvider);
            var logEntry = CreateLogEntry(HLog.LogLevel.Info, "filtered message");

            // Act
            BepInExHLog.Log(logEntry);

            // Assert
            Assert.Empty(entries);
        }

        [Theory]
        [InlineData(HLog.LogLevel.Error, BepInExLogLevel.Error)]
        [InlineData(HLog.LogLevel.Warning, BepInExLogLevel.Warning)]
        [InlineData(HLog.LogLevel.Info, BepInExLogLevel.Info)]
        [InlineData(HLog.LogLevel.Debug, BepInExLogLevel.Debug)]
        [InlineData(HLog.LogLevel.Notice, BepInExLogLevel.Info)]
        public void Log_WhenLoggerProviderExists_WritesExpectedBepInExLevel(HLog.LogLevel level, BepInExLogLevel expectedLevel)
        {
            // Arrange
            var loggerProvider = CreateLoggerProvider();
            var entries = new List<BepInExLogEventArgs>();
            loggerProvider.Logger.LogEvent += (sender, args) => entries.Add(args);
            BepInExHLog.Initialize(HLog.LogLevel.Debug, null, loggerProvider);
            var logEntry = CreateLogEntry(level, "mapped message");

            // Act
            BepInExHLog.Log(logEntry);

            // Assert
            var entry = Assert.Single(entries);
            Assert.Equal(expectedLevel, entry.Level);
            Assert.Equal(logEntry.ToString(), entry.Data);
            Assert.Same(loggerProvider.Logger, entry.Source);
        }

        [Fact]
        public void Log_WhenLoggerProviderAndUnityProviderAreNull_DoesNotThrow()
        {
            // Arrange
            BepInExHLog.Initialize(HLog.LogLevel.Debug, null, null);
            var logEntry = CreateLogEntry(HLog.LogLevel.Info, "no provider message");

            // Act
            var exception = Record.Exception(() => BepInExHLog.Log(logEntry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Log_WhenLoggerProviderIsNullAndUnityProviderExists_DoesNotThrow()
        {
            // Arrange
            BepInExHLog.Initialize(HLog.LogLevel.Debug, new UnityProvider(), null);
            var logEntry = CreateLogEntry(HLog.LogLevel.Info, "unity fallback message");

            // Act
            var exception = Record.Exception(() => BepInExHLog.Log(logEntry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Log_WhenLogEntryIsNull_SwallowsException()
        {
            // Arrange
            BepInExHLog.Initialize(HLog.LogLevel.Debug, null, CreateLoggerProvider());

            // Act
            var exception = Record.Exception(() => BepInExHLog.Log(null));

            // Assert
            Assert.Null(exception);
        }

        private static BepInExLoggerProvider CreateLoggerProvider()
        {
            return new BepInExLoggerProvider(new BepInExManualLogSource(Guid.NewGuid().ToString("N")));
        }

        private static LogEntry CreateLogEntry(HLog.LogLevel level, string message)
        {
            return new LogEntry(1, "12:34:56.789", 2, 3, "Scene", level, message, "File.cs", 4, "Member", null);
        }
    }
}
