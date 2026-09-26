using System;

namespace BetterExperience.Patches
{
    internal enum PortraitApplyResult { None, Loading, Applied, Failed }

    /// <summary>请求和锁定的状态机，资源准备与显示操作由主线程适配器提供。</summary>
    internal sealed class PortraitControlSession
    {
        private sealed class Request
        {
            internal long Version;
            internal PortraitSelection Selection;
            internal bool Lock;
            internal bool Force;
            internal float Started;
        }

        internal const float LoadTimeout = 15f;
        private Request pending;
        private long version;
        internal object Owner { get; private set; }
        internal PortraitSelection? Active { get; private set; }
        internal PortraitSelection? RequestedActive { get; private set; }
        internal PortraitSelection? Applying { get; private set; }
        internal bool Locked { get; private set; }
        internal bool HasPending => pending != null;
        internal bool LockRequested => pending != null ? pending.Lock : Locked;
        internal Exception Error { get; private set; }
        internal bool Protects(object owner) => owner != null && ReferenceEquals(Owner, owner) && (Locked || HasPending);
        internal bool Blocks(object owner) => Protects(owner) && !Applying.HasValue;
        internal PortraitSelection? OverrideFor(object owner) => !ReferenceEquals(Owner, owner) ? null : Applying ?? (Locked ? Active : null);

        internal void Bind(object owner)
        {
            if (ReferenceEquals(Owner, owner)) return;
            Reset();
            Owner = owner;
        }

        internal void Queue(PortraitSelection selection, bool lockAfter, float now, bool force = false)
        {
            if (Owner == null) throw new InvalidOperationException("Portrait is unavailable.");
            if (!PortraitControlLogic.IsMainPose(selection.Pose)) throw new ArgumentException("Invalid portrait pose.");
            pending = new Request { Version = ++version, Selection = selection, Lock = lockAfter, Force = force, Started = now };
            Error = null;
        }

        internal void CancelPending()
        {
            version++;
            pending = null;
        }

        internal PortraitApplyResult Tick(float now, Func<PortraitSelection, PortraitSelection?> prepare,
            Action<PortraitSelection, bool> apply)
        {
            var request = pending;
            if (request == null) return PortraitApplyResult.None;
            try
            {
                var effective = prepare(request.Selection);
                if (request.Version != version) return PortraitApplyResult.None;
                if (!effective.HasValue)
                {
                    if (now - request.Started >= LoadTimeout) throw new TimeoutException("Portrait resource loading timed out.");
                    return PortraitApplyResult.Loading;
                }
                Applying = effective;
                // 未锁定时游戏可能已经换姿态；仅已锁定且相同的请求可以省略重绘。
                if (!Locked || !Active.HasValue || !Active.Value.Equals(effective.Value) || request.Force)
                    apply(effective.Value, request.Force);
                if (request.Version != version) return PortraitApplyResult.None;
                Active = effective;
                RequestedActive = request.Selection;
                Locked = request.Lock;
                pending = null;
                return PortraitApplyResult.Applied;
            }
            catch (Exception ex)
            {
                if (request.Version != version) return PortraitApplyResult.None;
                Error = ex;
                pending = null;
                return PortraitApplyResult.Failed;
            }
            finally
            {
                Applying = null;
            }
        }

        internal void Reset()
        {
            CancelPending();
            Owner = null;
            Active = RequestedActive = Applying = null;
            Locked = false;
            Error = null;
        }
    }
}
