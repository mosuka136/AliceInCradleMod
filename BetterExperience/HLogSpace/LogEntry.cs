using System;
using System.IO;
using System.Text;
using static BetterExperience.HLogSpace.HLog;

namespace BetterExperience.HLogSpace
{
    public class LogEntry
    {
        public int Id { get; }
        public string Timestamp { get; }
        public int ThreadId { get; }
        public int Frame { get; }
        public string Scene { get; }
        public LogLevel Level { get; }
        public string Message { get; }
        public string File { get; }
        public int Line { get; }
        public string Member { get; }
        public Exception Exception { get; }

        public LogEntry(int id, string timestamp, int threadId, int frame, string scene, LogLevel level, string message, string file, int line, string member, Exception exception)
        {
            Id = id;
            Timestamp = timestamp ?? "??:??:??.???";
            ThreadId = threadId;
            Frame = frame;
            Scene = scene ?? "?";
            Level = level;
            Message = message ?? string.Empty;
            File = file ?? string.Empty;
            Line = line;
            Member = member ?? string.Empty;
            Exception = exception;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(256);

            sb.Append('[').Append(Id).Append("] ")
              .Append(Timestamp).Append(" T").Append(ThreadId)
              .Append(" F").Append(Frame)
              .Append(" S=").Append(Scene)
              .Append(" ").Append(Level.ToString()).Append(" | ").Append(Message);

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
