using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal sealed class ReplacementPreviewPose
    {
        internal PortraitSelection Selection;
        internal string SpineKey;
        internal string JsonKey;
    }

    internal static class ReplacementPreview
    {
        internal static List<ReplacementTarget> Layers(ReplacementSelection selection, ReplacementTarget target)
        {
            if (target == null || !selection.Authorizes(new[] { target.Owner }) || selection.Invalid(target.Identity))
                return new List<ReplacementTarget>();
            var layers = selection.Layers(target.Identity);
            if (!layers.Contains(target)) return new List<ReplacementTarget>();
            // 只在临时副本里把本次启用的层放到最后，正式配置和其他目标不变。
            return layers.Where(layer => !ReferenceEquals(layer, target)).Concat(new[] { target }).ToList();
        }

        // 同时启用多个包时优先后层；同一包优先当前姿态，再按姿态目录顺序选一个可显示的目标。
        internal static ReplacementPreviewPose Choose(IReadOnlyList<ReplacementTarget> targets,
            IReadOnlyList<ReplacementPreviewPose> poses, out ReplacementTarget target)
        {
            foreach (var package in targets.Where(value => value.Type == "spine").GroupBy(value => value.PackageId).Reverse())
            {
                foreach (var pose in poses)
                {
                    if (!PortraitControlLogic.IsMainPose(pose.Selection.Pose)) continue;
                    var candidate = package.FirstOrDefault(value => value.SpineKey == pose.SpineKey && value.JsonKey == pose.JsonKey);
                    if (candidate == null) continue;
                    target = candidate;
                    return pose;
                }
            }
            target = null;
            return null;
        }
    }
}
