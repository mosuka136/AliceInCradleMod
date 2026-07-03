using BetterExperience.HLogGUI;
using BetterExperience.BLogSpace;
using static BetterExperience.HLogGUI.ListEditor;

namespace BetterExperience.Test.HLogGUI
{
    public class ListEditorTests
    {
        [Theory]
        [InlineData("Warning", BLog.LogLevel.Info, true)]
        [InlineData("Info", BLog.LogLevel.Info, true)]
        [InlineData("Debug", BLog.LogLevel.Info, false)]
        [InlineData("not-a-level", BLog.LogLevel.Debug, false)]
        [InlineData("", BLog.LogLevel.Debug, false)]
        public void IsLogLevelHigherOrEqual_WithLevelText_ReturnsExpectedResult(
            string entryLogLevel,
            BLog.LogLevel filterLogLevel,
            bool expected)
        {
            // Act
            var result = IsLogLevelHigherOrEqual(entryLogLevel, filterLogLevel);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetText_WithKnownContentTypes_ReturnsMatchingBindingText()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");
            var entry = CreateEntry(
                id: 8,
                timestamp: new DateTime(2026, 6, 13, 4, 5, 6, 7),
                threadId: 9,
                frame: 10,
                scene: "Map",
                level: BLog.LogLevel.Error,
                message: "Message",
                file: @"C:\Code\Patch.cs",
                line: 11,
                member: "Run",
                exception: exception);
            entry.UpdateRepeat(new DateTime(2026, 6, 13, 5, 6, 7, 8));
            var binding = new EntryBinding(entry);
            var expected = new Dictionary<EntryContentType, string>
            {
                { EntryContentType.Id, "8" },
                { EntryContentType.Timestamp, "04:05:06.007" },
                { EntryContentType.ThreadId, "9" },
                { EntryContentType.Frame, "10" },
                { EntryContentType.Scene, "Map" },
                { EntryContentType.Level, "Error" },
                { EntryContentType.Message, "Message" },
                { EntryContentType.File, @"C:\Code\Patch.cs" },
                { EntryContentType.Line, "11" },
                { EntryContentType.Member, "Run" },
                { EntryContentType.Exception, exception.ToString() },
                { EntryContentType.LastRepeatTime, "05:06:07.008" },
                { EntryContentType.RepeatCount, "2" },
            };

            // Act & Assert
            foreach (var pair in expected)
            {
                Assert.Equal(pair.Value, ColumnEditor.GetText(binding, pair.Key));
            }
        }

        [Fact]
        public void GetText_WithNoneContentType_ReturnsEmptyString()
        {
            // Arrange
            var binding = new EntryBinding(CreateEntry());

            // Act
            var result = ColumnEditor.GetText(binding, EntryContentType.None);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        private static LogEntry CreateEntry(
            int id = 1,
            DateTime? timestamp = null,
            int threadId = 2,
            int frame = 3,
            string scene = "Scene",
            BLog.LogLevel level = BLog.LogLevel.Info,
            string message = "Message",
            string file = @"C:\Code\File.cs",
            int line = 10,
            string member = "Member",
            Exception exception = null)
        {
            return new LogEntry(
                id,
                timestamp ?? new DateTime(2026, 6, 13, 0, 0, 0),
                threadId,
                frame,
                scene,
                level,
                message,
                file,
                line,
                member,
                exception);
        }
    }
}
