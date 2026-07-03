using BetterExperience.BLogSpace;
using BepInExLogEventArgs = BepInEx.Logging.LogEventArgs;
using BepInExLogLevel = BepInEx.Logging.LogLevel;
using BepInExManualLogSource = BepInEx.Logging.ManualLogSource;

namespace BetterExperience.Test.HLogSpace
{
    public class BepInExHLogTests : IDisposable
    {
        public void Dispose()
        {
            BepInExLog.Initialize(BLog.LogLevel.Debug, null, null);
        }

        [Fact]
        public void Initialize_WhenCalled_SetsStaticProperties()
        {
            // Arrange
            var unityProvider = new UnityProvider();
            var loggerProvider = CreateLoggerProvider();

            // Act
            BepInExLog.Initialize(BLog.LogLevel.Warning, unityProvider, loggerProvider);

            // Assert
            Assert.Equal(BLog.LogLevel.Warning, BepInExLog.Level);
            Assert.Same(unityProvider, BepInExLog.UnityProvider);
            Assert.Same(loggerProvider, BepInExLog.BepInExLogger);
        }

        [Fact]
        public void Log_WhenLevelBelowConfiguredThreshold_DoesNotWriteToBepInExLogger()
        {
            // Arrange
            var loggerProvider = CreateLoggerProvider();
            var entries = new List<BepInExLogEventArgs>();
            loggerProvider.Logger.LogEvent += (sender, args) => entries.Add(args);
            BepInExLog.Initialize(BLog.LogLevel.Warning, null, loggerProvider);
            var logEntry = CreateLogEntry(BLog.LogLevel.Info, "filtered message");

            // Act
            BepInExLog.Log(logEntry);

            // Assert
            Assert.Empty(entries);
        }

        [Theory]
        [InlineData(BLog.LogLevel.Error, BepInExLogLevel.Error)]
        [InlineData(BLog.LogLevel.Warning, BepInExLogLevel.Warning)]
        [InlineData(BLog.LogLevel.Info, BepInExLogLevel.Info)]
        [InlineData(BLog.LogLevel.Debug, BepInExLogLevel.Debug)]
        [InlineData(BLog.LogLevel.Notice, BepInExLogLevel.Info)]
        public void Log_WhenLoggerProviderExists_WritesExpectedBepInExLevel(BLog.LogLevel level, BepInExLogLevel expectedLevel)
        {
            // Arrange
            var loggerProvider = CreateLoggerProvider();
            var entries = new List<BepInExLogEventArgs>();
            loggerProvider.Logger.LogEvent += (sender, args) => entries.Add(args);
            BepInExLog.Initialize(BLog.LogLevel.Debug, null, loggerProvider);
            var logEntry = CreateLogEntry(level, "mapped message");

            // Act
            BepInExLog.Log(logEntry);

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
            BepInExLog.Initialize(BLog.LogLevel.Debug, null, null);
            var logEntry = CreateLogEntry(BLog.LogLevel.Info, "no provider message");

            // Act
            var exception = Record.Exception(() => BepInExLog.Log(logEntry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Log_WhenLoggerProviderIsNullAndUnityProviderExists_DoesNotThrow()
        {
            // Arrange
            BepInExLog.Initialize(BLog.LogLevel.Debug, new UnityProvider(), null);
            var logEntry = CreateLogEntry(BLog.LogLevel.Info, "unity fallback message");

            // Act
            var exception = Record.Exception(() => BepInExLog.Log(logEntry));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Log_WhenLogEntryIsNull_SwallowsException()
        {
            // Arrange
            BepInExLog.Initialize(BLog.LogLevel.Debug, null, CreateLoggerProvider());

            // Act
            var exception = Record.Exception(() => BepInExLog.Log(null));

            // Assert
            Assert.Null(exception);
        }

        private static BepInExLoggerProvider CreateLoggerProvider()
        {
            return new BepInExLoggerProvider(new BepInExManualLogSource(Guid.NewGuid().ToString("N")));
        }

        private static LogEntry CreateLogEntry(BLog.LogLevel level, string message)
        {
            return new LogEntry(1, new DateTime(2026, 6, 13, 12, 34, 56, 789), 2, 3, "Scene", level, message, "File.cs", 4, "Member", null);
        }
    }
}
