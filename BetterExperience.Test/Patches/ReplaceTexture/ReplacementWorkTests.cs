using BetterExperience.Patches.ReplaceTexture;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ReplacementWorkTests
    {
        [Fact]
        public async Task Work_DoesNotBlockCallerAndDeliversResultOnlyOnce()
        {
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var work = new ReplacementWork<string>(token => { entered.Set(); release.Wait(token); return "ready"; });
            try
            {
                Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
                Assert.False(work.IsCompleted);
                Assert.False(work.TryTake(out _, out _));
                release.Set();
                await Complete(work);
                Assert.True(work.TryTake(out var value, out var error));
                Assert.Equal("ready", value);
                Assert.Null(error);
                Assert.False(work.TryTake(out _, out _));
            }
            finally { release.Set(); }
        }

        [Fact]
        public async Task Work_CancelledOldResultCannotReplaceNewSelection()
        {
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var old = new ReplacementWork<string>(_ => { entered.Set(); release.Wait(TimeSpan.FromSeconds(5)); return "old"; });
            try
            {
                Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
                old.Dispose();
                using var current = new ReplacementWork<string>(_ => "new");
                release.Set();
                await Complete(current);
                Assert.False(old.TryTake(out _, out _));
                Assert.True(current.TryTake(out var result, out var error));
                Assert.Null(error);
                Assert.Equal("new", result);
            }
            finally { release.Set(); }
        }

        [Fact]
        public async Task Worker_SerializesHeavyWorkAndDoesNotExecuteCancelledQueuedWork()
        {
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var first = new ReplacementWork<string>(token => { entered.Set(); release.Wait(token); return "first"; });
            int calls = 0;
            try
            {
                Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
                using var cancelled = new ReplacementWork<byte[]>(_ => { Interlocked.Increment(ref calls); return new byte[1]; });
                cancelled.Dispose();
                using var next = new ReplacementWork<string>(_ => "next");
                release.Set();
                await Complete(next);
                Assert.Equal(0, calls);
                Assert.True(next.TryTake(out var value, out _));
                Assert.Equal("next", value);
            }
            finally { release.Set(); }
        }

        [Fact]
        public async Task Work_FailureIsReturnedWithoutPoisoningSubsequentRequests()
        {
            using var failing = new ReplacementWork<string>(_ => throw new InvalidDataException("damaged"));
            await Complete(failing);
            Assert.True(failing.TryTake(out var result, out var error));
            Assert.Null(result);
            Assert.IsType<InvalidDataException>(error);
            using var next = new ReplacementWork<string>(_ => "recovered");
            await Complete(next);
            Assert.True(next.TryTake(out var recovered, out var nextError));
            Assert.Equal("recovered", recovered);
            Assert.Null(nextError);
        }

        [Fact]
        public async Task Work_DisposingCompletedResultPreventsLaterApplication()
        {
            using var work = new ReplacementWork<byte[]>(_ => new byte[20]);
            await Complete(work);
            work.Dispose();
            Assert.False(work.TryTake(out _, out _));
        }

        internal static async Task Complete<T>(ReplacementWork<T> work) where T : class
        {
            var timeout = DateTime.UtcNow.AddSeconds(10);
            while (!work.IsCompleted && DateTime.UtcNow < timeout) await Task.Delay(10);
            Assert.True(work.IsCompleted, "Background resource preparation did not complete.");
        }
    }
}
