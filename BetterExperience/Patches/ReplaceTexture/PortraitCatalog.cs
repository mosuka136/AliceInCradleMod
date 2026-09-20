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
        internal string AtlasText;
        internal string Identity => Target + "\n" + JsonKey;
    }

    internal sealed class PortraitCatalog
    {
        // Discovered holds every structurally valid manifest regardless of selection, so options can be offered for it.
        internal readonly List<PortraitPackage> Discovered = new List<PortraitPackage>();
        internal readonly Dictionary<string, PortraitPackage> Packages = new Dictionary<string, PortraitPackage>(StringComparer.Ordinal);
        internal readonly HashSet<string> ReservedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal readonly List<string> Errors = new List<string>();

        internal static string Resolve(string root, string directory, string relative, HashSet<string> verifiedDirectories = null)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0)
                throw new InvalidDataException("Expected relative resource path: " + relative);
            string full = Path.GetFullPath(Path.Combine(directory, relative));
            if (!Within(root, full)) throw new InvalidDataException("Resource escapes ReplaceTexture: " + relative);
            // Reject reparse points as well as lexical traversal; a junction must not bypass the root or Sensitive rules.
            for (string path = full; path != null && Within(root, path); path = Path.GetDirectoryName(path))
            {
                // 同一次扫描内缓存已验证的路径，避免每个资源路径都逐级重复探测文件系统。
                if (verifiedDirectories != null && verifiedDirectories.Contains(path)) continue;
                if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Resource uses a reparse point: " + path);
                verifiedDirectories?.Add(path);
            }
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

        // 把启用列表条目的原始行整理为启用 id 有序列表：跳过空白 id，按大小写敏感去重（保留首次出现的位置），
        // 仅统计开关打开的行。行序保留启用先后，供激活阶段的同目标冲突兜底（保留靠后的行）使用。
        internal static List<string> EnabledIds(IEnumerable<(string Id, bool Enabled)> rows)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<string>();
            foreach (var row in rows ?? Array.Empty<(string, bool)>())
            {
                if (!row.Enabled || string.IsNullOrWhiteSpace(row.Id)) continue;
                if (seen.Add(row.Id.Trim())) result.Add(row.Id.Trim());
            }
            return result;
        }

        // 把扫描结果同步进启用列表行：已知包保持现有行序并保留用户开关（稳定行序，列表不随文件扫描跳动），
        // 新扫描到的包按发现顺序追加到末尾（默认关闭）；id 非空白但扫描不到的行视为包已被移除，直接删除；
        // 空白行（输入中的新行）原样置尾保留，避免打断未提交的编辑。敏感内容被关闭时扫描看不到敏感包，
        // 无法区分“被移除”与“被隐藏”，此时不删除未知行（dropUnmatched=false），等敏感开关恢复后再清理。
        internal static List<(string Id, bool Enabled)> SyncRows(IReadOnlyList<PortraitPackage> discovered,
            IEnumerable<(string Id, bool Enabled)> current, bool dropUnmatched = true)
        {
            var flags = new Dictionary<string, bool>(StringComparer.Ordinal);
            var order = new List<string>();
            var trailing = new List<(string, bool)>();
            foreach (var row in current ?? Array.Empty<(string, bool)>())
            {
                string id = row.Id?.Trim();
                if (string.IsNullOrEmpty(id)) { trailing.Add(row); continue; }
                bool known = discovered.Any(package => package.Id == id);
                if (known && !flags.ContainsKey(id)) { flags.Add(id, row.Enabled); order.Add(id); }
                else if (!known && !dropUnmatched) trailing.Add(row);
                // 其余（重复的已知行，或包已被移除的行）直接删除。
            }
            var rows = new List<(string, bool)>(discovered.Count + trailing.Count);
            foreach (string id in order) rows.Add((id, flags[id]));
            foreach (var package in discovered)
                if (!flags.ContainsKey(package.Id)) rows.Add((package.Id, false));
            rows.AddRange(trailing);
            return rows;
        }

        // 冲突自动取消选中：本轮从关变开的行视为用户最新选择，同目标的其它已启用行自动改为关闭，
        // 使列表中同一目标始终只有最新选择的包保持启用；无法判定先后（如启动时配置里已存在多个
        // 同目标启用行）时保留行序靠后的行。未冲突或未启用的行保持不变。
        internal static List<(string Id, bool Enabled)> ResolveSelection(IReadOnlyList<(string Id, bool Enabled)> rows,
            IEnumerable<(string Id, bool Enabled)> previous, IReadOnlyList<PortraitPackage> discovered)
        {
            var before = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var row in previous ?? Array.Empty<(string, bool)>())
            {
                string id = row.Id?.Trim();
                if (!string.IsNullOrEmpty(id) && !before.ContainsKey(id)) before[id] = row.Enabled;
            }
            var targets = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var package in discovered ?? Array.Empty<PortraitPackage>())
                targets[package.Id] = package.Target;
            var byTarget = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (int i = 0; i < rows.Count; i++)
            {
                string id = rows[i].Id?.Trim();
                if (!rows[i].Enabled || string.IsNullOrEmpty(id) || !targets.TryGetValue(id, out string target)) continue;
                if (!byTarget.TryGetValue(target, out List<int> group)) byTarget[target] = group = new List<int>();
                group.Add(i);
            }
            var deselect = new HashSet<int>();
            foreach (var group in byTarget.Values)
            {
                if (group.Count < 2) continue;
                // 优先保留“本轮从关变开”的最后一行；都不属于本轮新选中（存量冲突）则保留行序靠后的行。
                var newlySelected = group.Where(index => before.TryGetValue(rows[index].Id.Trim(), out bool was) && !was).ToList();
                var pool = newlySelected.Count > 0 ? newlySelected : group;
                int winner = pool[pool.Count - 1];
                foreach (int index in group)
                    if (index != winner) deselect.Add(index);
            }
            var result = new List<(string, bool)>(rows.Count);
            for (int i = 0; i < rows.Count; i++)
                result.Add(deselect.Contains(i) ? (rows[i].Id, false) : rows[i]);
            return result;
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
            // 本次扫描内已通过 reparse 检查的路径集合，供 Resolve 复用，减少重复的文件系统探测。
            var verified = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                    string atlasPath = Resolve(root, dir, PortraitJson.String(manifest, "atlas"), verified);
                    Allowed(sensitive, allowSensitive, atlasPath);
                    string atlasText = File.ReadAllText(atlasPath);
                    var atlas = ReadAtlas(atlasText);
                    // Reserve every page even for a disabled or unsupported multi-page package.
                    foreach (var page in atlas.Pages)
                        result.ReservedImages.Add(Resolve(root, Path.GetDirectoryName(atlasPath), page.name, verified));
                    if (PortraitJson.Integer(PortraitJson.Get(manifest, "formatVersion")) != 1)
                        throw new InvalidDataException("Unsupported portrait manifest version.");
                    if (atlas.Pages.Count != 1) throw new InvalidDataException("Portrait atlas must contain exactly one page.");
                    var package = new PortraitPackage
                    {
                        Id = Required(manifest, "id"), Target = Required(manifest, "target"),
                        JsonKey = Required(manifest, "jsonKey"), ManifestPath = file, AtlasPath = atlasPath,
                        JsonPath = Resolve(root, dir, Required(manifest, "json"), verified), AtlasText = atlasText,
                        ImagePath = Resolve(root, Path.GetDirectoryName(atlasPath), atlas.Pages[0].name, verified)
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

        // Activate only selects the enabled, discovered packages; no file IO happens here. Full JSON text and
        // image bytes load lazily in Build at the actual portrait switch, so toggling rows stays cheap.
        // Row sync normally deselects same-target conflicts beforehand (ResolveSelection); if a conflicting
        // set still reaches this point, the pack lower in the enabled list wins so a duplicate identity
        // never reaches the catalog.
        internal static void Activate(PortraitCatalog catalog, bool enabled, IReadOnlyList<string> enabledIds)
        {
            if (!enabled) return;
            var order = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < enabledIds.Count; i++)
                if (!order.ContainsKey(enabledIds[i])) order[enabledIds[i]] = i;
            var found = new List<PortraitPackage>();
            foreach (var package in catalog.Discovered)
                if (order.ContainsKey(package.Id)) found.Add(package);
            foreach (var group in found.GroupBy(p => p.Target, StringComparer.Ordinal).Where(g => g.Count() > 1).ToList())
            {
                var ranked = group.OrderBy(p => order[p.Id]).ToList();
                catalog.Errors.Add("Portrait pack " + ranked[ranked.Count - 1].Id + " overrides "
                    + string.Join(", ", ranked.Take(ranked.Count - 1).Select(p => p.Id)) + " for target " + group.Key + ".");
                foreach (var overridden in ranked.Take(ranked.Count - 1)) found.Remove(overridden);
            }
            foreach (var package in found)
                catalog.Packages.Add(package.Identity, package);
        }

        internal static PortraitCatalog Scan(string root, string sensitive, bool allowSensitive, bool enabled,
            IReadOnlyList<string> enabledIds)
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
