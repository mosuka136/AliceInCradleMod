using System;

namespace BetterExperience.Patches
{
    internal enum PortraitPreviewReadiness { Loading, Ready, Failed }
    internal enum PortraitPreviewResult { None, Loading, Showing, Restored, Failed }

    /// <summary>临时显示会话；不改变控制面板的选择、请求或锁定状态。</summary>
    internal sealed class PortraitPreviewSession
    {
        internal const float Duration = 2f;
        internal readonly PortraitControlSession Control = new PortraitControlSession();
        internal PortraitSelection? Original { get; private set; }
        internal bool Active => Original.HasValue;
        internal bool Restoring { get; private set; }
        internal Exception Error { get; private set; }
        internal bool RestoreFailed { get; private set; }
        private float started;
        private float? shownAt;
        private long version;

        internal PortraitControlSession ControlFor(object owner, PortraitControlSession manual) =>
            Active && ReferenceEquals(Control.Owner, owner) ? Control : manual;

        internal void Begin(object owner, PortraitSelection original, PortraitSelection preview, float now)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (!PortraitControlLogic.IsMainPose(original.Pose) || !PortraitControlLogic.IsMainPose(preview.Pose))
                throw new ArgumentException("Invalid portrait preview pose.");
            version++;
            if (!Active || !ReferenceEquals(Control.Owner, owner)) Original = original;
            Control.Bind(owner);
            Control.Queue(preview, true, now, force: true);
            started = now;
            shownAt = null;
            Restoring = false;
            Error = null;
            RestoreFailed = false;
        }

        internal void Restore(float now)
        {
            if (!Active || Restoring) return;
            version++;
            Restoring = true;
            Control.Queue(Original.Value, false, now, force: true);
        }

        internal PortraitPreviewResult Tick(float now, Func<PortraitSelection, PortraitSelection?> prepare,
            Action<PortraitSelection, bool> apply, Func<PortraitPreviewReadiness> readiness)
        {
            if (!Active) return PortraitPreviewResult.None;
            if (!Restoring && shownAt.HasValue && now - shownAt.Value >= Duration) Restore(now);
            long requestVersion = version;
            // 替换内容就绪也是切换的前置条件，不能先切姿态再等待它覆盖原版。
            var result = Control.Tick(now, value =>
            {
                var prepared = prepare(value);
                if (Restoring || !prepared.HasValue || requestVersion != version) return prepared;
                var readyToApply = readiness();
                if (readyToApply == PortraitPreviewReadiness.Failed)
                    throw new InvalidOperationException("Replacement preview could not be prepared.");
                return readyToApply == PortraitPreviewReadiness.Ready ? prepared : null;
            }, apply);
            if (requestVersion != version) return PortraitPreviewResult.None;
            if (Restoring)
            {
                if (result == PortraitApplyResult.Loading) return PortraitPreviewResult.Loading;
                RestoreFailed = result == PortraitApplyResult.Failed;
                if (RestoreFailed) Error = Control.Error;
                bool failed = Error != null;
                Reset(clearError: false);
                return failed ? PortraitPreviewResult.Failed : PortraitPreviewResult.Restored;
            }
            if (result == PortraitApplyResult.Failed) Error = Control.Error;
            var ready = result == PortraitApplyResult.Failed ? PortraitPreviewReadiness.Failed
                : result == PortraitApplyResult.Loading ? PortraitPreviewReadiness.Loading : readiness();
            if (requestVersion != version) return PortraitPreviewResult.None;
            if (Error != null || ready == PortraitPreviewReadiness.Failed
                || (!shownAt.HasValue && now - started >= PortraitControlSession.LoadTimeout))
            {
                Error = Error ?? (ready == PortraitPreviewReadiness.Failed
                    ? (Exception)new InvalidOperationException("Replacement preview is no longer available.")
                    : new TimeoutException("Replacement preview loading timed out."));
                Restore(now);
                return PortraitPreviewResult.Loading;
            }
            if (ready != PortraitPreviewReadiness.Ready) return PortraitPreviewResult.Loading;
            if (!shownAt.HasValue) shownAt = now;
            return PortraitPreviewResult.Showing;
        }

        internal void Reset(bool clearError = true)
        {
            version++;
            Control.Reset();
            Original = null;
            Restoring = false;
            shownAt = null;
            if (clearError) { Error = null; RestoreFailed = false; }
        }
    }
}
