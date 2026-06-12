using System;
using System.IO;
using System.Text;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.HLogSpace
{
    public class LogEntry
    {
        public int Id { get; }
        public DateTime Timestamp { get; }
        public int ThreadId { get; }
        public int Frame { get; }
        public string Scene { get; }
        public LogLevel Level { get; }
        public string Message { get; }
        public string File { get; }
        public int Line { get; }
        public string Member { get; }
        public Exception Exception { get; }
        public DateTime LastRepeatTime { get; set; }
        public int RepeatCount { get; set; }
        public bool IsRepeated => RepeatCount > 1;

        public LogEntry(int id, DateTime timestamp, int threadId, int frame, string scene, LogLevel level, string message, string file, int line, string member, Exception exception)
        {
            Id = id;
            Timestamp = timestamp;
            ThreadId = threadId;
            Frame = frame;
            Scene = scene ?? "?";
            Level = level;
            Message = message ?? string.Empty;
            File = file ?? string.Empty;
            Line = line;
            Member = member ?? string.Empty;
            Exception = exception;

            LastRepeatTime = timestamp;
            RepeatCount = 1;
        }

        /// <summary>
        /// 更新日志的重复信息。当检测到新的日志与当前日志相同（通过 <see cref="Equals(LogEntry)"/> 方法比较）时，调用此方法来更新最后重复时间和重复次数。
        /// </summary>
        /// <param name="newRepeatTime">最后一次重复的时间</param>
        public void UpdateRepeat(DateTime newRepeatTime)
        {
            LastRepeatTime = newRepeatTime;
            RepeatCount++;
        }

        /// <summary>
        /// 判断两条日志是否相同。比较时不会考虑日志的 Id、时间戳、帧数、最后重复时间、重复次数等信息，而是只比较日志的内容等关键信息。
        /// </summary>
        /// <returns>如果两条日志相同则返回 <c>true</c>，否则返回 <c>false</c></returns>
        public bool Equals(LogEntry other)
        {
            if (other == null)
                return false;

            return ThreadId == other.ThreadId &&
                   Scene == other.Scene &&
                   Level == other.Level &&
                   Message == other.Message &&
                   File == other.File &&
                   Line == other.Line &&
                   Member == other.Member &&
                   Exception?.ToString() == other.Exception?.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is LogEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ThreadId.GetHashCode();
                hash = hash * 31 + (Scene?.GetHashCode() ?? 0);
                hash = hash * 31 + Level.GetHashCode();
                hash = hash * 31 + (Message?.GetHashCode() ?? 0);
                hash = hash * 31 + (File?.GetHashCode() ?? 0);
                hash = hash * 31 + Line.GetHashCode();
                hash = hash * 31 + (Member?.GetHashCode() ?? 0);
                hash = hash * 31 + (Exception?.Message.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(256);

            sb.Append('[').Append(Id).Append("] ")
              .Append(Timestamp.ToString("HH:mm:ss.fff"));

            if (IsRepeated)
                sb.Append('-').Append(LastRepeatTime.ToString("HH:mm:ss.fff"));

            sb.Append(" T").Append(ThreadId)
              .Append(" F").Append(Frame)
              .Append(" S=").Append(Scene)
              .Append(" ").Append(Level.ToString());

            if (IsRepeated)
                sb.Append(" x").Append(RepeatCount);

            sb.Append(" | ").Append(Message);

            if (!string.IsNullOrEmpty(Member) && !string.IsNullOrEmpty(Path.GetFileName(File)) && Line > 0)
            {
                sb.Append(" (").Append(Path.GetFileName(File))
                  .Append(':').Append(Line)
                  .Append(" ").Append(Member).Append(')');
            }

            if (Exception != null)
            {
                sb.AppendLine();
                sb.Append(Exception);
            }

            return sb.ToString();
        }
    }
}
