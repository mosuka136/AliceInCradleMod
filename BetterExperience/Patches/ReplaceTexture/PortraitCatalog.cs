using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal sealed class PortraitPackage
    {
        internal string Id;
        internal string Target;
        internal string JsonKey;
        internal string ManifestPath;
        internal string JsonPath;
        internal string AtlasPath;
        internal string ImagePath;
        internal string Json;
        internal string AtlasText;
        internal byte[] Image;
        internal string Identity => Target + "\n" + JsonKey;
    }

    internal sealed class PortraitCatalog
    {
        // Discovered holds every structurally valid manifest regardless of selection, so options can be offered for it.
        internal readonly List<PortraitPackage> Discovered = new List<PortraitPackage>();
        internal readonly Dictionary<string, PortraitPackage> Packages = new Dictionary<string, PortraitPackage>(StringComparer.Ordinal);
        internal readonly HashSet<string> ReservedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal readonly HashSet<string> Conflicts = new HashSet<string>(StringComparer.Ordinal);
        internal readonly List<string> Errors = new List<string>();

        internal static string Resolve(string root, string directory, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0)
                throw new InvalidDataException("Expected relative resource path: " + relative);
            string full = Path.GetFullPath(Path.Combine(directory, relative));
            if (!Within(root, full)) throw new InvalidDataException("Resource escapes ReplaceTexture: " + relative);
            // Reject reparse points as well as lexical traversal; a junction must not bypass the root or Sensitive rules.
            for (string path = full; path != null && Within(root, path); path = Path.GetDirectoryName(path))
                if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Resource uses a reparse point: " + path);
            return full;
        }

        internal static bool Within(string root, string path)
        {
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string full = Path.GetFullPath(path);
            return full.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                || full.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        internal static void Allowed(string sensitive, bool allowSensitive, params string[] paths)
        {
            if (!allowSensitive && paths.Any(path => Within(sensitive, path)))
                throw new InvalidDataException("Sensitive content is disabled.");
        }

        // 把启用列表条目的原始行整理为启用 id 集合：跳过空白 id，按大小写敏感去重，仅统计开关打开的行。
        internal static HashSet<string> EnabledIds(IEnumerable<(string Id, bool Enabled)> rows)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows ?? Array.Empty<(string, bool)>())
                if (row.Enabled && !string.IsNullOrWhiteSpace(row.Id)) result.Add(row.Id.Trim());
            return result;
        }

        // 把扫描结果同步进启用列表行：扫描到的包按扫描顺序排列并保留用户已设置的开关（新包默认关闭），
        // 扫描不到的行（包被删除或用户手工添加）原样移到末尾保留，避免打断未提交的编辑或丢失意图。
        internal static List<(string Id, bool Enabled)> SyncRows(IReadOnlyList<PortraitPackage> discovered,
            IEnumerable<(string Id, bool Enabled)> current)
        {
            var states = new Dictionary<string, bool>(StringComparer.Ordinal);
            var trailing = new List<(string, bool)>();
            foreach (var row in current ?? Array.Empty<(string, bool)>())
            {
                string id = row.Id?.Trim();
                if (string.IsNullOrEmpty(id) || !discovered.Any(package => package.Id == id)) trailing.Add(row);
                else states[id] = row.Enabled;
            }
            var rows = new List<(string, bool)>(discovered.Count + trailing.Count);
            foreach (var package in discovered)
                rows.Add((package.Id, states.TryGetValue(package.Id, out bool enabled) && enabled));
            rows.AddRange(trailing);
            return rows;
        }

        internal static bool CanRetain(PortraitPackage previous, string root, string sensitive, bool allowSensitive,
            bool enabled, IReadOnlyCollection<string> enabledIds, bool switchingPackage = false)
        {
            if (!enabled || !File.Exists(previous.ManifestPath) || (!switchingPackage && !enabledIds.Contains(previous.Id))) return false;
            try
            {
                foreach (string path in new[] { previous.ManifestPath, previous.JsonPath, previous.AtlasPath, previous.ImagePath })
                {
                    Allowed(sensitive, allowSensitive, path);
                    Resolve(root, Path.GetDirectoryName(path), Path.GetFileName(path));
                }
                return true;
            }
            catch { return false; }
        }

        // Discover catalogs every qualifying manifest so the option list and image reservation do not depend on selection.
        // Content checks here are limited to cheap ones (manifest shape, JSON parse, PNG header); full image loading happens in Activate.
        internal static PortraitCatalog Discover(string root, string sensitive, bool allowSensitive)
        {
            var result = new PortraitCatalog();
            if (!Directory.Exists(root)) return result;
            // Enumeration deliberately never follows directory junctions.
            foreach (string file in Enumerate(root).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                if (!file.EndsWith(".portrait.json", StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    // Sensitive manifests are skipped silently while disabled, matching the image scan's exclusion.
                    if (!allowSensitive && Within(sensitive, file)) continue;
                    Allowed(sensitive, allowSensitive, file);
                    var manifest = PortraitJson.Parse(File.ReadAllText(file));
                    string dir = Path.GetDirectoryName(file);
                    string atlasPath = Resolve(root, dir, PortraitJson.String(manifest, "atlas"));
                    Allowed(sensitive, allowSensitive, atlasPath);
                    string atlasText = File.ReadAllText(atlasPath);
                    var atlas = ReadAtlas(atlasText);
                    // Reserve every page even for a disabled or unsupported multi-page package.
                    foreach (var page in atlas.Pages)
                        result.ReservedImages.Add(Resolve(root, Path.GetDirectoryName(atlasPath), page.name));
                    if (PortraitJson.Integer(PortraitJson.Get(manifest, "formatVersion")) != 1)
                        throw new InvalidDataException("Unsupported portrait manifest version.");
                    if (atlas.Pages.Count != 1) throw new InvalidDataException("Portrait atlas must contain exactly one page.");
                    var package = new PortraitPackage
                    {
                        Id = Required(manifest, "id"), Target = Required(manifest, "target"),
                        JsonKey = Required(manifest, "jsonKey"), ManifestPath = file, AtlasPath = atlasPath,
                        JsonPath = Resolve(root, dir, Required(manifest, "json")), AtlasText = atlasText,
                        ImagePath = Resolve(root, Path.GetDirectoryName(atlasPath), atlas.Pages[0].name)
                    };
                    Allowed(sensitive, allowSensitive, package.JsonPath, package.ImagePath);
                    PortraitJson.Parse(File.ReadAllText(package.JsonPath));
                    ValidateImage(ReadHeader(package.ImagePath), atlas);
                    result.Discovered.Add(package);
                }
                catch (Exception ex) { result.Errors.Add(file + ": " + ex.Message); }
            }
            // Duplicate ids would be ambiguous options, so neither manifest is offered or activated.
            foreach (var duplicate in result.Discovered.GroupBy(p => p.Id, StringComparer.Ordinal).Where(g => g.Count() > 1).ToList())
            {
                result.Errors.Add("Duplicate portrait id: " + duplicate.Key);
                result.Discovered.RemoveAll(p => p.Id == duplicate.Key);
            }
            return result;
        }

        // Activate fully loads the selected, discovered packages and refuses conflicting sets.
        internal static void Activate(PortraitCatalog catalog, bool enabled, IReadOnlyCollection<string> enabledIds)
        {
            if (!enabled) return;
            var found = new List<PortraitPackage>();
            foreach (var package in catalog.Discovered)
            {
                if (!enabledIds.Contains(package.Id)) continue;
                try
                {
                    package.Json = File.ReadAllText(package.JsonPath);
                    PortraitJson.Parse(package.Json);
                    package.Image = File.ReadAllBytes(package.ImagePath);
                    ValidateImage(package.Image, ReadAtlas(package.AtlasText));
                    found.Add(package);
                }
                catch (Exception ex) { catalog.Errors.Add(package.ManifestPath + ": " + ex.Message); }
            }
            foreach (var group in found.GroupBy(p => p.Target, StringComparer.Ordinal))
            {
                if (group.Count() > 1)
                {
                    foreach (var package in group) catalog.Conflicts.Add(package.Identity);
                    catalog.Errors.Add("Multiple enabled portrait packages target " + group.Key);
                }
            }
            foreach (var package in found)
                if (!catalog.Conflicts.Contains(package.Identity))
                    catalog.Packages.Add(package.Identity, package);
        }

        internal static PortraitCatalog Scan(string root, string sensitive, bool allowSensitive, bool enabled,
            IReadOnlyCollection<string> enabledIds)
        {
            var result = Discover(root, sensitive, allowSensitive);
            Activate(result, enabled, enabledIds);
            return result;
        }

        private static IEnumerable<string> Enumerate(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0) yield return file;
            foreach (string child in Directory.GetDirectories(directory))
                if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) == 0)
                    foreach (string file in Enumerate(child)) yield return file;
        }

        private static string Required(Dictionary<string, object> map, string key)
        {
            string value = PortraitJson.String(map, key);
            if (string.IsNullOrWhiteSpace(value) || value.Contains("\n") || value.Contains("\r"))
                throw new InvalidDataException("Missing or invalid " + key);
            return value;
        }

        internal static Atlas ReadAtlas(string text)
        {
            return new Atlas(new StringReader(text), "", new MetadataTextureLoader());
        }

        internal static void ValidateImage(byte[] bytes, Atlas atlas)
        {
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (bytes.Length < 33 || !bytes.Take(8).SequenceEqual(signature)
                || bytes[12] != 73 || bytes[13] != 72 || bytes[14] != 68 || bytes[15] != 82)
                throw new InvalidDataException("Expected PNG with IHDR.");
            int width = ReadInt(bytes, 16), height = ReadInt(bytes, 20);
            if (atlas.Pages.Count != 1 || width <= 0 || height <= 0
                || atlas.Pages[0].width != width || atlas.Pages[0].height != height)
                throw new InvalidDataException("PNG dimensions do not match the single-page atlas.");
            if (atlas.Pages[0].pma) throw new InvalidDataException("Export straight-alpha PNG (PMA is unsupported).");
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var region in atlas.Regions)
            {
                if (!names.Add(region.name)) throw new InvalidDataException("Duplicate atlas region: " + region.name);
                // This game's Atlas reader already swaps width/height for a rotated packed region.
                int w = region.width;
                int h = region.height;
                if (region.degrees != 0 && region.degrees != 90) throw new InvalidDataException("Unsupported atlas rotation.");
                if (region.x < 0 || region.y < 0 || w <= 0 || h <= 0
                    || (long)region.x + w > width || (long)region.y + h > height
                    || region.originalWidth <= 0 || region.originalHeight <= 0)
                    throw new InvalidDataException("Invalid atlas bounds: " + region.name);
            }
        }

        private static int ReadInt(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        // Discovery only needs the IHDR header ValidateImage inspects; the full image loads in Activate.
        private static byte[] ReadHeader(string path)
        {
            var header = new byte[33];
            using (var stream = File.OpenRead(path))
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < header.Length) return header.Take(read).ToArray();
            }
            return header;
        }

        private sealed class MetadataTextureLoader : TextureLoader
        {
            public void Load(AtlasPage page, string path) { }
            public void Unload(object texture) { }
        }
    }
}
