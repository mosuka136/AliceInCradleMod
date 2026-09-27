using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using BetterExperience.Patches.ReplaceTexture;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterExperience.Patches
{
    internal static partial class PortraitControlRuntime
    {
        private static readonly PortraitPreviewSession Preview = new PortraitPreviewSession();
        private static readonly object PreviewNoticeOwner = new object();
        private static ReplacementTarget previewTarget;
        private static UIPictureBodySpine previewBody;

        private static PortraitControlSession ControlFor(UIPictureBase picture) =>
            Preview.ControlFor(picture, Session);

        internal static void OnReplacementSelectionChanged(IReadOnlyList<ReplacementTarget> targets)
        {
            try
            {
                if (targets.Count == 0 || Session.HasPending || !TryPicture(out var picture))
                {
                    EndReplacementPreview(true);
                    return;
                }
                EnsureCatalog();
                var original = Preview.Active && ReferenceEquals(Preview.Control.Owner, picture) ? Preview.Original.Value
                    : new PortraitSelection(picture.getCurEmot(), picture.getCurrentState(), picture.getAdditionalState());
                if (!PortraitControlLogic.IsMainPose(original.Pose)) return;
                var poses = new List<ReplacementPreviewPose>();
                foreach (var requested in new[] { original }.Concat(catalog.SelectMany(entry => entry.Presets)).Distinct())
                {
                    var state = requested.State;
                    var emotion = picture.GetEmot(requested.Pose, ref state);
                    var paint = emotion?.Get(state);
                    var body = (paint?.Body?.getReplaceTerm() ?? paint?.Body) as UIPictureBodySpine;
                    var viewer = body?.getViewer();
                    var texture = viewer?.getSvTexture();
                    if (texture == null) continue;
                    poses.Add(new ReplacementPreviewPose
                    {
                        Selection = new PortraitSelection(emotion.emot_id, state, requested.Additional),
                        SpineKey = texture.key,
                        JsonKey = viewer.replace_json_key ?? texture.MtiText.default_json_key
                    });
                }
                var chosen = ReplacementPreview.Choose(targets, poses, out var target);
                if (chosen == null || !ReplacementRuntime.PreviewTargetEnabled(target)) { EndReplacementPreview(true); return; }
                ReplacementRuntime.CancelPendingResourcePreview();
                previewTarget = target;
                previewBody = null;
                Preview.Begin(picture, original, chosen.Selection, Time.unscaledTime);
                NoticeGUI.SetStatus(PreviewNoticeOwner, "正在准备资源立绘预览…");
            }
            catch (Exception ex)
            {
                BLog.Error("Could not start the replacement portrait preview.", ex);
                EndReplacementPreview(true);
            }
        }

        private static bool UpdateReplacementPreview()
        {
            if (!Preview.Active) return false;
            var picture = Preview.Control.Owner as UIPicture;
            if (ConfigManager.EnableBetterExperience?.Value != true)
            {
                Stop(true);
                return false;
            }
            if (picture == null || picture.Gob == null)
            {
                EndReplacementPreview(false);
                return false;
            }
            if (!TryPicture(out var current))
            {
                EndReplacementPreview(true);
                return false;
            }
            if (!ReferenceEquals(current, picture))
            {
                EndReplacementPreview(false);
                return false;
            }
            try
            {
                if (!ReplacementRuntime.PreviewTargetEnabled(previewTarget)) Preview.Restore(Time.unscaledTime);
                var result = Preview.Tick(Time.unscaledTime, value => PrepareReplacementPreview(picture, value),
                    (value, force) => ApplyReplacementPreview(picture, value, force),
                    () => ReplacementRuntime.PrepareResourcePreview(previewBody, previewTarget, picture.MtrSpine));
                if (result == PortraitPreviewResult.Showing)
                    NoticeGUI.SetStatus(PreviewNoticeOwner, "资源立绘预览：" + PortraitControlLogic.Display(Preview.Control.Active.Value.Pose.ToString()) + "（2 秒后恢复）");
                if (!Preview.Active)
                {
                    bool restoreGame = Preview.RestoreFailed && !Session.Locked;
                    FinishReplacementPreview();
                    if (restoreGame) RestoreGamePortrait(picture);
                    if (result == PortraitPreviewResult.Failed)
                    {
                        BLog.Warn("Replacement portrait preview failed: " + Preview.Error?.Message);
                        NoticeGUI.Show("资源预览失败，已结束预览。", 4f, PreviewNoticeOwner);
                    }
                }
            }
            catch (Exception ex)
            {
                BLog.Error("Replacement portrait preview failed.", ex);
                EndReplacementPreview(true);
            }
            return Preview.Active;
        }

        private static void EndReplacementPreview(bool restore)
        {
            if (!Preview.Active) return;
            var picture = Preview.Control.Owner as UIPicture;
            bool restoreGame = false;
            try
            {
                if (restore && picture != null && picture.Gob != null && picture.gob_prepared)
                {
                    Preview.Restore(Time.unscaledTime);
                    Preview.Tick(Time.unscaledTime, value => PrepareReplacementPreview(picture, value),
                        (value, force) => ApplyReplacementPreview(picture, value, force), () => PortraitPreviewReadiness.Ready);
                    if (Preview.Active) return;
                    restoreGame = Preview.RestoreFailed && !Session.Locked;
                }
                else Preview.Reset();
            }
            catch (Exception ex)
            {
                Preview.Reset();
                BLog.Error("Could not restore the portrait after a resource preview.", ex);
                restoreGame = picture != null && picture.Gob != null && picture.gob_prepared && !Session.Locked;
            }
            FinishReplacementPreview();
            if (restoreGame) RestoreGamePortrait(picture);
        }

        private static void FinishReplacementPreview()
        {
            ReplacementRuntime.FinishResourcePreview();
            previewTarget = null;
            previewBody = null;
            NoticeGUI.RemoveStatus(PreviewNoticeOwner);
            // 原控制会话一直保留；结束预览后重新应用它的锁定状态。
            refreshRequired = true;
        }

        private static PortraitSelection? PrepareReplacementPreview(UIPicture picture, PortraitSelection requested)
        {
            if (Preview.Restoring) ReplacementRuntime.CancelPendingResourcePreview();
            var prepared = Prepare(picture, requested);
            if (!prepared.HasValue) return null;
            var state = prepared.Value.State;
            var paint = picture.GetEmot(prepared.Value.Pose, ref state).Get(state);
            var body = (paint.Body.getReplaceTerm() ?? paint.Body) as UIPictureBodySpine;
            if (Preview.Restoring)
                return ReplacementRuntime.PreparePreviewReturn(body, picture.MtrSpine) ? prepared : null;
            previewBody = body;
            return prepared;
        }

        private static void ApplyReplacementPreview(UIPicture picture, PortraitSelection value, bool force)
        {
            if (Preview.Restoring) ReplacementRuntime.RemoveResourcePreview();
            else ReplacementRuntime.ActivateResourcePreview(previewBody, previewTarget);
            Apply(picture, value, force);
        }
    }
}
