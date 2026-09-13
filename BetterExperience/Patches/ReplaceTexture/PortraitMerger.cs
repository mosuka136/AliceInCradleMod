using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal static class PortraitMerger
    {
        private sealed class Entry
        {
            internal string Skin;
            internal string Slot;
            internal string Name;
            internal Dictionary<string, object> Data;
            internal string Key => KeyOf(Skin, Slot, Name);
        }

        internal static string Merge(string originalJson, string externalJson, Atlas atlas)
        {
            var original = PortraitJson.Parse(originalJson);
            var external = PortraitJson.Parse(externalJson);
            string version = Version(original), externalVersion = Version(external);
            if (version != "4.1" || externalVersion != version)
                throw new InvalidDataException("This loader requires matching Spine 4.1 exports.");
            foreach (string key in new[] { "bones", "slots", "ik", "transform", "path", "physics" })
                if (!PortraitJson.Equal(PortraitJson.Get(original, key), PortraitJson.Get(external, key)))
                    throw new InvalidDataException("Original skeleton contract changed: " + key);

            var originalEntries = Entries(original);
            var replacements = Entries(external);
            var originalSkins = Skins(original);
            foreach (var skin in Skins(external))
            {
                if (!originalSkins.TryGetValue(skin.Key, out var baseSkin))
                    throw new InvalidDataException("Unknown skin: " + skin.Key);
                var oldMetadata = baseSkin.Where(p => p.Key != "attachments").ToDictionary(p => p.Key, p => p.Value);
                var newMetadata = skin.Value.Where(p => p.Key != "attachments").ToDictionary(p => p.Key, p => p.Value);
                if (!PortraitJson.Equal(oldMetadata, newMetadata)) throw new InvalidDataException("Skin constraints changed: " + skin.Key);
            }

            var deform = DeformTargets(original);
            var protectedRoots = new HashSet<string>(deform.Select(key => Root(originalEntries, key, new HashSet<string>()).Key));
            int boneCount = PortraitJson.Array(PortraitJson.Get(original, "bones")).Count;
            foreach (var replacement in replacements.Values)
            {
                if (!originalEntries.TryGetValue(replacement.Key, out var source))
                    throw new InvalidDataException("Unknown attachment: " + replacement.Key);
                if (!Renderable(source.Data) || !Renderable(replacement.Data))
                {
                    if (!PortraitJson.Equal(source.Data, replacement.Data))
                        throw new InvalidDataException("Functional attachment cannot be changed: " + replacement.Key);
                    continue;
                }
                if (PathOf(source) != PathOf(replacement))
                    throw new InvalidDataException("Keep the original atlas region name: " + replacement.Key);
                Validate(replacement, boneCount);
                string root = Root(originalEntries, source.Key, new HashSet<string>()).Key;
                if (protectedRoots.Contains(root)) CheckDeform(source, replacement);
                var attachments = PortraitJson.Object(PortraitJson.Get(originalSkins[replacement.Skin], "attachments"));
                PortraitJson.Object(attachments[replacement.Slot])[replacement.Name] = replacement.Data;
            }

            var merged = Entries(original);
            foreach (var entry in merged.Values)
            {
                if (!Renderable(entry.Data)) continue;
                Validate(entry, boneCount);
                if (atlas.FindRegion(PathOf(entry)) == null)
                    throw new InvalidDataException("Missing atlas region: " + PathOf(entry));
                Root(merged, entry.Key, new HashSet<string>());
            }
            // Let the exact game runtime check constraints, deform timelines and linked mesh binding too.
            string json = PortraitJson.Serialize(original);
            var reader = new SkeletonJson(atlas);
            reader.ReadSkeletonData(new StringReader(json));
            return json;
        }

        private static string Version(Dictionary<string, object> json)
        {
            var parts = (PortraitJson.String(PortraitJson.Object(PortraitJson.Get(json, "skeleton")), "spine") ?? "").Split('.');
            return parts.Length >= 2 ? parts[0] + "." + parts[1] : "";
        }

        private static Dictionary<string, Dictionary<string, object>> Skins(Dictionary<string, object> json)
        {
            var result = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
            foreach (var item in PortraitJson.Array(PortraitJson.Get(json, "skins")))
            {
                var skin = PortraitJson.Object(item);
                result.Add(PortraitJson.String(skin, "name"), skin);
            }
            return result;
        }

        private static Dictionary<string, Entry> Entries(Dictionary<string, object> json)
        {
            var result = new Dictionary<string, Entry>(StringComparer.Ordinal);
            foreach (var skin in Skins(json))
            {
                var raw = PortraitJson.Get(skin.Value, "attachments");
                if (raw == null) continue;
                foreach (var slot in PortraitJson.Object(raw))
                    foreach (var item in PortraitJson.Object(slot.Value))
                    {
                        var entry = new Entry { Skin = skin.Key, Slot = slot.Key, Name = item.Key, Data = PortraitJson.Object(item.Value) };
                        result.Add(entry.Key, entry);
                    }
            }
            return result;
        }

        private static string KeyOf(string skin, string slot, string name)
        {
            if (new[] { skin, slot, name }.Any(s => string.IsNullOrEmpty(s) || s.Contains("\n")))
                throw new InvalidDataException("Invalid skin/slot/attachment key.");
            return skin + "\n" + slot + "\n" + name;
        }

        private static string TypeOf(Dictionary<string, object> data) => PortraitJson.String(data, "type", "region");
        private static bool Renderable(Dictionary<string, object> data) => new[] { "region", "mesh", "linkedmesh" }.Contains(TypeOf(data));
        private static string PathOf(Entry entry) => PortraitJson.String(entry.Data, "path", PortraitJson.String(entry.Data, "name", entry.Name));

        private static Entry Root(Dictionary<string, Entry> entries, string key, HashSet<string> visiting)
        {
            if (!entries.TryGetValue(key, out var entry) || !visiting.Add(key))
                throw new InvalidDataException("Missing or cyclic linked mesh/deform reference: " + key);
            if (TypeOf(entry.Data) != "linkedmesh") return entry;
            string parent = PortraitJson.String(entry.Data, "parent");
            // The game resolves an omitted linked-mesh skin against DefaultSkin, not the containing skin.
            string skin = PortraitJson.String(entry.Data, "skin", "default");
            var root = Root(entries, KeyOf(skin, entry.Slot, parent), visiting);
            if (TypeOf(root.Data) != "mesh") throw new InvalidDataException("Linked mesh parent is not a mesh: " + key);
            return root;
        }

        private static HashSet<string> DeformTargets(Dictionary<string, object> json)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            var animations = PortraitJson.Get(json, "animations");
            if (animations == null) return result;
            foreach (var animation in PortraitJson.Object(animations).Values)
            {
                var data = PortraitJson.Object(animation);
                foreach (string property in new[] { "deform", "attachments" })
                {
                    var skins = PortraitJson.Get(data, property);
                    if (skins == null) continue;
                    foreach (var skin in PortraitJson.Object(skins))
                        foreach (var slot in PortraitJson.Object(skin.Value))
                            foreach (var attachment in PortraitJson.Object(slot.Value))
                                if (property == "deform" || PortraitJson.Get(PortraitJson.Object(attachment.Value), "deform") != null)
                                    result.Add(KeyOf(skin.Key, slot.Key, attachment.Key));
                }
            }
            return result;
        }

        private static void CheckDeform(Entry source, Entry replacement)
        {
            if (TypeOf(source.Data) != TypeOf(replacement.Data))
                throw new InvalidDataException("Deform attachment type changed: " + source.Key);
            foreach (string key in new[] { "triangles", "uvs", "hull", "edges", "parent", "skin", "deform" })
                if (!PortraitJson.Equal(PortraitJson.Get(source.Data, key), PortraitJson.Get(replacement.Data, key)))
                    throw new InvalidDataException("Deform topology or vertex correspondence changed: " + source.Key + "/" + key);
            if (TypeOf(source.Data) != "mesh") return;
            var a = PortraitJson.Array(source.Data["vertices"]);
            var b = PortraitJson.Array(replacement.Data["vertices"]);
            int uvCount = PortraitJson.Array(source.Data["uvs"]).Count;
            if (a.Count != b.Count) throw new InvalidDataException("Deform vertex layout changed: " + source.Key);
            if (a.Count == uvCount) return;
            for (int i = 0; i < a.Count;)
            {
                int count = PortraitJson.Integer(a[i]);
                if (!PortraitJson.Equal(a[i], b[i])) throw new InvalidDataException("Deform influences changed: " + source.Key);
                i++;
                for (int j = 0; j < count; j++, i += 4)
                    if (!PortraitJson.Equal(a[i], b[i]) || !PortraitJson.Equal(a[i + 3], b[i + 3]))
                        throw new InvalidDataException("Deform bone/weight layout changed: " + source.Key);
            }
        }

        private static void Validate(Entry entry, int boneCount)
        {
            var data = entry.Data;
            if (PortraitJson.Get(data, "sequence") != null) throw new InvalidDataException("Attachment sequences are unsupported: " + entry.Key);
            foreach (string key in new[] { "x", "y", "rotation", "scaleX", "scaleY", "width", "height" })
                if (PortraitJson.Get(data, key) != null) PortraitJson.Number(data[key]);
            if (TypeOf(data) == "linkedmesh") return;
            if (TypeOf(data) == "region")
            {
                if (PortraitJson.Number(PortraitJson.Get(data, "width")) <= 0 || PortraitJson.Number(PortraitJson.Get(data, "height")) <= 0)
                    throw new InvalidDataException("Invalid region dimensions: " + entry.Key);
                return;
            }
            var uvs = PortraitJson.Array(PortraitJson.Get(data, "uvs"));
            var vertices = PortraitJson.Array(PortraitJson.Get(data, "vertices"));
            var triangles = PortraitJson.Array(PortraitJson.Get(data, "triangles"));
            if (uvs.Count < 6 || uvs.Count % 2 != 0 || triangles.Count == 0 || triangles.Count % 3 != 0)
                throw new InvalidDataException("Invalid mesh shape: " + entry.Key);
            foreach (var uv in uvs) PortraitJson.Number(uv);
            foreach (var vertex in vertices) PortraitJson.Number(vertex);
            foreach (var index in triangles)
                if (PortraitJson.Integer(index) < 0 || PortraitJson.Integer(index) >= uvs.Count / 2)
                    throw new InvalidDataException("Mesh triangle index is out of range: " + entry.Key);
            if (vertices.Count == uvs.Count) return;
            int cursor = 0;
            for (int v = 0; v < uvs.Count / 2; v++)
            {
                if (cursor >= vertices.Count) throw new InvalidDataException("Truncated weighted vertices: " + entry.Key);
                int count = PortraitJson.Integer(vertices[cursor++]);
                if (count <= 0 || count > boneCount || (long)cursor + count * 4L > vertices.Count)
                    throw new InvalidDataException("Invalid bone influence count: " + entry.Key);
                var indices = new HashSet<int>();
                double total = 0;
                for (int i = 0; i < count; i++, cursor += 4)
                {
                    int bone = PortraitJson.Integer(vertices[cursor]);
                    double weight = PortraitJson.Number(vertices[cursor + 3]);
                    if (bone < 0 || bone >= boneCount || !indices.Add(bone) || weight < 0 || weight > 1)
                        throw new InvalidDataException("Invalid bone/weight: " + entry.Key);
                    total += weight;
                }
                if (Math.Abs(total - 1) > 0.02) throw new InvalidDataException("Bone weights must sum to one: " + entry.Key);
            }
            if (cursor != vertices.Count) throw new InvalidDataException("Trailing weighted vertex data: " + entry.Key);
        }
    }
}
