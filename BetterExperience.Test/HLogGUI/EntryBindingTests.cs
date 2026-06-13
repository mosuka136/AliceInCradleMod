using BetterExperience.HLogGUI;
using BetterExperience.HLogSpace;

namespace BetterExperience.Test.HLogGUI
{
    public class EntryBindingTests
    {
        [Fact]
        public void Constructor_WhenEntryProvided_ExposesFormattedFields()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");
            var timestamp = new DateTime(2026, 6, 13, 1, 2, 3, 4);
            var entry = CreateEntry(
                id: 7,
                timestamp: timestamp,
                threadId: 3,
                frame: 12,
                scene: "Battle",
                level: HLog.LogLevel.Warning,
                message: "Message",
                file: @"C:\Code\Player.cs",
                line: 45,
                member: "Update",
                exception: exception);

            // Act
            var binding = new EntryBinding(entry);

            // Assert
            Assert.Same(entry, binding.Entry);
            Assert.Equal("7", binding.Id);
            Assert.Equal("01:02:03.004", binding.Timestamp);
            Assert.Equal("3", binding.ThreadId);
            Assert.Equal("12", binding.Frame);
            Assert.Equal("Battle", binding.Scene);
            Assert.Equal("Warning", binding.Level);
            Assert.Equal("Message", binding.Message);
            Assert.Equal(@"C:\Code\Player.cs", binding.File);
            Assert.Equal("45", binding.Line);
            Assert.Equal("Update", binding.Member);
            Assert.Equal(exception.ToString(), binding.Exception);
            Assert.Equal("01:02:03.004", binding.LastRepeatTime);
            Assert.Equal("1", binding.RepeatCount);
            Assert.False(binding.IsRepeated);
        }

        [Fact]
        public void Properties_WhenEntryIsRepeated_ExposeRepeatState()
        {
            // Arrange
            var entry = CreateEntry(timestamp: new DateTime(2026, 6, 13, 1, 2, 3, 4));
            entry.UpdateRepeat(new DateTime(2026, 6, 13, 2, 3, 4, 5));

            // Act
            var binding = new EntryBinding(entry);

            // Assert
            Assert.Equal("02:03:04.005", binding.LastRepeatTime);
            Assert.Equal("2", binding.RepeatCount);
            Assert.True(binding.IsRepeated);
        }

        [Fact]
        public void Equals_WhenEntriesHaveSameLogContent_ReturnsTrue()
        {
            // Arrange
            var first = new EntryBinding(CreateEntry(id: 1, timestamp: new DateTime(2026, 6, 13, 1, 0, 0), frame: 10));
            var second = new EntryBinding(CreateEntry(id: 2, timestamp: new DateTime(2026, 6, 13, 2, 0, 0), frame: 20));

            // Act
            var typedEquals = first.Equals(second);
            var objectEquals = first.Equals((object)second);

            // Assert
            Assert.True(typedEquals);
            Assert.True(objectEquals);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void Equals_WhenOtherIsNullOrDifferentContent_ReturnsFalse()
        {
            // Arrange
            var binding = new EntryBinding(CreateEntry(message: "first"));
            var different = new EntryBinding(CreateEntry(message: "second"));

            // Act & Assert
            Assert.False(binding.Equals(null));
            Assert.False(binding.Equals((object)"not an entry binding"));
            Assert.False(binding.Equals(different));
        }

        [Fact]
        public void ToString_ReturnsUnderlyingEntryText()
        {
            // Arrange
            var entry = CreateEntry(id: 3, timestamp: new DateTime(2026, 6, 13, 10, 11, 12, 13), message: "copy me");
            var binding = new EntryBinding(entry);

            // Act
            var result = binding.ToString();

            // Assert
            Assert.Equal(entry.ToString(), result);
        }

        private static LogEntry CreateEntry(
            int id = 1,
            DateTime? timestamp = null,
            int threadId = 2,
            int frame = 3,
            string scene = "Scene",
            HLog.LogLevel level = HLog.LogLevel.Info,
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
