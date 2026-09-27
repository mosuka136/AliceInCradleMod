using BetterExperience.Patches.ReplaceTexture;
using System.Security.Cryptography;

namespace BetterExperience.ResourceEncryptor
{
    internal static class PackEncryptor
    {
        internal static int Encrypt(string input, string output, Action<string> progress = null)
        {
            input = Path.TrimEndingDirectorySeparator(Path.GetFullPath(input));
            output = Path.TrimEndingDirectorySeparator(Path.GetFullPath(output));
            if (ReplacementResourcePaths.Within(input, output) || ReplacementResourcePaths.Within(output, input))
                throw new InvalidDataException("Input and output directories must not overlap.");
            ReplacementResourcePaths.CheckAncestors(input);
            ReplacementResourcePaths.CheckAncestors(output);
            if (!Directory.Exists(input)) throw new DirectoryNotFoundException("Input directory does not exist: " + input);
            if (Directory.Exists(output) || File.Exists(output)) throw new IOException("Output already exists: " + output);
            string parent = Path.GetDirectoryName(output);
            if (!Directory.Exists(parent)) throw new DirectoryNotFoundException("Output parent directory does not exist: " + parent);

            var sources = Collect(input);
            // 暂存目录与最终目录在同一父目录中，所有文件回读一致后才发布。
            string staging = Path.Combine(parent, ".be-encrypt-" + Guid.NewGuid().ToString("N"));
            if (Directory.Exists(staging) || File.Exists(staging)) throw new IOException("Temporary output already exists.");
            Directory.CreateDirectory(staging);
            try
            {
                foreach (var source in sources.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                {
                    string relative = Path.GetRelativePath(input, source.Key);
                    string path = ReplacementResourcePaths.Resolve(input, input, relative);
                    string destination = ReplacementResourcePaths.Resolve(staging, staging, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    ReplacementResourcePaths.CheckAncestors(destination);
                    using (var decoded = ReplacementResourceIO.OpenRead(path))
                    using (var encoded = new FileStream(destination, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                        ReplacementResourceIO.Encrypt(decoded, encoded);
                    if (!Hash(destination).SequenceEqual(source.Value))
                        throw new InvalidDataException("Resource changed or encrypted output did not round-trip: " + relative);
                    progress?.Invoke(relative);
                }
                ReplacementResourcePaths.CheckAncestors(output);
                Directory.Move(staging, output);
                return sources.Count;
            }
            finally
            {
                if (Directory.Exists(staging)) RemoveStaging(staging, parent);
            }
        }

        private static Dictionary<string, byte[]> Collect(string root)
        {
            var files = Enumerate(root).Where(path => path.EndsWith(".replacement.json", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
            if (files.Count == 0) throw new InvalidDataException("No v2 .replacement.json manifests were found.");
            var sources = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            string sensitive = Path.Combine(root, "Sensitive");
            foreach (string file in files)
            {
                var manifest = ManifestJson.Parse(ReplacementResourceIO.ReadText(file));
                if (ManifestJson.Integer(ManifestJson.Get(manifest, "formatVersion")) != 2)
                    throw new InvalidDataException("Unsupported replacement manifest version: " + file);
                string id = Required(manifest, "id");
                if (!ids.Add(id)) throw new InvalidDataException("Duplicate replacement id: " + id);
                var targets = ManifestJson.Array(ManifestJson.Get(manifest, "targets"));
                if (targets.Count == 0) throw new InvalidDataException("Replacement package has no targets: " + id);
                Add(sources, file);
                var identities = new HashSet<string>(StringComparer.Ordinal);
                foreach (var item in targets)
                {
                    var target = ManifestJson.Object(item);
                    string type = Required(target, "type").ToLowerInvariant();
                    string identity;
                    if (type == "texture")
                    {
                        string loader = Required(target, "loader").ToLowerInvariant();
                        if (loader == "mti") identity = "texture\nmti\n" + Required(target, "assetKey") + "\n" + ManifestJson.String(target, "imageKey", "");
                        else if (loader == "resources")
                        {
                            string objectType = Required(target, "objectType");
                            if (objectType != "Texture2D" && objectType != "Sprite") throw new InvalidDataException("Invalid Resources objectType.");
                            identity = "texture\nresources\n" + Required(target, "path") + "\n" + objectType;
                        }
                        else throw new InvalidDataException("Texture loader must be mti or resources.");
                        AddDependency(root, sensitive, file, Required(target, "image"), "image", sources);
                    }
                    else if (type == "spine")
                    {
                        identity = "spine\n" + Required(target, "key") + "\n" + Required(target, "jsonKey");
                        foreach (string kind in new[] { "image", "atlas" })
                        {
                            string relative = ManifestJson.String(target, kind);
                            if (relative != null) AddDependency(root, sensitive, file, relative, kind, sources);
                        }
                        object spine = ManifestJson.Get(target, "spine");
                        if (spine != null)
                            AddDependency(root, sensitive, file, Required(ManifestJson.Object(spine), "json"), "json", sources);
                    }
                    else throw new InvalidDataException("Target type must be texture or spine.");
                    if (!identities.Add(identity)) throw new InvalidDataException("Duplicate target in package: " + id);
                }
            }
            return sources;
        }

        private static void AddDependency(string root, string sensitive, string manifest, string relative,
            string kind, Dictionary<string, byte[]> sources)
        {
            string path = ReplacementResourcePaths.Resolve(root, Path.GetDirectoryName(manifest), relative);
            if (ReplacementResourcePaths.Within(sensitive, path) != ReplacementResourcePaths.Within(sensitive, manifest))
                throw new InvalidDataException("A package and all dependencies must stay in the same normal or Sensitive tree.");
            if (!File.Exists(path)) throw new FileNotFoundException("Replacement resource was not found.", path);
            if (kind == "image")
            {
                var header = ReplacementResourceIO.ReadPrefix(path, 33);
                byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
                if (header.Length < 33 || !header.Take(8).SequenceEqual(signature)
                    || header[12] != 73 || header[13] != 72 || header[14] != 68 || header[15] != 82)
                    throw new InvalidDataException("Expected PNG with IHDR: " + path);
            }
            else if (kind == "json") ManifestJson.Parse(ReplacementResourceIO.ReadText(path));
            else if (string.IsNullOrWhiteSpace(ReplacementResourceIO.ReadText(path)))
                throw new InvalidDataException("Empty atlas: " + path);
            Add(sources, path);
        }

        private static void Add(Dictionary<string, byte[]> sources, string path)
        {
            if (!sources.ContainsKey(path)) sources.Add(path, Hash(path));
        }

        private static byte[] Hash(string path)
        {
            using (var stream = ReplacementResourceIO.OpenRead(path))
            using (var sha = SHA256.Create()) return sha.ComputeHash(stream);
        }

        private static string Required(Dictionary<string, object> value, string key)
        {
            string text = ManifestJson.String(value, key);
            if (string.IsNullOrWhiteSpace(text) || text.Contains('\r') || text.Contains('\n'))
                throw new InvalidDataException("Missing or invalid " + key);
            return text;
        }

        private static IEnumerable<string> Enumerate(string directory)
        {
            ReplacementResourcePaths.RejectLink(directory);
            foreach (string path in Directory.EnumerateFileSystemEntries(directory))
            {
                ReplacementResourcePaths.RejectLink(path);
                if (Directory.Exists(path))
                {
                    foreach (string child in Enumerate(path)) yield return child;
                }
                else yield return path;
            }
        }

        private static void RemoveStaging(string staging, string parent)
        {
            string full = Path.GetFullPath(staging);
            if (!string.Equals(Path.GetDirectoryName(full), Path.GetFullPath(parent), StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(full).StartsWith(".be-encrypt-", StringComparison.Ordinal))
                throw new IOException("Refusing to remove an unexpected temporary directory: " + full);
            ReplacementResourcePaths.CheckAncestors(full);
            // 删除前检查完整目录树，不跟随目录链接；这里只删除本次创建的暂存目录。
            foreach (string path in Enumerate(full)) ReplacementResourcePaths.RejectLink(path);
            Directory.Delete(full, true);
        }
    }
}
