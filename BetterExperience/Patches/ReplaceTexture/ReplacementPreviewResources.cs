using BetterExperience.BLogSpace;
using nel;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XX;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal static partial class ReplacementRuntime
    {
        private sealed class PreviewBuild
        {
            internal ReplacementTarget Target;
            internal BetobetoManager.SvTexture Texture;
            internal int Revision;
            internal List<ReplacementTarget> Layers;
            internal SkeletonDataAsset Original;
            internal ReplacementWork<PreparedSpine> Work;
            internal SpineBundle Candidate;
        }

        private const string PreviewLoadKey = "_BetterExperience_Preview";
        private static readonly HashSet<BetobetoManager.SvTexture> previewHeld = new HashSet<BetobetoManager.SvTexture>();
        private static PreviewBuild previewBuild;

        internal static void CancelPendingResourcePreview() => CancelPreviewBuild();

        // 这里仅创建独立资源，不安装到 SvTexture，也不切换当前立绘或改动其材质。
        internal static PortraitPreviewReadiness PrepareResourcePreview(UIPictureBodySpine body,
            ReplacementTarget target, Material material)
        {
            if (!PreviewTargetEnabled(target)) return PortraitPreviewReadiness.Failed;
            var viewer = body?.getViewer();
            var texture = viewer?.getSvTexture();
            if (texture == null || material == null || texture.key != target.SpineKey
                || (viewer.replace_json_key ?? texture.MtiText.default_json_key) != target.JsonKey)
                return PortraitPreviewReadiness.Failed;
            if (previewBuild == null || !ReferenceEquals(previewBuild.Target, target)
                || previewBuild.Texture != texture || previewBuild.Revision != revision)
            {
                CancelPreviewBuild();
                var layers = ReplacementPreview.Layers(selection, target);
                if (layers.Count == 0) return PortraitPreviewReadiness.Failed;
                if (previewHeld.Add(texture))
                {
                    texture.MtiImage0.addLoadKey(PreviewLoadKey, false);
                    texture.MtiText.addLoadKey(PreviewLoadKey);
                }
                SpineViewer.prepareAtlasAssetsS(texture.MtiText, out var atlas, out var original, target.JsonKey);
                string json = original.skeletonJSON.text, atlasText = atlas.atlasFile.text;
                float scale = original.scale;
                string root = PatchInfo.ReplaceImagePath, sensitive = PatchInfo.ReplaceSensitiveImagePath;
                bool allowSensitive = selection.AllowSensitive;
                previewBuild = new PreviewBuild
                {
                    Target = target, Texture = texture, Revision = revision, Layers = layers, Original = original,
                    Work = new ReplacementWork<PreparedSpine>(token => ReplacementPreparation.Spine(
                        json, atlasText, scale, layers, root, sensitive, allowSensitive, token))
                };
            }
            if (previewBuild.Candidate != null) return PortraitPreviewReadiness.Ready;
            if (previewBuild.Work == null) return PreviewReadiness(body, target);
            if (!previewBuild.Work.IsCompleted || !ClaimUpload()) return PortraitPreviewReadiness.Loading;
            previewBuild.Work.TryTake(out var prepared, out var error);
            previewBuild.Work = null;
            if (error != null) throw error;
            if (previewBuild.Original == null) throw new InvalidOperationException("Original preview data was released.");
            previewBuild.Candidate = Build(texture, previewBuild.Layers, material, prepared, previewBuild.Original);
            return PortraitPreviewReadiness.Ready;
        }

        internal static void ActivateResourcePreview(UIPictureBodySpine body, ReplacementTarget target)
        {
            var texture = body?.getViewer()?.getSvTexture();
            if (!PreviewTargetEnabled(target) || previewBuild?.Candidate == null || previewBuild.Texture != texture
                || !ReferenceEquals(previewBuild.Target, target) || previewBuild.Revision != revision)
                throw new InvalidOperationException("Preview resources are not ready for this selection.");
            RemoveResourcePreview();
            if (!spineStates.TryGetValue(texture, out var state)) spineStates.Add(texture, state = new SpineState());
            var previous = state.Shown;
            state.Preview = previewBuild.Candidate;
            state.PreviewTarget = target;
            previewBuild.Candidate = null;
            Register(body.getViewer());
            RebindShown(texture, state, previous);
        }

        // 恢复姿态前先准备正常排序的组合；即使与预览使用同一姿态，也不能把临时优先级留下。
        internal static bool PreparePreviewReturn(UIPictureBodySpine body, Material material)
        {
            var viewer = body?.getViewer();
            var texture = viewer?.getSvTexture();
            if (texture == null) return true;
            string key = viewer.replace_json_key ?? texture.MtiText.default_json_key;
            Change(texture, key, material, viewer);
            return spineStates.TryGetValue(texture, out var state) && state.Pending == null
                && state.Attempt == revision && state.JsonKey == key;
        }

        internal static void RemoveResourcePreview()
        {
            foreach (var pair in spineStates.Where(pair => pair.Value.Preview != null).ToArray())
                RemoveResourcePreview(pair.Key, pair.Value);
        }

        private static void RemoveResourcePreview(BetobetoManager.SvTexture texture, SpineState state)
        {
            var old = state.Preview;
            state.Preview = null;
            state.PreviewTarget = null;
            try { RebindShown(texture, state, old); }
            finally { retired.Add(old); }
        }

        private static void RevokeUnauthorizedPreviews()
        {
            foreach (var pair in spineStates.Where(pair => pair.Value.Preview != null).ToArray())
                if (!PreviewTargetEnabled(pair.Value.PreviewTarget)
                    || !CanRetain(pair.Value.Preview.Sources, pair.Value.PreviewTarget.Identity))
                {
                    RemoveResourcePreview(pair.Key, pair.Value);
                    // 恢复原姿态可能还需加载，先撤销正在显示的临时资源。
                    foreach (var viewer in LiveViewers().Where(viewer => viewer.enabled && viewer.getSvTexture() == pair.Key))
                    {
                        try { Replay(viewer); }
                        catch (Exception ex) { BLog.Error("Could not redraw a revoked resource preview.", ex); }
                    }
                }
        }

        private static void CancelPreviewBuild()
        {
            var pending = previewBuild;
            previewBuild = null;
            pending?.Work?.Dispose();
            pending?.Candidate?.Dispose();
        }

        internal static void FinishResourcePreview()
        {
            try { RemoveResourcePreview(); }
            finally
            {
                CancelPreviewBuild();
                foreach (var texture in previewHeld.ToArray())
                {
                    previewHeld.Remove(texture);
                    try { texture.MtiImage0.remLoadKey(PreviewLoadKey); }
                    finally { texture.MtiText.remLoadKey(PreviewLoadKey); }
                }
            }
        }
    }
}
