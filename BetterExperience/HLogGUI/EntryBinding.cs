using BetterExperience.HLogSpace;

namespace BetterExperience.HLogGUI
{
    public class EntryBinding
    {
        public LogEntry Entry { get; }
        public string Id => Entry.Id.ToString();
        public string Timestamp => Entry.Timestamp.ToString("HH:mm:ss.fff");
        public string ThreadId => Entry.ThreadId.ToString();
        public string Frame => Entry.Frame.ToString();
        public string Scene => Entry.Scene;
        public string Level => Entry.Level.ToString();
        public string Message => Entry.Message;
        public string File => Entry.File;
        public string Line => Entry.Line.ToString();
        public string Member => Entry.Member;
        public string Exception => Entry.Exception?.ToString() ?? string.Empty;
        public string LastRepeatTime => Entry.LastRepeatTime.ToString("HH:mm:ss.fff");
        public string RepeatCount => Entry.RepeatCount.ToString();
        public bool IsRepeated => Entry.IsRepeated;

        public EntryBinding(LogEntry entry)
        {
            Entry = entry;
        }

        public bool Equals(EntryBinding other)
        {
            if (other == null)
                return false;
            return Entry.Equals(other.Entry);
        }

        public override bool Equals(object obj)
        {
            return obj is EntryBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Entry.GetHashCode();
        }

        public override string ToString()
        {
            return Entry.ToString();
        }
    }
}
