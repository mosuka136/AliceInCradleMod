using System;
using System.Threading;
using System.Threading.Tasks;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal static class ReplacementWorker
    {
        // 同时只运行一个重型准备任务，避免多个立绘争抢 CPU 和产生大批临时明文。
        internal static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
    }

    /// <summary>一次性后台任务。主线程只轮询完成状态，取消后不再提供结果。</summary>
    internal sealed class ReplacementWork<T> : IDisposable where T : class
    {
        private sealed class Result
        {
            internal T Value;
            internal Exception Error;
        }

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private Task<Result> task;
        private bool disposed;
        internal bool IsCompleted => !disposed && task != null && task.IsCompleted;

        internal ReplacementWork(Func<CancellationToken, T> prepare)
        {
            if (prepare == null) throw new ArgumentNullException(nameof(prepare));
            var token = cancellation.Token;
            task = Task.Run(async () =>
            {
                bool entered = false;
                try
                {
                    await ReplacementWorker.Gate.WaitAsync(token).ConfigureAwait(false);
                    entered = true;
                    token.ThrowIfCancellationRequested();
                    T value = prepare(token);
                    token.ThrowIfCancellationRequested();
                    return new Result { Value = value };
                }
                catch (Exception ex) { return new Result { Error = ex }; }
                finally { if (entered) ReplacementWorker.Gate.Release(); }
            });
        }

        internal bool TryTake(out T value, out Exception error)
        {
            value = null;
            error = null;
            if (!IsCompleted) return false;
            var result = task.GetAwaiter().GetResult();
            value = result.Value;
            error = result.Error;
            Dispose();
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            cancellation.Cancel();
            var pending = task;
            task = null;
            if (pending.IsCompleted) cancellation.Dispose();
            else pending.ContinueWith(_ => cancellation.Dispose(), TaskScheduler.Default);
        }
    }
}
