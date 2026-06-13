using BetterExperience.HLogGUI;
using BetterExperience.HLogSpace;

namespace BetterExperience.Test.HLogGUI
{
    public class EntryListBindingTests
    {
        [Fact]
        public void AddEntry_WhenEntryIsNull_DoesNotChangeCollection()
        {
            // Arrange
            var list = new EntryListBinding();

            // Act
            list.AddEntry(null);

            // Assert
            Assert.Empty(list.EntryList);
            Assert.Empty(list.Entries);
            Assert.All(list.SortedEntryList.Values, Assert.Empty);
            Assert.False(list.HasExceptionEntry);
            Assert.False(list.HasRepeatedEntry);
        }

        [Fact]
        public void AddEntry_WhenEntryIsNew_AddsItToDefaultAndSortedCollections()
        {
            // Arrange
            var list = new EntryListBinding();
            var entry = new EntryBinding(CreateEntry(id: 5, message: "new entry"));

            // Act
            list.AddEntry(entry);

            // Assert
            Assert.Same(entry, Assert.Single(list.EntryList));
            Assert.Same(entry, Assert.Single(list.Entries));
            Assert.All(list.SortedEntryList.Values, sortedSet => Assert.Same(entry, Assert.Single(sortedSet)));
        }

        [Fact]
        public void AddEntry_WhenEntryHasExceptionAndRepeatCount_SetsFlags()
        {
            // Arrange
            var list = new EntryListBinding();
            var logEntry = CreateEntry(exception: new InvalidOperationException("boom"));
            logEntry.UpdateRepeat(new DateTime(2026, 6, 13, 2, 0, 0));

            // Act
            list.AddEntry(new EntryBinding(logEntry));

            // Assert
            Assert.True(list.HasExceptionEntry);
            Assert.True(list.HasRepeatedEntry);
        }

        [Fact]
        public void AddEntry_WhenEquivalentEntryAlreadyExists_MergesRepeatStateAndKeepsSingleEntry()
        {
            // Arrange
            var list = new EntryListBinding();
            var original = CreateEntry(id: 1, timestamp: new DateTime(2026, 6, 13, 1, 0, 0), message: "same");
            var repeated = CreateEntry(id: 2, timestamp: new DateTime(2026, 6, 13, 1, 0, 1), message: "same");
            repeated.UpdateRepeat(new DateTime(2026, 6, 13, 1, 0, 2));

            // Act
            list.AddEntry(new EntryBinding(original));
            list.AddEntry(new EntryBinding(repeated));

            // Assert
            var merged = Assert.Single(list.EntryList);
            Assert.Equal("3", merged.RepeatCount);
            Assert.Equal("01:00:02.000", merged.LastRepeatTime);
            Assert.True(merged.IsRepeated);
            Assert.True(list.HasRepeatedEntry);
            Assert.All(list.SortedEntryList.Values, sortedSet => Assert.Same(merged, Assert.Single(sortedSet)));
        }

        [Fact]
        public void Entries_WhenSortOrderIsTimestamp_ReturnsAscendingOrDescendingTimestampOrder()
        {
            // Arrange
            var list = new EntryListBinding();
            var middle = new EntryBinding(CreateEntry(id: 1, timestamp: new DateTime(2026, 6, 13, 12, 0, 0), message: "middle"));
            var late = new EntryBinding(CreateEntry(id: 2, timestamp: new DateTime(2026, 6, 13, 13, 0, 0), message: "late"));
            var early = new EntryBinding(CreateEntry(id: 3, timestamp: new DateTime(2026, 6, 13, 11, 0, 0), message: "early"));
            list.AddEntry(middle);
            list.AddEntry(late);
            list.AddEntry(early);

            // Act
            list.SortOrder = EntryContentType.Timestamp;
            var ascending = list.Entries.Select(x => x.Message).ToArray();
            list.IsSortDescending = true;
            var descending = list.Entries.Select(x => x.Message).ToArray();

            // Assert
            Assert.Equal(new[] { "early", "middle", "late" }, ascending);
            Assert.Equal(new[] { "late", "middle", "early" }, descending);
        }

        [Fact]
        public void CompareByIdFallback_WhenPrimaryComparisonIsEqual_UsesEntryId()
        {
            // Arrange
            var lowerId = new EntryBinding(CreateEntry(id: 1));
            var higherId = new EntryBinding(CreateEntry(id: 2));

            // Act
            var result = EntryListBinding.CompareByIdFallback(higherId, lowerId, 0);

            // Assert
            Assert.True(result > 0);
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
