using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal sealed class SpineCompositionResult
    {
        internal string Json;
        internal readonly Dictionary<string, string> BoneMap = new Dictionary<string, string>(StringComparer.Ordinal);
        internal ReplacementDisplay Display = new ReplacementDisplay();
        internal string DirtMode;
        internal bool DirtEnabled;
        internal bool HasClipping;
        internal SkeletonData PreparedData;
    }

    internal static class SpineComposer
    {
        private sealed class AttachmentEntry
        {
            internal string Skin;
            internal string Slot;
            internal string Name;
            internal Dictionary<string, object> Data;
            internal string Key => KeyOf(Skin, Slot, Name);
        }

        private static readonly string[] AllSections =
        {
            "bones", "slots", "constraints", "skins", "events", "animations"
        };

        private static readonly string[] ConstraintSections = { "ik", "transform", "path", "physics" };
        private static readonly Regex DirtRegion = new Regex(
            "^(EM\\d*_|ND\\d*_|m\\d+_|f\\d+_*m\\d+_)[\\w \\-]+$", RegexOptions.CultureInvariant);

        internal static SpineCompositionResult Compose(string originalJson, IReadOnlyList<ReplacementTarget> layers, Atlas atlas,
            float? runtimeScale = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (layers == null || layers.Count == 0) throw new InvalidDataException("Spine composition has no layers.");
            if (atlas == null || atlas.Pages.Count != 1) throw new InvalidDataException("Spine composition requires one atlas page.");
            var original = PortraitJson.Parse(originalJson);
            RequireVersion(original);
            var result = CloneObject(original);
            var provenance = new Dictionary<Dictionary<string, object>, List<string>>();
            RecordAttachmentSources(result, BoneNames(original), provenance);
            var animationMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var skinMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var boneMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var display = new ReplacementDisplay();
            string animationFallback = null;
            string skinFallback = null;
            string dirt = "auto";

            foreach (var layer in layers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (layer.JsonPath != null)
                {
                    var source = PortraitJson.Parse(ReplacementResourceIO.ReadText(layer.JsonPath));
                    RequireVersion(source);
                    var sections = layer.Sections.Contains("all")
                        ? new HashSet<string>(AllSections, StringComparer.Ordinal)
                        : layer.Sections;
                    foreach (string section in sections)
                    {
                        if (section == "constraints") ReplaceConstraints(result, source);
                        else if (section == "attachments") MergeAttachments(result, source, provenance);
                        else if (section == "skins") ReplaceSkins(result, source, provenance);
                        else Replace(result, source, section);
                    }
                }
                Overlay(animationMap, layer.AnimationMap);
                Overlay(skinMap, layer.SkinMap);
                Overlay(boneMap, layer.BoneMap);
                if (layer.AnimationFallback != null) animationFallback = layer.AnimationFallback;
                if (layer.SkinFallback != null) skinFallback = layer.SkinFallback;
                Overlay(display, layer.Display);
                if (layer.Dirt != null) dirt = layer.Dirt;
            }

            var finalBones = BoneNames(result);
            var finalBoneIndices = finalBones.Select((name, index) => new { name, index })
                .ToDictionary(item => item.name, item => item.index, StringComparer.Ordinal);
            ValidateBoneMappings(boneMap, finalBoneIndices);
            RewriteBoneReferences(result, finalBoneIndices, boneMap);
            RemapWeightedVertices(result, provenance, finalBoneIndices, boneMap);
            AddAliases(original, result, "animations", animationMap, animationFallback);
            AddSkinAliases(original, result, skinMap, skinFallback);
            ValidateStructure(result, atlas);

            string json = PortraitJson.Serialize(result);
            cancellationToken.ThrowIfCancellationRequested();
            // 与 SpineAtlasAsset.GetAtlas 的坐标约定一致；只在运行时准备路径翻转一次。
            if (runtimeScale.HasValue) atlas.FlipV();
            var reader = new SkeletonJson(atlas) { Scale = runtimeScale.HasValue ? display.SkeletonScale ?? runtimeScale.Value : 1f };
            var data = reader.ReadSkeletonData(new StringReader(json));
            bool compatibleDirt = atlas.Regions.Any(region => DirtRegion.IsMatch(region.name));
            if (dirt == "legacy" && !compatibleDirt)
                throw new InvalidDataException("effects.dirt=legacy requires a compatible EM/ND atlas region.");
            var composition = new SpineCompositionResult
            {
                Json = json,
                PreparedData = runtimeScale.HasValue ? data : null,
                Display = display,
                DirtMode = dirt,
                DirtEnabled = dirt == "legacy" || (dirt == "auto" && compatibleDirt),
                HasClipping = Entries(result).Values.Any(entry => TypeOf(entry.Data) == "clipping")
            };
            foreach (var pair in boneMap) composition.BoneMap.Add(pair.Key, pair.Value);
            return composition;
        }

        private static void Overlay(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (var pair in source) target[pair.Key] = pair.Value;
        }

        private static void Overlay(ReplacementDisplay target, ReplacementDisplay source)
        {
            if (source.SkeletonScale.HasValue) target.SkeletonScale = source.SkeletonScale;
            if (source.ScaleMultiplier.HasValue) target.ScaleMultiplier = source.ScaleMultiplier;
            if (source.OffsetX.HasValue) target.OffsetX = source.OffsetX;
            if (source.OffsetY.HasValue) target.OffsetY = source.OffsetY;
            if (source.Width.HasValue) target.Width = source.Width;
            if (source.Height.HasValue) target.Height = source.Height;
            if (source.RightShift.HasValue) target.RightShift = source.RightShift;
        }

        private static void Replace(Dictionary<string, object> target, Dictionary<string, object> source, string section)
        {
            object value = PortraitJson.Get(source, section);
            if (value == null) target.Remove(section);
            else target[section] = Clone(value);
        }

        private static void ReplaceConstraints(Dictionary<string, object> target, Dictionary<string, object> source)
        {
            foreach (string section in ConstraintSections) Replace(target, source, section);
        }

        private static void ReplaceSkins(Dictionary<string, object> target, Dictionary<string, object> source,
            Dictionary<Dictionary<string, object>, List<string>> provenance)
        {
            var skins = Clone(PortraitJson.Get(source, "skins")) as List<object>
                ?? throw new InvalidDataException("Replacement skins section is missing.");
            target["skins"] = skins;
            provenance.Clear();
            RecordAttachmentSources(target, BoneNames(source), provenance);
        }

        private static void MergeAttachments(Dictionary<string, object> target, Dictionary<string, object> source,
            Dictionary<Dictionary<string, object>, List<string>> provenance)
        {
            var targetSkins = Skins(target);
            var sourceBones = BoneNames(source);
            foreach (var sourceSkin in Skins(source))
            {
                if (!targetSkins.TryGetValue(sourceSkin.Key, out var targetSkin))
                {
                    targetSkin = new Dictionary<string, object>
                    {
                        ["name"] = sourceSkin.Key,
                        ["attachments"] = new Dictionary<string, object>()
                    };
                    PortraitJson.Array(target["skins"]).Add(targetSkin);
                    targetSkins.Add(sourceSkin.Key, targetSkin);
                }
                object raw = PortraitJson.Get(sourceSkin.Value, "attachments");
                if (raw == null) continue;
                var targetAttachments = GetOrCreateObject(targetSkin, "attachments");
                foreach (var sourceSlot in PortraitJson.Object(raw))
                {
                    var targetSlot = GetOrCreateObject(targetAttachments, sourceSlot.Key);
                    foreach (var sourceAttachment in PortraitJson.Object(sourceSlot.Value))
                    {
                        var data = CloneObject(PortraitJson.Object(sourceAttachment.Value));
                        targetSlot[sourceAttachment.Key] = data;
                        provenance[data] = sourceBones;
                    }
                }
            }
        }

        private static void RecordAttachmentSources(Dictionary<string, object> json, List<string> bones,
            Dictionary<Dictionary<string, object>, List<string>> provenance)
        {
            foreach (var entry in Entries(json).Values) provenance[entry.Data] = bones;
        }

        private static void RemapWeightedVertices(Dictionary<string, object> json,
            Dictionary<Dictionary<string, object>, List<string>> provenance, Dictionary<string, int> finalBones,
            Dictionary<string, string> boneMap)
        {
            foreach (var entry in Entries(json).Values)
            {
                if (!provenance.TryGetValue(entry.Data, out var sourceBones))
                    throw new InvalidDataException("Attachment source bones are unknown: " + entry.Key);
                object rawVertices = PortraitJson.Get(entry.Data, "vertices");
                if (rawVertices == null) continue;
                var vertices = PortraitJson.Array(rawVertices);
                int vertexCount = VertexCount(entry.Data);
                if (vertexCount < 0 || vertices.Count == vertexCount * 2) continue;
                int cursor = 0;
                for (int vertex = 0; vertex < vertexCount; vertex++)
                {
                    if (cursor >= vertices.Count) throw new InvalidDataException("Truncated weighted vertices: " + entry.Key);
                    int influenceCount = PortraitJson.Integer(vertices[cursor++]);
                    if (influenceCount <= 0 || influenceCount > sourceBones.Count
                        || (long)cursor + influenceCount * 4L > vertices.Count)
                        throw new InvalidDataException("Invalid weighted vertices: " + entry.Key);
                    var indices = new HashSet<int>();
                    double totalWeight = 0;
                    for (int influence = 0; influence < influenceCount; influence++, cursor += 4)
                    {
                        int oldIndex = PortraitJson.Integer(vertices[cursor]);
                        if (oldIndex < 0 || oldIndex >= sourceBones.Count)
                            throw new InvalidDataException("Weighted vertex bone index is out of range: " + entry.Key);
                        string finalName = ResolveBone(sourceBones[oldIndex], finalBones, boneMap);
                        int finalIndex = finalBones[finalName];
                        if (!indices.Add(finalIndex))
                            throw new InvalidDataException("Duplicate weighted vertex bone: " + entry.Key);
                        vertices[cursor] = finalIndex;
                        PortraitJson.Number(vertices[cursor + 1]);
                        PortraitJson.Number(vertices[cursor + 2]);
                        double weight = PortraitJson.Number(vertices[cursor + 3]);
                        if (weight < 0 || weight > 1)
                            throw new InvalidDataException("Invalid weighted vertex weight: " + entry.Key);
                        totalWeight += weight;
                    }
                    if (Math.Abs(totalWeight - 1) > 0.02)
                        throw new InvalidDataException("Weighted vertex weights must sum to one: " + entry.Key);
                }
                if (cursor != vertices.Count) throw new InvalidDataException("Trailing weighted vertex data: " + entry.Key);
            }
        }

        private static int VertexCount(Dictionary<string, object> attachment)
        {
            string type = TypeOf(attachment);
            if (type == "mesh")
            {
                object uvs = PortraitJson.Get(attachment, "uvs");
                return uvs == null ? -1 : PortraitJson.Array(uvs).Count / 2;
            }
            object count = PortraitJson.Get(attachment, "vertexCount");
            return count == null ? -1 : PortraitJson.Integer(count);
        }

        private static void RewriteBoneReferences(Dictionary<string, object> json, Dictionary<string, int> finalBones,
            Dictionary<string, string> boneMap)
        {
            foreach (object value in RequiredArray(json, "bones"))
            {
                var bone = PortraitJson.Object(value);
                RewriteString(bone, "parent", finalBones, boneMap);
            }
            foreach (object value in RequiredArray(json, "slots"))
                RewriteString(PortraitJson.Object(value), "bone", finalBones, boneMap);
            foreach (var skin in Skins(json).Values)
            {
                object rawBones = PortraitJson.Get(skin, "bones");
                if (rawBones == null) continue;
                var bones = PortraitJson.Array(rawBones);
                for (int i = 0; i < bones.Count; i++)
                {
                    string name = bones[i] as string
                        ?? throw new InvalidDataException("Skin bone reference must be a string.");
                    bones[i] = ResolveBone(name, finalBones, boneMap);
                }
            }
            foreach (string section in ConstraintSections)
            {
                object raw = PortraitJson.Get(json, section);
                if (raw == null) continue;
                foreach (object value in PortraitJson.Array(raw))
                {
                    var constraint = PortraitJson.Object(value);
                    object bones = PortraitJson.Get(constraint, "bones");
                    if (bones != null)
                    {
                        var list = PortraitJson.Array(bones);
                        for (int i = 0; i < list.Count; i++)
                        {
                            string name = list[i] as string ?? throw new InvalidDataException("Constraint bone must be a string.");
                            list[i] = ResolveBone(name, finalBones, boneMap);
                        }
                    }
                    RewriteString(constraint, "bone", finalBones, boneMap);
                    if (section != "path") RewriteString(constraint, "target", finalBones, boneMap);
                }
            }
            object animations = PortraitJson.Get(json, "animations");
            if (animations == null) return;
            foreach (var animation in PortraitJson.Object(animations).Values)
            {
                var data = PortraitJson.Object(animation);
                object bones = PortraitJson.Get(data, "bones");
                if (bones == null) continue;
                var timelines = PortraitJson.Object(bones);
                var renamed = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var pair in timelines)
                {
                    string name = ResolveBone(pair.Key, finalBones, boneMap);
                    if (renamed.ContainsKey(name)) throw new InvalidDataException("Animation bone mapping collides: " + name);
                    renamed.Add(name, pair.Value);
                }
                data["bones"] = renamed;
            }
        }

        private static void RewriteString(Dictionary<string, object> data, string key, Dictionary<string, int> finalBones,
            Dictionary<string, string> boneMap)
        {
            string value = PortraitJson.String(data, key);
            if (value != null) data[key] = ResolveBone(value, finalBones, boneMap);
        }

        private static string ResolveBone(string name, Dictionary<string, int> finalBones, Dictionary<string, string> boneMap)
        {
            if (finalBones.ContainsKey(name)) return name;
            if (boneMap.TryGetValue(name, out string mapped) && finalBones.ContainsKey(mapped)) return mapped;
            throw new InvalidDataException("Missing bone reference: " + name);
        }

        private static void ValidateBoneMappings(Dictionary<string, string> boneMap, Dictionary<string, int> finalBones)
        {
            foreach (var pair in boneMap)
                if (!finalBones.ContainsKey(pair.Value))
                    throw new InvalidDataException("Bone compatibility target does not exist: " + pair.Key + " -> " + pair.Value);
        }

        private static void AddAliases(Dictionary<string, object> original, Dictionary<string, object> result, string section,
            Dictionary<string, string> mappings, string fallback)
        {
            var final = GetOrCreateObject(result, section);
            var originalValues = PortraitJson.Get(original, section) == null
                ? new Dictionary<string, object>() : PortraitJson.Object(original[section]);
            foreach (string name in originalValues.Keys.Concat(mappings.Keys).Distinct(StringComparer.Ordinal))
            {
                string resolved;
                if (mappings.TryGetValue(name, out string mapped)) resolved = mapped;
                else if (final.ContainsKey(name)) resolved = name;
                else resolved = fallback;
                if (string.IsNullOrEmpty(resolved) || !final.TryGetValue(resolved, out object value))
                    throw new InvalidDataException("No compatible " + section.TrimEnd('s') + " for original name: " + name);
                if (resolved != name || mappings.ContainsKey(name)) final[name] = Clone(value);
            }
        }

        private static void AddSkinAliases(Dictionary<string, object> original, Dictionary<string, object> result,
            Dictionary<string, string> mappings, string fallback)
        {
            var final = Skins(result);
            var originalNames = Skins(original).Keys.Concat(mappings.Keys).Distinct(StringComparer.Ordinal).ToList();
            foreach (string name in originalNames)
            {
                string resolved;
                if (mappings.TryGetValue(name, out string mapped)) resolved = mapped;
                else if (final.ContainsKey(name)) resolved = name;
                else resolved = fallback;
                if (string.IsNullOrEmpty(resolved) || !final.TryGetValue(resolved, out var value))
                    throw new InvalidDataException("No compatible skin for original name: " + name);
                if (resolved == name && !mappings.ContainsKey(name)) continue;
                var alias = CloneObject(value);
                alias["name"] = name;
                int index = PortraitJson.Array(result["skins"]).FindIndex(item => PortraitJson.String(PortraitJson.Object(item), "name") == name);
                if (index >= 0) PortraitJson.Array(result["skins"])[index] = alias;
                else PortraitJson.Array(result["skins"]).Add(alias);
                final[name] = alias;
            }
        }

        private static void ValidateStructure(Dictionary<string, object> json, Atlas atlas)
        {
            var slots = new HashSet<string>(RequiredArray(json, "slots")
                .Select(value => PortraitJson.String(PortraitJson.Object(value), "name")), StringComparer.Ordinal);
            var entries = Entries(json);
            foreach (var entry in entries.Values)
            {
                if (!slots.Contains(entry.Slot)) throw new InvalidDataException("Attachment uses missing slot: " + entry.Key);
                string type = TypeOf(entry.Data);
                ValidateAttachmentGeometry(entry);
                if (type == "linkedmesh") LinkedRoot(entries, entry.Key, new HashSet<string>(StringComparer.Ordinal));
                if (type == "region" || type == "mesh" || type == "linkedmesh")
                {
                    string path = PortraitJson.String(entry.Data, "path", PortraitJson.String(entry.Data, "name", entry.Name));
                    if (atlas.FindRegion(path) == null) throw new InvalidDataException("Missing atlas region: " + path);
                }
                if (type == "clipping")
                {
                    string end = PortraitJson.String(entry.Data, "end");
                    if (end != null && !slots.Contains(end)) throw new InvalidDataException("Clipping attachment uses missing end slot: " + end);
                }
            }
            object animations = PortraitJson.Get(json, "animations");
            if (animations == null) return;
            foreach (var animation in PortraitJson.Object(animations))
            {
                var data = PortraitJson.Object(animation.Value);
                foreach (string property in new[] { "deform", "attachments" })
                {
                    object raw = PortraitJson.Get(data, property);
                    if (raw == null) continue;
                    foreach (var skin in PortraitJson.Object(raw))
                        foreach (var slot in PortraitJson.Object(skin.Value))
                            foreach (var attachment in PortraitJson.Object(slot.Value))
                            {
                                bool isDeform = property == "deform"
                                    || PortraitJson.Get(PortraitJson.Object(attachment.Value), "deform") != null;
                                if (isDeform && !entries.ContainsKey(KeyOf(skin.Key, slot.Key, attachment.Key)))
                                    throw new InvalidDataException("Deform uses missing attachment: "
                                        + KeyOf(skin.Key, slot.Key, attachment.Key));
                            }
                }
            }
        }

        private static void ValidateAttachmentGeometry(AttachmentEntry entry)
        {
            var data = entry.Data;
            if (PortraitJson.Get(data, "sequence") != null)
                throw new InvalidDataException("Attachment sequences are unsupported: " + entry.Key);
            if (TypeOf(data) != "mesh") return;
            var uvs = PortraitJson.Array(PortraitJson.Get(data, "uvs"));
            var triangles = PortraitJson.Array(PortraitJson.Get(data, "triangles"));
            var vertices = PortraitJson.Array(PortraitJson.Get(data, "vertices"));
            if (uvs.Count < 6 || uvs.Count % 2 != 0 || triangles.Count == 0 || triangles.Count % 3 != 0)
                throw new InvalidDataException("Invalid mesh shape: " + entry.Key);
            foreach (object uv in uvs) PortraitJson.Number(uv);
            foreach (object vertex in vertices) PortraitJson.Number(vertex);
            foreach (object rawIndex in triangles)
            {
                int index = PortraitJson.Integer(rawIndex);
                if (index < 0 || index >= uvs.Count / 2)
                    throw new InvalidDataException("Mesh triangle index is out of range: " + entry.Key);
            }
        }

        private static AttachmentEntry LinkedRoot(Dictionary<string, AttachmentEntry> entries, string key,
            HashSet<string> visiting)
        {
            if (!entries.TryGetValue(key, out var entry) || !visiting.Add(key))
                throw new InvalidDataException("Missing or cyclic linked mesh reference: " + key);
            if (TypeOf(entry.Data) != "linkedmesh") return entry;
            string skin = PortraitJson.String(entry.Data, "skin", "default");
            string parent = PortraitJson.String(entry.Data, "parent");
            var root = LinkedRoot(entries, KeyOf(skin, entry.Slot, parent), visiting);
            if (TypeOf(root.Data) != "mesh") throw new InvalidDataException("Linked mesh parent is not a mesh: " + key);
            return root;
        }

        private static Dictionary<string, AttachmentEntry> Entries(Dictionary<string, object> json)
        {
            var result = new Dictionary<string, AttachmentEntry>(StringComparer.Ordinal);
            foreach (var skin in Skins(json))
            {
                object raw = PortraitJson.Get(skin.Value, "attachments");
                if (raw == null) continue;
                foreach (var slot in PortraitJson.Object(raw))
                    foreach (var attachment in PortraitJson.Object(slot.Value))
                    {
                        var entry = new AttachmentEntry
                        {
                            Skin = skin.Key,
                            Slot = slot.Key,
                            Name = attachment.Key,
                            Data = PortraitJson.Object(attachment.Value)
                        };
                        result.Add(entry.Key, entry);
                    }
            }
            return result;
        }

        private static Dictionary<string, Dictionary<string, object>> Skins(Dictionary<string, object> json)
        {
            var result = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
            foreach (object value in RequiredArray(json, "skins"))
            {
                var skin = PortraitJson.Object(value);
                result.Add(PortraitJson.String(skin, "name"), skin);
            }
            return result;
        }

        private static List<object> RequiredArray(Dictionary<string, object> json, string key)
        {
            object value = PortraitJson.Get(json, key);
            return value == null ? new List<object>() : PortraitJson.Array(value);
        }

        private static List<string> BoneNames(Dictionary<string, object> json)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (object value in RequiredArray(json, "bones"))
            {
                string name = PortraitJson.String(PortraitJson.Object(value), "name");
                if (string.IsNullOrEmpty(name) || !seen.Add(name)) throw new InvalidDataException("Invalid or duplicate bone name.");
                result.Add(name);
            }
            if (result.Count == 0) throw new InvalidDataException("Spine skeleton has no bones.");
            return result;
        }

        private static Dictionary<string, object> GetOrCreateObject(Dictionary<string, object> parent, string key)
        {
            if (!parent.TryGetValue(key, out object value))
            {
                var created = new Dictionary<string, object>(StringComparer.Ordinal);
                parent[key] = created;
                return created;
            }
            return PortraitJson.Object(value);
        }

        private static string KeyOf(string skin, string slot, string attachment)
        {
            if (new[] { skin, slot, attachment }.Any(string.IsNullOrEmpty))
                throw new InvalidDataException("Invalid skin/slot/attachment key.");
            return skin + "\n" + slot + "\n" + attachment;
        }

        private static string TypeOf(Dictionary<string, object> attachment) =>
            PortraitJson.String(attachment, "type", "region").ToLowerInvariant();

        private static void RequireVersion(Dictionary<string, object> json)
        {
            string version = PortraitJson.String(PortraitJson.Object(PortraitJson.Get(json, "skeleton")), "spine", "");
            string[] parts = version.Split('.');
            if (parts.Length < 2 || parts[0] != "4" || parts[1] != "1")
                throw new InvalidDataException("This loader requires Spine 4.1 JSON exports.");
        }

        private static Dictionary<string, object> CloneObject(Dictionary<string, object> value) =>
            (Dictionary<string, object>)Clone(value);

        private static object Clone(object value)
        {
            if (value is Dictionary<string, object> map)
            {
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var pair in map) result.Add(pair.Key, Clone(pair.Value));
                return result;
            }
            if (value is IList list)
            {
                var result = new List<object>(list.Count);
                foreach (object item in list) result.Add(Clone(item));
                return result;
            }
            return value;
        }
    }
}
