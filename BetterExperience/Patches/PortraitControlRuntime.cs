using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    /// <summary>仅在 Unity 主线程控制当前 HUD 立绘，不写入角色状态或存档。</summary>
    internal static partial class PortraitControlRuntime
    {
        private static readonly PortraitControlSession Session = new PortraitControlSession();
        private static readonly object NoticeOwner = new object();
        private static readonly FieldInfo PreviousPose = AccessTools.Field(typeof(UIPictureBase), "pre_emot");
        private static readonly FieldInfo PreviousAdditional = AccessTools.Field(typeof(UIPictureBase), "pre_sta");
        private static readonly FieldInfo NextFade = AccessTools.Field(typeof(UIPictureFader), "Next");
        private static readonly FieldInfo GroundTime = AccessTools.Field(typeof(UIPictureFader), "t_ground");
        private static readonly FieldInfo FadeTime = AccessTools.Field(typeof(UIPictureFader), "t");
        private static readonly FieldInfo FadeLockTime = AccessTools.Field(typeof(UIPictureFader), "t_lock");
        private static readonly FieldInfo TimeoutSkip = AccessTools.Field(typeof(UIPictureFader), "timeout_skip");
        private static List<PortraitPoseEntry> catalog = new List<PortraitPoseEntry>();
        private static UIPictureBase.PrEmotion[] catalogSource;
        private static UIPicture catalogOwner;
        private static string filter = "";
        private static PortraitSelection? draft;
        private static List<PortraitPoseEntry> shownPoses = new List<PortraitPoseEntry>();
        private static List<PortraitSelection> shownPresets = new List<PortraitSelection>();
        private static PortraitSelection? rollbackOverride;
        private static int resourceRevision;
        private static byte sensitiveLevel;
        private static bool refreshRequired;

        internal static bool Blocks(UIPictureBase picture) => ControlFor(picture).Blocks(picture);
        internal static bool Protects(UIPictureBase picture) => ControlFor(picture).Protects(picture);
        internal static bool LocksAnimation(UIPicture picture)
        {
            var control = ControlFor(picture);
            return control.Protects(picture) && (control.Locked || ReferenceEquals(control, Preview.Control));
        }

        internal static bool TryAdditional(UIPicture picture, out UIPictureBase.EMSTATE_ADD value)
        {
            var control = ControlFor(picture);
            var selection = ReferenceEquals(control.Owner, picture) ? rollbackOverride ?? control.OverrideFor(picture) : null;
            value = selection?.Additional ?? 0;
            return selection.HasValue;
        }

        private static bool TryPicture(out UIPicture picture)
        {
            picture = null;
            return ConfigManager.EnableBetterExperience?.Value == true && UIBase.Instance != null
                && UIBase.Instance.getCurrentPict(out picture) && picture.gob_prepared && picture.Gob != null
                && picture.Pr != null && picture.FDCon != null && PortraitControlCatalog.Read(picture) != null;
        }

        private static void EnsureCatalog()
        {
            if (!TryPicture(out var picture)) return;
            var source = PortraitControlCatalog.Read(picture);
            if (ReferenceEquals(catalogOwner, picture) && ReferenceEquals(catalogSource, source)) return;
            catalog = PortraitControlCatalog.Build(source);
            catalogSource = source;
            catalogOwner = picture;
            if (!draft.HasValue || !catalog.Any(entry => entry.Pose == draft.Value.Pose))
            {
                var current = catalog.FirstOrDefault(entry => entry.Pose == picture.getCurEmot()) ?? catalog.FirstOrDefault();
                draft = current == null ? (PortraitSelection?)null : PortraitControlLogic.Editable(new PortraitSelection(
                    current.Pose, picture.getCurrentState(), picture.getAdditionalState()));
            }
        }

        internal static string GetFilter() => filter;
        internal static void SetFilter(string value) => filter = (value ?? "").Trim();

        internal static List<(string Display, bool Selected)> GetPoses()
        {
            EnsureCatalog();
            shownPoses = TryPicture(out _) ? catalog.Where(entry => DebugGiveLogic.MatchesFilter(
                PortraitControlLogic.Display(entry.Pose.ToString()), entry.Pose.ToString(), filter)).ToList() : new List<PortraitPoseEntry>();
            return shownPoses.Select(entry => (PortraitControlLogic.Display(entry.Pose.ToString()), draft?.Pose == entry.Pose)).ToList();
        }

        internal static void SetPoses(List<(string Display, bool Selected)> rows)
        {
            var labels = shownPoses.Select(entry => PortraitControlLogic.Display(entry.Pose.ToString())).ToList();
            if (!PortraitControlLogic.ValidRows(rows, labels)) return;
            int index = PortraitControlLogic.SelectOne(rows, labels, shownPoses.FindIndex(entry => draft?.Pose == entry.Pose));
            if (index < 0)
            {
                EndReplacementPreview(true);
                draft = null;
                Session.CancelPending();
                return;
            }
            var next = shownPoses[index].Presets.First();
            if (draft?.Pose == next.Pose) return;
            draft = next;
            Changed();
        }

        internal static List<(string Display, bool Selected)> GetPresets()
        {
            EnsureCatalog();
            shownPresets = TryPicture(out _) && draft.HasValue
                ? catalog.FirstOrDefault(entry => entry.Pose == draft.Value.Pose)?.Presets ?? new List<PortraitSelection>()
                : new List<PortraitSelection>();
            return shownPresets.Select(value => (PortraitControlLogic.Describe(value), draft.HasValue && draft.Value.Equals(value))).ToList();
        }

        internal static void SetPresets(List<(string Display, bool Selected)> rows)
        {
            var labels = shownPresets.Select(PortraitControlLogic.Describe).ToList();
            int previous = shownPresets.FindIndex(value => draft.HasValue && draft.Value.Equals(value));
            int index = PortraitControlLogic.SelectOne(rows, labels, previous);
            if (index < 0 || index == previous || !draft.HasValue || shownPresets[index].Pose != draft.Value.Pose) return;
            draft = shownPresets[index];
            Changed();
        }

        internal static List<(string Display, bool Selected)> GetStates() => PortraitControlLogic.FlagRows(
            typeof(UIPictureBase.EMSTATE), PortraitControlLogic.MainMask, (uint)(draft?.State ?? 0));
        internal static List<(string Display, bool Selected)> GetAdditional() => PortraitControlLogic.FlagRows(
            typeof(UIPictureBase.EMSTATE_ADD), PortraitControlLogic.AdditionalMask, (uint)(draft?.Additional ?? 0));

        internal static void SetStates(List<(string Display, bool Selected)> rows)
        {
            if (!draft.HasValue) return;
            var old = draft.Value;
            var state = (UIPictureBase.EMSTATE)PortraitControlLogic.ReadFlags(typeof(UIPictureBase.EMSTATE),
                PortraitControlLogic.MainMask, (uint)old.State, rows);
            if (state == old.State) return;
            draft = new PortraitSelection(old.Pose, state, old.Additional);
            Changed();
        }

        internal static void SetAdditional(List<(string Display, bool Selected)> rows)
        {
            if (!draft.HasValue) return;
            var old = draft.Value;
            var state = (UIPictureBase.EMSTATE_ADD)PortraitControlLogic.ReadFlags(typeof(UIPictureBase.EMSTATE_ADD),
                PortraitControlLogic.AdditionalMask, (uint)old.Additional, rows);
            if (state == old.Additional) return;
            draft = new PortraitSelection(old.Pose, old.State, state);
            Changed();
        }

        private static void Changed()
        {
            EndReplacementPreview(true);
            if (Session.LockRequested) Queue(true);
            else Session.CancelPending();
        }

        internal static bool GetLocked() => Session.LockRequested;
        internal static void SetLocked(bool value)
        {
            if (value) Queue(true);
            else Stop(restore: true, clearSelection: false);
        }

        internal static void ApplyOnce(bool value)
        {
            if (value) Queue(Session.LockRequested);
        }

        private static void Queue(bool lockAfter)
        {
            EndReplacementPreview(true);
            try
            {
                EnsureCatalog();
                if (!TryPicture(out var picture) || !draft.HasValue)
                {
                    NoticeGUI.Show("请在读档后选择立绘姿态。", owner: NoticeOwner);
                    return;
                }
                // 明确的面板操作优先于尚未完成的预览恢复。
                EndReplacementPreview(false);
                if (!ReferenceEquals(Session.Owner, picture))
                {
                    Stop(restore: true, clearSelection: false);
                    Session.Bind(picture);
                }
                resourceRevision = ReplacementRuntime.Revision;
                sensitiveLevel = X.sensitive_level;
                Session.Queue(draft.Value, lockAfter, Time.unscaledTime);
                Tick(picture);
            }
            catch (Exception ex) { ReportFailure(ex); }
        }

        internal static void Update()
        {
            if (UpdateReplacementPreview()) return;
            if (Session.Owner == null) return;
            if (ConfigManager.EnableBetterExperience?.Value != true)
            {
                Stop(true);
                return;
            }
            var picture = Session.Owner as UIPicture;
            if (picture == null || picture.Gob == null)
            {
                Stop(false);
                return;
            }
            // 换地图的短暂空窗保留目标；真正的读档、销毁由生命周期补丁清理。
            if (!TryPicture(out var current)) return;
            if (!ReferenceEquals(current, picture))
            {
                Stop(true);
                return;
            }
            try
            {
                if (resourceRevision != ReplacementRuntime.Revision || sensitiveLevel != X.sensitive_level
                    || !ReferenceEquals(catalogSource, PortraitControlCatalog.Read(picture)))
                {
                    resourceRevision = ReplacementRuntime.Revision;
                    sensitiveLevel = X.sensitive_level;
                    EnsureCatalog();
                    refreshRequired = true;
                }
                if (refreshRequired && Session.Locked && !Session.HasPending && Session.RequestedActive.HasValue)
                {
                    Session.Queue(Session.RequestedActive.Value, true, Time.unscaledTime, force: true);
                    refreshRequired = false;
                }
                Tick(picture);
            }
            catch (Exception ex) { Session.CancelPending(); ReportFailure(ex); }
        }

        private static void Tick(UIPicture picture)
        {
            var result = Session.Tick(Time.unscaledTime, value => Prepare(picture, value),
                (value, force) => Apply(picture, value, force));
            if (result == PortraitApplyResult.Loading)
                NoticeGUI.SetStatus(NoticeOwner, "立绘资源加载中…");
            else if (result == PortraitApplyResult.Applied)
            {
                draft = PortraitControlLogic.Editable(Session.Active.Value);
                string message = "已生效：" + PortraitControlLogic.Describe(Session.Active.Value);
                if (!PortraitControlLogic.Editable(Session.RequestedActive.Value).Equals(draft.Value))
                    message += "（已按姿态支持范围和游戏显示模式调整）";
                NoticeGUI.RemoveStatus(NoticeOwner);
                NoticeGUI.Show(message, 6f, NoticeOwner);
            }
            else if (result == PortraitApplyResult.Failed)
                ReportFailure(Session.Error);
        }

        private static PortraitSelection? Prepare(UIPicture picture, PortraitSelection requested)
        {
            if (!PortraitControlLogic.IsMainPose(requested.Pose)) throw new ArgumentException("Invalid pose.");
            var emotions = PortraitControlCatalog.Read(picture);
            if (emotions == null || (int)requested.Pose >= emotions.Length || !PortraitControlCatalog.HasBody(emotions[(int)requested.Pose]?.Default))
                throw new InvalidOperationException("Portrait resource is unavailable.");
            var editable = PortraitControlLogic.Editable(requested);
            var state = editable.State;
            var emotion = picture.GetEmot(editable.Pose, ref state);
            var paint = emotion.Get(state);
            if (!PortraitControlCatalog.HasBody(paint)) throw new InvalidOperationException("Portrait state has no drawing resource.");
            var body = paint.Body.getReplaceTerm() ?? paint.Body;
            // isPreparedResource 自身会发起异步纹理加载；提前 prepareMaterial 会改动当前共用材质。
            if (!body.isPreparedResource()) return null;
            uint mainMask = (uint)(emotion.avail_state | body.variation_bits | UIPictureBase.EMSTATE.SP_SENSITIVE);
            state = (UIPictureBase.EMSTATE)((uint)state & mainMask);
            var additional = (UIPictureBase.EMSTATE_ADD)((uint)editable.Additional & PortraitControlCatalog.AdditionalMask(body));
            if (X.sensitive_level > 0) additional |= UIPictureBase.EMSTATE_ADD.SENSITIVE;
            if (X.sensitive_level > 1) additional |= UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
            return new PortraitSelection(emotion.emot_id, state, additional);
        }

        private static void Apply(UIPicture picture, PortraitSelection selection, bool force)
        {
            var old = new PortraitSelection(picture.getCurEmot(), picture.getCurrentState(),
                (UIPictureBase.EMSTATE_ADD)PreviousAdditional.GetValue(picture));
            try
            {
                if (force) PreviousPose.SetValue(picture, UIEMOT._OFFLINE);
                var result = picture.changeEmotIn(selection.Pose, selection.State, null, UIPictureFader.UIP_RES.IMMEDIATE | UIPictureFader.UIP_RES.STATE_ABSOLUTE);
                if ((result & (UIPictureFader.UIP_RES.ERROR | UIPictureFader.UIP_RES.NOW_LOADING)) != 0)
                    throw new InvalidOperationException("Portrait could not be applied after preparation.");
                ClearFader(picture);
            }
            catch
            {
                // 切换异常可能发生在 closeEmot 之后，强制重建旧姿态以避免留下空白网格。
                try
                {
                    if (PortraitControlLogic.IsMainPose(old.Pose))
                    {
                        rollbackOverride = old;
                        PreviousPose.SetValue(picture, UIEMOT._OFFLINE);
                        picture.changeEmotIn(old.Pose, old.State, null, UIPictureFader.UIP_RES.IMMEDIATE);
                    }
                }
                finally { rollbackOverride = null; }
                throw;
            }
        }

        private static void ClearFader(UIPicture picture)
        {
            var fader = picture.FDCon;
            if (fader == null) return;
            NextFade.SetValue(fader, null);
            GroundTime.SetValue(fader, 0f);
            FadeTime.SetValue(fader, 0f);
            FadeLockTime.SetValue(fader, 0f);
            TimeoutSkip.SetValue(fader, false);
            picture.ground_level = 0f;
        }

        internal static void Stop(bool restore, bool clearSelection = true)
        {
            var picture = Session.Owner as UIPicture ?? Preview.Control.Owner as UIPicture;
            bool touched = Session.HasPending || Session.Active.HasValue || Preview.Active;
            EndReplacementPreview(false);
            NoticeGUI.Clear(PreviewNoticeOwner);
            Session.Reset();
            refreshRequired = false;
            rollbackOverride = null;
            NoticeGUI.Clear(NoticeOwner);
            if (clearSelection)
            {
                draft = null;
                filter = "";
                catalog.Clear();
                shownPoses.Clear();
                shownPresets.Clear();
                catalogOwner = null;
                catalogSource = null;
            }
            if (!restore || !touched || picture == null || picture.Gob == null || !picture.gob_prepared) return;
            RestoreGamePortrait(picture);
        }

        private static void RestoreGamePortrait(UIPicture picture)
        {
            try
            {
                ClearFader(picture);
                picture.FDCon.Blur();
                var previous = picture.getCurEmot();
                PreviousPose.SetValue(picture, UIEMOT._OFFLINE);
                PreviousAdditional.SetValue(picture, UIPictureBase.EMSTATE_ADD._NO_CHECK);
                try { picture.changeEmotDefault(true, true); }
                finally
                {
                    // 事件可能暂时禁止默认姿态。等待 recheck 期间仍保留可绘制的旧索引。
                    if (picture.getCurEmot() == UIEMOT._OFFLINE) PreviousPose.SetValue(picture, previous);
                }
                picture.recheck(0, 10);
            }
            catch (Exception ex) { BLog.Error("Failed to restore the game portrait.", ex); }
        }

        internal static void OnDestroyed(UIPicture picture)
        {
            if (ReferenceEquals(Session.Owner, picture) || ReferenceEquals(catalogOwner, picture)
                || ReferenceEquals(Preview.Control.Owner, picture)) Stop(false);
        }

        internal static void OnScriptReload(UIPictureBase picture)
        {
            if (ReferenceEquals(Preview.Control.Owner, picture)) EndReplacementPreview(true);
            if (ReferenceEquals(Session.Owner, picture)) refreshRequired = true;
        }

        private static void ReportFailure(Exception ex)
        {
            BLog.Error("Portrait control failed; keeping the last successful selection.", ex);
            NoticeGUI.RemoveStatus(NoticeOwner);
            NoticeGUI.Show("立绘切换失败，保留原显示。" + (ex is TimeoutException ? "资源加载超时。" : "请查看模组日志。"), 6f, NoticeOwner);
        }
    }
}
