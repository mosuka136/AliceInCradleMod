using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal sealed class ReplacementDisplay
    {
        internal float? SkeletonScale;
        internal float? ScaleMultiplier;
        internal float? OffsetX;
        internal float? OffsetY;
        internal float? Width;
        internal float? Height;
        internal float? RightShift;
    }

    internal sealed class ReplacementTarget
    {
        internal ReplacementPackage Owner;
        internal string PackageId;
        internal string Type;
        internal string Loader;
        internal string AssetKey;
        internal string ImageKey;
        internal string ResourcePath;
        internal string ObjectType;
        internal string SpineKey;
        internal string JsonKey;
        internal string ImagePath;
        internal string AtlasPath;
        internal string JsonPath;
        internal readonly HashSet<string> Sections = new HashSet<string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> AnimationMap = new Dictionary<string, string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> SkinMap = new Dictionary<string, string>(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> BoneMap = new Dictionary<string, string>(StringComparer.Ordinal);
        internal string AnimationFallback;
        internal string SkinFallback;
        internal ReplacementDisplay Display = new ReplacementDisplay();
        internal string Dirt;

        internal string Identity
        {
            get
            {
                if (Type == "spine") return "spine\n" + SpineKey + "\n" + JsonKey;
                if (Loader == "mti") return "texture\nmti\n" + AssetKey + "\n" + (ImageKey ?? "");
                return "texture\nresources\n" + ResourcePath + "\n" + ObjectType;
            }
        }
    }

    internal sealed class ReplacementPackage
    {
        internal string Id;
        internal string ManifestPath;
        internal bool Sensitive;
        internal readonly List<ReplacementTarget> Targets = new List<ReplacementTarget>();
        internal readonly HashSet<string> InvalidTargetIdentities = new HashSet<string>(StringComparer.Ordinal);
        internal bool HasUnidentifiedTargetErrors;
    }

    internal sealed class ReplacementCatalog
    {
        internal readonly List<ReplacementPackage> Packages = new List<ReplacementPackage>();
        internal readonly List<string> Errors = new List<string>();
        // 磁盘上所有清单声明的 id，包含未授权的敏感包、解析失败的包和 id 重复的包。
        // 配置行是否“对应的包已不存在”只依据这个集合，与敏感授权和解析结果无关。
        internal readonly HashSet<string> DeclaredIds = new HashSet<string>(StringComparer.Ordinal);
        // 存在读不出 id 的清单时为 false：此时无法判定未知配置行归属，不能删除任何未知行。
        internal bool DeclaredIdsComplete = true;

        internal static ReplacementCatalog Discover(string root, string sensitive, bool allowSensitive)
        {
            var result = new ReplacementCatalog();
            if (!Directory.Exists(root)) return result;
            var verified = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Enumerate(root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                if (!file.EndsWith(".replacement.json", StringComparison.OrdinalIgnoreCase)) continue;
                // 先单独读出清单声明的 id 再做完整解析：敏感包被关闭或清单解析失败时也要登记 id，
                // 否则这些包的配置行会被误判成“包已被删除”而清除。
                Dictionary<string, object> json = null;
                Exception unreadable = null;
                string declared = null;
                try
                {
                    json = PortraitJson.Parse(ReplacementResourceIO.ReadText(file));
                    declared = PortraitJson.String(json, "id")?.Trim();
                }
                catch (Exception ex) { unreadable = ex; }
                if (string.IsNullOrEmpty(declared)) result.DeclaredIdsComplete = false;
                else result.DeclaredIds.Add(declared);
                try
                {
                    bool isSensitive = PortraitCatalog.Within(sensitive, file);
                    if (isSensitive && !allowSensitive) continue;
                    if (json == null)
                        throw unreadable ?? new InvalidDataException("Replacement manifest could not be read.");
                    var package = ParsePackage(root, sensitive, file, isSensitive, verified, result.Errors, json);
                    result.Packages.Add(package);
                }
                catch (Exception ex) { result.Errors.Add(file + ": " + ex.Message); }
            }
            foreach (var duplicate in result.Packages.GroupBy(package => package.Id, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).ToList())
            {
                result.Errors.Add("Duplicate replacement id: " + duplicate.Key);
                result.Packages.RemoveAll(package => package.Id == duplicate.Key);
            }
            return result;
        }

        internal IEnumerable<ReplacementTarget> Layers(string identity, IReadOnlyList<string> enabledIds)
        {
            var byId = Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
            foreach (string id in enabledIds)
                if (byId.TryGetValue(id, out var package))
                    foreach (var target in package.Targets)
                        if (target.Identity == identity) yield return target;
        }

        internal static List<string> EnabledIds(IEnumerable<(string Id, bool Enabled)> rows)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows ?? Array.Empty<(string, bool)>())
            {
                string id = row.Id?.Trim();
                if (row.Enabled && !string.IsNullOrEmpty(id) && seen.Add(id)) result.Add(id);
            }
            return result;
        }

        // 以磁盘上仍然存在的清单 id 为准同步配置行：已授权且解析成功的包（discovered）与仅登记了 id 的包
        // （未授权的敏感包、解析失败的包、id 重复的包）都算存在，对应行连同用户开关一起保留；
        // 扫描不到的 id 视为包已被删除，该行直接从配置中清除。存在读不出 id 的清单时无法判定未知行归属，
        // 整轮退化为只追加不删除。
        internal List<(string Id, bool Enabled)> SyncRows(IEnumerable<(string Id, bool Enabled)> current)
        {
            return SyncRows(Packages, current, DeclaredIdsComplete, DeclaredIds);
        }

        internal static List<(string Id, bool Enabled)> SyncRows(IReadOnlyList<ReplacementPackage> discovered,
            IEnumerable<(string Id, bool Enabled)> current, bool dropUnmatched, IEnumerable<string> declaredIds = null)
        {
            var known = new HashSet<string>(discovered.Select(package => package.Id), StringComparer.Ordinal);
            foreach (string id in declaredIds ?? Array.Empty<string>())
                if (!string.IsNullOrEmpty(id)) known.Add(id);
            var rows = new List<(string, bool)>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in current ?? Array.Empty<(string, bool)>())
            {
                string id = row.Id?.Trim();
                if (string.IsNullOrEmpty(id)) { rows.Add(row); continue; }
                if (known.Contains(id) && seen.Add(id)) rows.Add((id, row.Enabled));
                else if (!known.Contains(id) && !dropUnmatched) rows.Add(row);
            }
            int blank = rows.FindIndex(row => string.IsNullOrWhiteSpace(row.Item1));
            if (blank < 0) blank = rows.Count;
            foreach (var package in discovered)
                if (seen.Add(package.Id)) rows.Insert(blank++, (package.Id, false));
            return rows;
        }

        private static ReplacementPackage ParsePackage(string root, string sensitive, string file, bool isSensitive,
            HashSet<string> verified, List<string> errors, Dictionary<string, object> json)
        {
            if (PortraitJson.Integer(PortraitJson.Get(json, "formatVersion")) != 2)
                throw new InvalidDataException("Unsupported replacement manifest version.");
            var package = new ReplacementPackage
            {
                Id = Required(json, "id"), ManifestPath = file, Sensitive = isSensitive
            };
            string directory = Path.GetDirectoryName(file);
            var items = PortraitJson.Array(PortraitJson.Get(json, "targets"));
            if (items.Count == 0) throw new InvalidDataException("Replacement package has no targets.");
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (object item in items)
            {
                var targetJson = PortraitJson.Object(item);
                string identity = null;
                try
                {
                    identity = IdentityOf(targetJson);
                    if (!identities.Add(identity))
                        throw new InvalidDataException("Duplicate target in package: " + identity.Replace('\n', '/'));
                    var target = ParseTarget(root, sensitive, directory, package.Id, isSensitive, targetJson, verified);
                    target.Owner = package;
                    package.Targets.Add(target);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidDataException && ex.Message.StartsWith("Duplicate target in package:", StringComparison.Ordinal))
                        throw;
                    if (identity != null) package.InvalidTargetIdentities.Add(identity);
                    else package.HasUnidentifiedTargetErrors = true;
                    errors.Add(file + " [" + (identity ?? "invalid target") + "]: " + ex.Message);
                }
            }
            return package;
        }

        private static string IdentityOf(Dictionary<string, object> json)
        {
            string type = Required(json, "type").ToLowerInvariant();
            if (type == "spine") return "spine\n" + Required(json, "key") + "\n" + Required(json, "jsonKey");
            if (type != "texture") throw new InvalidDataException("Target type must be texture or spine.");
            string loader = Required(json, "loader").ToLowerInvariant();
            if (loader == "mti")
                return "texture\nmti\n" + Required(json, "assetKey") + "\n" + (PortraitJson.String(json, "imageKey") ?? "");
            if (loader == "resources")
                return "texture\nresources\n" + Required(json, "path") + "\n" + Required(json, "objectType");
            throw new InvalidDataException("Texture loader must be mti or resources.");
        }

        private static ReplacementTarget ParseTarget(string root, string sensitive, string directory, string packageId,
            bool packageSensitive, Dictionary<string, object> json, HashSet<string> verified)
        {
            var target = new ReplacementTarget { PackageId = packageId, Type = Required(json, "type").ToLowerInvariant() };
            if (target.Type == "texture")
            {
                target.Loader = Required(json, "loader").ToLowerInvariant();
                target.ImagePath = Resource(root, sensitive, directory, Required(json, "image"), packageSensitive, verified);
                ValidatePngHeader(target.ImagePath);
                if (target.Loader == "mti")
                {
                    target.AssetKey = Required(json, "assetKey");
                    target.ImageKey = PortraitJson.String(json, "imageKey");
                }
                else if (target.Loader == "resources")
                {
                    target.ResourcePath = Required(json, "path");
                    target.ObjectType = Required(json, "objectType");
                    if (target.ObjectType != "Texture2D" && target.ObjectType != "Sprite")
                        throw new InvalidDataException("Resources objectType must be Texture2D or Sprite.");
                }
                else throw new InvalidDataException("Texture loader must be mti or resources.");
                return target;
            }
            if (target.Type != "spine") throw new InvalidDataException("Target type must be texture or spine.");
            target.SpineKey = Required(json, "key");
            target.JsonKey = Required(json, "jsonKey");
            string image = PortraitJson.String(json, "image");
            string atlas = PortraitJson.String(json, "atlas");
            if (image != null)
            {
                target.ImagePath = Resource(root, sensitive, directory, image, packageSensitive, verified);
                ValidatePngHeader(target.ImagePath);
            }
            if (atlas != null)
            {
                target.AtlasPath = Resource(root, sensitive, directory, atlas, packageSensitive, verified);
                if (PortraitCatalog.ReadAtlas(ReplacementResourceIO.ReadText(target.AtlasPath)).Pages.Count != 1)
                    throw new InvalidDataException("Spine replacement atlas must contain exactly one page.");
            }
            object spineValue = PortraitJson.Get(json, "spine");
            if (spineValue != null)
            {
                var spine = PortraitJson.Object(spineValue);
                target.JsonPath = Resource(root, sensitive, directory, Required(spine, "json"), packageSensitive, verified);
                PortraitJson.Parse(ReplacementResourceIO.ReadText(target.JsonPath));
                foreach (object sectionValue in PortraitJson.Array(PortraitJson.Get(spine, "replace")))
                {
                    string section = sectionValue as string ?? throw new InvalidDataException("Spine replace entries must be strings.");
                    if (!new[] { "bones", "slots", "constraints", "skins", "attachments", "events", "animations", "all" }.Contains(section))
                        throw new InvalidDataException("Unknown Spine replacement section: " + section);
                    target.Sections.Add(section);
                }
                if (target.Sections.Count == 0) throw new InvalidDataException("Spine replacement has no sections.");
                if (target.Sections.Contains("all") && target.Sections.Count != 1)
                    throw new InvalidDataException("all cannot be combined with other Spine replacement sections.");
                if (target.Sections.Contains("skins") && target.Sections.Contains("attachments"))
                    throw new InvalidDataException("skins and attachments cannot be selected in the same target layer.");
            }
            ReadCompatibility(json, target);
            ReadDisplay(json, target.Display);
            object effectsValue = PortraitJson.Get(json, "effects");
            if (effectsValue != null)
            {
                target.Dirt = PortraitJson.String(PortraitJson.Object(effectsValue), "dirt", "auto");
                if (!new[] { "auto", "legacy", "disabled" }.Contains(target.Dirt))
                    throw new InvalidDataException("effects.dirt must be auto, legacy or disabled.");
            }
            if (target.ImagePath == null && target.AtlasPath == null && target.JsonPath == null
                && target.AnimationMap.Count == 0 && target.SkinMap.Count == 0 && target.BoneMap.Count == 0
                && !HasDisplay(target.Display) && target.Dirt == null)
                throw new InvalidDataException("Spine target does not replace anything.");
            return target;
        }

        private static void ReadCompatibility(Dictionary<string, object> json, ReplacementTarget target)
        {
            object value = PortraitJson.Get(json, "compatibility");
            if (value == null) return;
            var map = PortraitJson.Object(value);
            ReadMap(map, "animations", target.AnimationMap);
            ReadMap(map, "skins", target.SkinMap);
            ReadMap(map, "bones", target.BoneMap);
            target.AnimationFallback = PortraitJson.String(map, "animationFallback");
            target.SkinFallback = PortraitJson.String(map, "skinFallback");
        }

        private static void ReadMap(Dictionary<string, object> parent, string key, Dictionary<string, string> output)
        {
            object value = PortraitJson.Get(parent, key);
            if (value == null) return;
            foreach (var pair in PortraitJson.Object(value))
            {
                string mapped = pair.Value as string;
                if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(mapped))
                    throw new InvalidDataException("Invalid compatibility mapping: " + key);
                output.Add(pair.Key, mapped);
            }
        }

        private static void ReadDisplay(Dictionary<string, object> json, ReplacementDisplay display)
        {
            object value = PortraitJson.Get(json, "display");
            if (value == null) return;
            var map = PortraitJson.Object(value);
            display.SkeletonScale = OptionalNumber(map, "skeletonScale");
            display.ScaleMultiplier = OptionalNumber(map, "scaleMultiplier");
            display.OffsetX = OptionalNumber(map, "offsetX");
            display.OffsetY = OptionalNumber(map, "offsetY");
            display.Width = OptionalNumber(map, "width");
            display.Height = OptionalNumber(map, "height");
            display.RightShift = OptionalNumber(map, "rightShift");
            if ((display.SkeletonScale.HasValue && display.SkeletonScale.Value <= 0)
                || (display.ScaleMultiplier.HasValue && display.ScaleMultiplier.Value <= 0)
                || (display.Width.HasValue && display.Width.Value <= 0)
                || (display.Height.HasValue && display.Height.Value <= 0))
                throw new InvalidDataException("Display scale and dimensions must be positive.");
        }

        private static float? OptionalNumber(Dictionary<string, object> map, string key)
        {
            object value = PortraitJson.Get(map, key);
            return value == null ? (float?)null : (float)PortraitJson.Number(value);
        }

        private static bool HasDisplay(ReplacementDisplay value)
        {
            return value.SkeletonScale.HasValue || value.ScaleMultiplier.HasValue || value.OffsetX.HasValue
                || value.OffsetY.HasValue || value.Width.HasValue || value.Height.HasValue || value.RightShift.HasValue;
        }

        private static string Resource(string root, string sensitive, string directory, string relative,
            bool packageSensitive, HashSet<string> verified)
        {
            string path = PortraitCatalog.Resolve(root, directory, relative, verified);
            if (PortraitCatalog.Within(sensitive, path) != packageSensitive)
                throw new InvalidDataException("A package and all dependencies must stay in the same normal or Sensitive tree.");
            if (!File.Exists(path)) throw new FileNotFoundException("Replacement resource was not found.", path);
            return path;
        }

        private static void ValidatePngHeader(string path)
        {
            byte[] header = ReplacementResourceIO.ReadPrefix(path, 33);
            if (header.Length != 33) throw new InvalidDataException("Expected PNG with IHDR.");
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            if (!header.Take(8).SequenceEqual(signature)) throw new InvalidDataException("Expected PNG with IHDR.");
        }

        private static string Required(Dictionary<string, object> map, string key)
        {
            string value = PortraitJson.String(map, key);
            if (string.IsNullOrWhiteSpace(value) || value.Contains("\n") || value.Contains("\r"))
                throw new InvalidDataException("Missing or invalid " + key);
            return value;
        }

        private static IEnumerable<string> Enumerate(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0) yield return file;
            foreach (string child in Directory.GetDirectories(directory))
                if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) == 0)
                    foreach (string file in Enumerate(child)) yield return file;
        }
    }
}
