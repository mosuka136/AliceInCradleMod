using nel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterExperience.Patches
{
    internal sealed class PortraitPoseEntry
    {
        internal UIEMOT Pose;
        internal List<PortraitSelection> Presets;
    }

    internal static class PortraitControlCatalog
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static readonly FieldInfo Emotions = typeof(UIPictureBase).GetField("AEmot", Fields);
        private static readonly FieldInfo Animations = typeof(UIPictureBodySpine).GetField("AAnim", Fields);
        private static readonly FieldInfo Skins = typeof(UIPictureBodySpine).GetField("OSkin", Fields);
        private static readonly FieldInfo Additional = typeof(UIPictureBodySpine)
            .GetNestedType("StateVariation", BindingFlags.NonPublic)?.GetField("st_add", Fields);

        internal static UIPictureBase.PrEmotion[] Read(UIPicture picture) =>
            Emotions?.GetValue(picture) as UIPictureBase.PrEmotion[];

        internal static bool HasBody(UIPictureBase.PrEmotion.BodyPaint paint) => paint?.Body != null
            && (paint.Body is UIPictureBodySpine spine ? spine.getViewer() != null : paint.AFrm != null && paint.AFrm.Length > 0);

        internal static List<PortraitPoseEntry> Build(UIPictureBase.PrEmotion[] emotions)
        {
            var result = new List<PortraitPoseEntry>();
            if (emotions == null) return result;
            foreach (var emotion in emotions)
            {
                if (emotion == null || !PortraitControlLogic.IsMainPose(emotion.emot_id)) continue;
                var paints = emotion.ODrawer.Where(pair => HasBody(pair.Value)).ToList();
                if (paints.Count == 0 || !HasBody(emotion.Default)) continue;
                var presets = paints.Select(pair => new PortraitSelection(emotion.emot_id, pair.Key)).ToList();
                foreach (var body in paints.Select(pair => pair.Value.Body).OfType<UIPictureBodySpine>().Distinct())
                {
                    presets.Add(new PortraitSelection(emotion.emot_id, body.get_base_state()));
                    var viewer = body.AViewerEntrySrc;
                    for (int i = 0; i < viewer.Length; i++)
                        presets.Add(new PortraitSelection(emotion.emot_id, viewer[i].emstate, viewer[i].emstate_add));
                }
                result.Add(new PortraitPoseEntry { Pose = emotion.emot_id, Presets = PortraitControlLogic.Presets(emotion.emot_id, presets) });
            }
            return result.OrderBy(entry => (int)entry.Pose).ToList();
        }

        internal static uint AdditionalMask(UIPictureBodyData body)
        {
            uint mask = 0;
            if (body.Alay_attr != null)
                foreach (var flag in body.Alay_attr) mask |= (uint)flag;
            if (body is UIPictureBodySpine)
            {
                if (Animations?.GetValue(body) is IEnumerable animations)
                    foreach (var variation in animations) mask |= ReadAdditional(variation);
                if (Skins?.GetValue(body) is IDictionary skins)
                    foreach (var variation in skins.Values) mask |= ReadAdditional(variation);
            }
            return mask & PortraitControlLogic.AdditionalMask;
        }

        private static uint ReadAdditional(object variation) => Additional == null || variation == null
            ? 0 : (uint)(UIPictureBase.EMSTATE_ADD)Additional.GetValue(variation);
    }
}
