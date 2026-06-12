using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.HLogGUI
{
    public class EntryListBinding
    {
        public bool HasRepeatedEntry { get; private set; } = false;
        public bool HasExceptionEntry { get; private set; } = false;
        public List<EntryBinding> EntryList { get; } = new List<EntryBinding>();
        public EntryContentType SortOrder { get; set; } = EntryContentType.None;
        public bool IsSortDescending { get; set; } = false;
        public Dictionary<EntryContentType, SortedSet<EntryBinding>> SortedEntryList { get; }
        public IEnumerable<EntryBinding> Entries
        {
            get
            {
                if (SortedEntryList.TryGetValue(SortOrder, out var sortedSet))
                    return IsSortDescending ? sortedSet.Reverse() : sortedSet;
                else
                    return EntryList;
            }
        }

        public EntryListBinding()
        {
            var comparerById = Comparer<EntryBinding>.Create(CompareId);
            var comparerByTimestamp = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.Timestamp.CompareTo(b.Entry.Timestamp);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByThreadId = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.ThreadId.CompareTo(b.Entry.ThreadId);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByFrame = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.Frame, b.Frame, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByScene = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.Scene, b.Scene, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByLevel = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.Level.CompareTo(b.Entry.Level);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByMessage = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.Message, b.Message, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByFile = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.File, b.File, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByLine = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.Line.CompareTo(b.Entry.Line);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByMember = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.Member, b.Member, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByException = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = string.Compare(a.Exception, b.Exception, StringComparison.Ordinal);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByLastRepeatTime = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.LastRepeatTime.CompareTo(b.Entry.LastRepeatTime);
                return CompareByIdFallback(a, b, result);
            });
            var comparerByRepeatCount = Comparer<EntryBinding>.Create((a, b) =>
            {
                var result = a.Entry.RepeatCount.CompareTo(b.Entry.RepeatCount);
                return CompareByIdFallback(a, b, result);
            });

            SortedEntryList = new Dictionary<EntryContentType, SortedSet<EntryBinding>>
            {
                { EntryContentType.Id, new SortedSet<EntryBinding>(comparerById) },
                { EntryContentType.Timestamp, new SortedSet<EntryBinding>(comparerByTimestamp) },
                { EntryContentType.ThreadId, new SortedSet<EntryBinding>(comparerByThreadId) },
                { EntryContentType.Frame, new SortedSet<EntryBinding>(comparerByFrame) },
                { EntryContentType.Scene, new SortedSet<EntryBinding>(comparerByScene) },
                { EntryContentType.Level, new SortedSet<EntryBinding>(comparerByLevel) },
                { EntryContentType.Message, new SortedSet<EntryBinding>(comparerByMessage) },
                { EntryContentType.File, new SortedSet<EntryBinding>(comparerByFile) },
                { EntryContentType.Line, new SortedSet<EntryBinding>(comparerByLine) },
                { EntryContentType.Member, new SortedSet<EntryBinding>(comparerByMember) },
                { EntryContentType.Exception, new SortedSet<EntryBinding>(comparerByException) },
                { EntryContentType.LastRepeatTime, new SortedSet<EntryBinding>(comparerByLastRepeatTime) },
                { EntryContentType.RepeatCount, new SortedSet<EntryBinding>(comparerByRepeatCount) }
            };
        }

        public void AddEntry(EntryBinding entry)
        {
            if (entry == null)
                return;

            if (!string.IsNullOrEmpty(entry.Exception))
                HasExceptionEntry = true;

            if (entry.IsRepeated)
                HasRepeatedEntry = true;

            var existingEntryList = EntryList.Where(e => e.Equals(entry));
            if (existingEntryList.Any())
            {
                var existing = existingEntryList.First();
                foreach (var sortedSet in SortedEntryList.Values)
                    sortedSet.Remove(existing);

                existing.Entry.LastRepeatTime = entry.Entry.LastRepeatTime;
                existing.Entry.RepeatCount += entry.Entry.RepeatCount;
                HasRepeatedEntry = true;

                foreach (var sortedSet in SortedEntryList.Values)
                    sortedSet.Add(existing);
                return;
            }

            EntryList.Add(entry);
            foreach (var sortedSet in SortedEntryList.Values)
                sortedSet.Add(entry);
        }

        public static int CompareByIdFallback(EntryBinding a, EntryBinding b, int result)
        {
            return result != 0 ? result : CompareId(a, b);
        }

        public static int CompareId(EntryBinding a, EntryBinding b)
        {
            return a.Entry.Id.CompareTo(b.Entry.Id);
        }
    }
}
