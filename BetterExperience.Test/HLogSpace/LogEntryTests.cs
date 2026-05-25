using System;
using BetterExperience.HLogSpace;

namespace BetterExperience.Test.HLogSpace
{
    public class LogEntryTests
    {
        [Fact]
        public void Constructor_WhenOptionalStringsAreNull_UsesFallbackValues()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");

            // Act
            var entry = new LogEntry(7, null, 3, 12, null, HLog.LogLevel.Warning, null, null, 45, null, exception);

            // Assert
            Assert.Equal(7, entry.Id);
            Assert.Equal("??:??:??.???", entry.Timestamp);
            Assert.Equal(3, entry.ThreadId);
            Assert.Equal(12, entry.Frame);
            Assert.Equal("?", entry.Scene);
            Assert.Equal(HLog.LogLevel.Warning, entry.Level);
            Assert.Equal(string.Empty, entry.Message);
            Assert.Equal(string.Empty, entry.File);
            Assert.Equal(45, entry.Line);
            Assert.Equal(string.Empty, entry.Member);
            Assert.Same(exception, entry.Exception);
        }

        [Fact]
        public void Constructor_WhenAllValuesProvided_AssignsOriginalValues()
        {
            // Arrange
            var exception = new Exception("failure");

            // Act
            var entry = new LogEntry(11, "12:34:56.789", 8, 99, "BattleScene", HLog.LogLevel.Error, "Something happened", @"C:\Logs\Game.cs", 123, "Run", exception);

            // Assert
            Assert.Equal(11, entry.Id);
            Assert.Equal("12:34:56.789", entry.Timestamp);
            Assert.Equal(8, entry.ThreadId);
            Assert.Equal(99, entry.Frame);
            Assert.Equal("BattleScene", entry.Scene);
            Assert.Equal(HLog.LogLevel.Error, entry.Level);
            Assert.Equal("Something happened", entry.Message);
            Assert.Equal(@"C:\Logs\Game.cs", entry.File);
            Assert.Equal(123, entry.Line);
            Assert.Equal("Run", entry.Member);
            Assert.Same(exception, entry.Exception);
        }

        [Fact]
        public void ToString_WhenFileMemberAndExceptionPresent_IncludesLocationAndException()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");
            var entry = new LogEntry(5, "01:02:03.004", 2, 77, "Town", HLog.LogLevel.Notice, "Hello", @"C:\Code\Player.cs", 18, "Update", exception);

            // Act
            var result = entry.ToString();

            // Assert
            var expected = "[5] 01:02:03.004 T2 F77 S=Town Notice | Hello (Player.cs:18 Update)" + Environment.NewLine + exception;
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", @"C:\Code\Player.cs", 18)]
        [InlineData("Update", "", 18)]
        [InlineData("Update", @"C:\Code\Player.cs", 0)]
        public void ToString_WhenLocationInfoIncomplete_OmitsLocationSuffix(string member, string file, int line)
        {
            // Arrange
            var entry = new LogEntry(9, "09:08:07.006", 4, 15, "Menu", HLog.LogLevel.Info, "Ready", file, line, member, null);

            // Act
            var result = entry.ToString();

            // Assert
            Assert.Equal("[9] 09:08:07.006 T4 F15 S=Menu Info | Ready", result);
        }

        [Fact]
        public void ToString_WhenExceptionIsNull_ReturnsSingleLineText()
        {
            // Arrange
            var entry = new LogEntry(1, "10:11:12.013", 6, 20, "Map", HLog.LogLevel.Debug, "Trace", @"C:\Temp\Trace.cs", 3, "Tick", null);

            // Act
            var result = entry.ToString();

            // Assert
            Assert.Equal("[1] 10:11:12.013 T6 F20 S=Map Debug | Trace (Trace.cs:3 Tick)", result);
        }
    }
}
