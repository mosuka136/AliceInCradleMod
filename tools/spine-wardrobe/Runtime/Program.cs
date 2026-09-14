using System.Text.Json;
using BetterExperience.Patches.ReplaceTexture;
using Spine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Wardrobe.Runtime.Tests")]

namespace Wardrobe;

internal static class Program
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, IncludeFields = true };

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Usage: Wardrobe.Runtime <atlas|validate|frames|geometry> request.json");
            var request = JsonDocument.Parse(File.ReadAllText(args[1])).RootElement;
            var atlas = PortraitCatalog.ReadAtlas(File.ReadAllText(request.GetProperty("atlas").GetString()));
            object result;
            if (args[0] == "atlas")
            {
                result = new
                {
                    pages = atlas.Pages.Select(p => new { p.name, p.width, p.height, p.pma }),
                    regions = atlas.Regions.Select(r => new
                    {
                        r.name, r.x, r.y, r.width, r.height, r.degrees,
                        r.originalWidth, r.originalHeight, r.offsetX, r.offsetY,
                        r.u, r.v, r.u2, r.v2
                    })
                };
            }
            else
            {
                string source = File.ReadAllText(request.GetProperty("original").GetString());
                if (args[0] == "geometry") result = Geometry(source, atlas, request);
                else
                {
                    string external = File.ReadAllText(request.GetProperty("external").GetString());
                    StrictContract(source, external);
                    PortraitCatalog.ValidateImage(File.ReadAllBytes(request.GetProperty("image").GetString()), atlas);
                    string merged = PortraitMerger.Merge(source, external, atlas);
                    var data = new SkeletonJson(atlas).ReadSkeletonData(new StringReader(merged));
                    if (args[0] == "validate")
                    {
                        result = new { passed = true, bones = data.Bones.Count, slots = data.Slots.Count,
                            skins = data.Skins.Select(s => s.Name), animations = data.Animations.Select(a => new { a.Name, a.Duration }) };
                    }
                    else if (args[0] == "frames") result = Frames(data, request);
                    else throw new ArgumentException("Unknown command: " + args[0]);
                }
            }
            string destination = request.GetProperty("output").GetString();
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination)));
            File.WriteAllText(destination, JsonSerializer.Serialize(result, Options));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    // Wardrobe v1 is intentionally stricter than the general portrait loader.
    internal static void StrictContract(string source, string external)
    {
        var a = PortraitJson.Parse(source);
        var b = PortraitJson.Parse(external);
        foreach (string key in new[] { "bones", "slots", "ik", "transform", "path", "physics", "animations", "events" })
            if (!PortraitJson.Equal(PortraitJson.Get(a, key), PortraitJson.Get(b, key)))
                throw new InvalidDataException("Wardrobe must preserve " + key);
        var sourceEntries = Entries(a).ToDictionary(e => e.Key, e => e.Value);
        var candidateEntries = Entries(b).ToDictionary(e => e.Key, e => e.Value);
        if (!sourceEntries.Keys.Order().SequenceEqual(candidateEntries.Keys.Order()))
            throw new InvalidDataException("Wardrobe must preserve the complete attachment set.");
        foreach (var entry in sourceEntries)
        {
            var old = entry.Value;
            var replacement = candidateEntries[entry.Key];
            foreach (var key in old.Keys.Union(replacement.Keys).Where(k => k != "vertices"))
                if (!PortraitJson.Equal(PortraitJson.Get(old, key), PortraitJson.Get(replacement, key)))
                    throw new InvalidDataException("Wardrobe v1 only changes mesh vertex coordinates: " + entry.Key + "/" + key);
            if (PortraitJson.Get(old, "vertices") is List<object> vertices)
            {
                var updated = PortraitJson.Array(PortraitJson.Get(replacement, "vertices"));
                if (vertices.Count != updated.Count) throw new InvalidDataException("Vertex count changed.");
                if (PortraitJson.String(old, "type", "region") != "mesh")
                {
                    if (!PortraitJson.Equal(vertices, updated)) throw new InvalidDataException("Functional vertices changed.");
                    continue;
                }
                int uvCount = PortraitJson.Array(old["uvs"]).Count;
                if (vertices.Count == uvCount) continue;
                for (int i = 0; i < vertices.Count;)
                {
                    int count = PortraitJson.Integer(vertices[i]);
                    if (!PortraitJson.Equal(vertices[i], updated[i])) throw new InvalidDataException("Influence count changed.");
                    i++;
                    for (int j = 0; j < count; j++, i += 4)
                        if (!PortraitJson.Equal(vertices[i], updated[i]) || !PortraitJson.Equal(vertices[i + 3], updated[i + 3]))
                            throw new InvalidDataException("Bone index or weight changed.");
                }
            }
        }
    }

    private static IEnumerable<KeyValuePair<string, Dictionary<string, object>>> Entries(Dictionary<string, object> json)
    {
        foreach (var item in PortraitJson.Array(json["skins"]))
        {
            var skin = PortraitJson.Object(item);
            if (PortraitJson.Get(skin, "attachments") == null) continue;
            foreach (var slot in PortraitJson.Object(skin["attachments"]))
                foreach (var attachment in PortraitJson.Object(slot.Value))
                    yield return new(skin["name"] + "/" + slot.Key + "/" + attachment.Key, PortraitJson.Object(attachment.Value));
        }
    }

    internal static object Geometry(string source, Atlas atlas, JsonElement request)
    {
        var json = PortraitJson.Parse(source);
        var entries = Entries(json).ToDictionary(e => e.Key, e => e.Value);
        var skeleton = new Skeleton(new SkeletonJson(atlas).ReadSkeletonData(new StringReader(source)));
        skeleton.UpdateWorldTransform();
        foreach (var edit in request.GetProperty("edits").EnumerateArray())
        {
            string key = edit.GetProperty("attachment").GetString();
            var data = entries[key];
            string skinName = key.Substring(0, key.IndexOf('/'));
            var activeSkin = new Skin("wardrobe-geometry");
            if (skeleton.Data.DefaultSkin != null) activeSkin.AddSkin(skeleton.Data.DefaultSkin);
            activeSkin.AddSkin(skeleton.Data.FindSkin(skinName) ?? throw new InvalidDataException("Unknown skin: " + skinName));
            skeleton.SetSkin(activeSkin, false);
            skeleton.SetToSetupPose();
            skeleton.UpdateWorldTransform();
            if (PortraitJson.String(data, "type", "region") != "mesh")
                throw new InvalidDataException("Coordinate edits require a root mesh: " + key);
            var vertices = PortraitJson.Array(data["vertices"]);
            int count = PortraitJson.Array(data["uvs"]).Count / 2;
            bool weighted = vertices.Count != count * 2;
            var offsets = new Dictionary<int, (float X, float Y)>();
            foreach (var delta in edit.GetProperty("deltas").EnumerateArray())
            {
                int index = delta.GetProperty("vertex").GetInt32();
                if (index < 0 || index >= count || offsets.ContainsKey(index)) throw new InvalidDataException("Invalid/duplicate vertex.");
                offsets[index] = (delta.GetProperty("dx").GetSingle(), delta.GetProperty("dy").GetSingle());
            }
            string slotName = edit.GetProperty("slot").GetString();
            if (!key.Contains("/" + slotName + "/", StringComparison.Ordinal)) throw new InvalidDataException("Slot mismatch.");
            int cursor = 0;
            for (int v = 0; v < count; v++)
            {
                var delta = offsets.GetValueOrDefault(v);
                int influences = weighted ? PortraitJson.Integer(vertices[cursor++]) : 1;
                for (int n = 0; n < influences; n++)
                {
                    Bone bone = weighted ? skeleton.Bones.Items[PortraitJson.Integer(vertices[cursor++])] : skeleton.FindSlot(slotName).Bone;
                    if (delta.X == 0 && delta.Y == 0)
                    {
                        cursor += weighted ? 3 : 2;
                        continue;
                    }
                    float determinant = bone.A * bone.D - bone.B * bone.C;
                    if (Math.Abs(determinant) < 0.000001f) throw new InvalidDataException("Singular bone transform.");
                    // Invert only the linear transform: these are world-space displacement vectors.
                    float dx = (bone.D * delta.X - bone.B * delta.Y) / determinant;
                    float dy = (bone.A * delta.Y - bone.C * delta.X) / determinant;
                    vertices[cursor] = (float)PortraitJson.Number(vertices[cursor]) + dx;
                    vertices[cursor + 1] = (float)PortraitJson.Number(vertices[cursor + 1]) + dy;
                    cursor += weighted ? 3 : 2;
                }
            }
        }
        var result = PortraitJson.Serialize(json);
        StrictContract(source, result);
        PortraitMerger.Merge(source, result, atlas);
        return JsonDocument.Parse(result).RootElement.Clone();
    }

    internal static object Frames(SkeletonData data, JsonElement request)
    {
        int samples = request.TryGetProperty("samples", out var rawSamples) ? rawSamples.GetInt32() : 9;
        if (samples < 2 || samples > 121) throw new ArgumentException("samples must be 2..121");
        var cases = new List<(string Name, string[] Skins, string[] Animations)>();
        foreach (var skin in data.Skins)
        {
            cases.Add((skin.Name + "/setup", new[] { skin.Name }, Array.Empty<string>()));
            foreach (var animation in data.Animations)
                cases.Add((skin.Name + "/" + animation.Name, new[] { skin.Name }, new[] { animation.Name }));
        }
        if (request.TryGetProperty("combinations", out var combinations))
            foreach (var c in combinations.EnumerateArray())
                cases.Add((c.GetProperty("name").GetString(), c.GetProperty("skins").EnumerateArray().Select(s => s.GetString()).ToArray(),
                    c.GetProperty("animations").EnumerateArray().Select(s => s.GetString()).ToArray()));
        if (request.TryGetProperty("setupOnly", out var setup) && setup.GetBoolean()) cases = cases.Where(c => c.Animations.Length == 0).ToList();
        var frames = new List<object>();
        foreach (var c in cases)
        {
            var animations = c.Animations.Select(name => data.FindAnimation(name) ?? throw new InvalidDataException("Unknown animation: " + name)).ToArray();
            float duration = animations.Length == 0 ? 0 : animations.Max(a => a.Duration);
            int frameCount = duration == 0 ? 1 : samples;
            for (int i = 0; i < frameCount; i++)
            {
                float time = frameCount == 1 ? 0 : duration * i / (frameCount - 1);
                var skeleton = new Skeleton(data);
                var combinedSkin = new Skin("wardrobe-preview");
                if (data.DefaultSkin != null) combinedSkin.AddSkin(data.DefaultSkin);
                foreach (string name in c.Skins)
                {
                    var skin = data.FindSkin(name) ?? throw new InvalidDataException("Unknown skin: " + name);
                    combinedSkin.AddSkin(skin);
                }
                skeleton.SetSkin(combinedSkin, false);
                skeleton.SetToSetupPose();
                foreach (var animation in animations) animation.Apply(skeleton, -1, time, false, null, 1, MixBlend.Replace, MixDirection.In);
                skeleton.UpdateWorldTransform();
                var draws = new List<object>();
                var clipping = new SkeletonClipping();
                foreach (var slot in skeleton.DrawOrder)
                {
                    float[] vertices, uvs;
                    int[] triangles;
                    float r, g, b, a;
                    string region;
                    if (slot.Attachment is ClippingAttachment clip)
                    {
                        clipping.ClipStart(slot, clip);
                        continue;
                    }
                    if (slot.Attachment is MeshAttachment mesh)
                    {
                        vertices = new float[mesh.WorldVerticesLength];
                        mesh.ComputeWorldVertices(slot, 0, vertices.Length, vertices, 0, 2);
                        uvs = mesh.UVs; triangles = mesh.Triangles;
                        r = mesh.R; g = mesh.G; b = mesh.B; a = mesh.A; region = mesh.Path;
                    }
                    else if (slot.Attachment is RegionAttachment attachment)
                    {
                        vertices = new float[8]; attachment.ComputeWorldVertices(slot, vertices, 0, 2);
                        uvs = attachment.UVs; triangles = new[] { 0, 1, 2, 2, 3, 0 };
                        r = attachment.R; g = attachment.G; b = attachment.B; a = attachment.A; region = attachment.Path;
                    }
                    else { clipping.ClipEnd(slot); continue; }
                    if (clipping.IsClipping)
                    {
                        clipping.ClipTriangles(vertices, vertices.Length, triangles, triangles.Length, uvs);
                        vertices = clipping.ClippedVertices.ToArray();
                        uvs = clipping.ClippedUVs.ToArray(); triangles = clipping.ClippedTriangles.ToArray();
                    }
                    if (vertices.Any(v => !float.IsFinite(v))) throw new InvalidDataException("Non-finite world vertices.");
                    draws.Add(new { slot = slot.Data.Name, attachment = slot.Attachment.Name, region, vertices, uvs, triangles,
                        color = new[] { r * slot.R, g * slot.G, b * slot.B, a * slot.A }, blend = slot.Data.BlendMode.ToString() });
                    clipping.ClipEnd(slot);
                }
                clipping.ClipEnd();
                frames.Add(new { name = c.Name, time, duration, draws,
                    bones = skeleton.Bones.Select(b => new[] { b.WorldX, b.WorldY, b.A, b.B, b.C, b.D }).ToArray() });
            }
        }
        var setupSkeleton = new Skeleton(data);
        setupSkeleton.UpdateWorldTransform();
        return new { frames, boneNames = data.Bones.Select(b => b.Name),
            setupBones = setupSkeleton.Bones.Select(b => new { name = b.Data.Name, b.WorldX, b.WorldY, b.A, b.B, b.C, b.D }) };
    }
}
