extern alias ResourceEncryptor;

using BetterExperience.Patches.ReplaceTexture;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using EncryptCommand = ResourceEncryptor::BetterExperience.ResourceEncryptor.Program;
using PackEncryptor = ResourceEncryptor::BetterExperience.ResourceEncryptor.PackEncryptor;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class ResourceEncryptorTests : IDisposable
    {
        private readonly EncryptedPackFixture pack = new EncryptedPackFixture();

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(15)]
        public void Encrypt_ExportsOnlyReferencedFilesAndPreservesSource(int encrypted)
        {
            pack.CreatePack(encrypted, "中文目录");
            pack.CreatePack(encrypted, "Sensitive/secret", "secret");
            pack.WriteText("中文目录/second.replacement.json", EncryptedPackFixture.Manifest("second"));
            pack.WriteText("unused.psd", "private source art");
            pack.WriteText("legacy.portrait.json", "not a v2 package");
            var original = Snapshot(pack.Input);
            var decoded = Directory.GetFiles(pack.Input, "*", SearchOption.AllDirectories)
                .ToDictionary(path => Path.GetRelativePath(pack.Input, path), ReplacementResourceIO.ReadBytes);
            using var output = new StringWriter();
            using var error = new StringWriter();

            int code = EncryptCommand.Run(new[] { "encrypt", "--output", pack.Output, "--input", pack.Input }, output, error);

            Assert.Equal(0, code);
            Assert.Equal("", error.ToString());
            var files = Directory.GetFiles(pack.Output, "*", SearchOption.AllDirectories);
            Assert.Equal(9, files.Length); // 三个清单、两组依赖；同一个 PNG 的多次引用只输出一次。
            Assert.False(File.Exists(Path.Combine(pack.Output, "unused.psd")));
            Assert.False(File.Exists(Path.Combine(pack.Output, "legacy.portrait.json")));
            foreach (string file in files)
            {
                Assert.Equal("BEREENC\0", Encoding.ASCII.GetString(File.ReadAllBytes(file), 0, 8));
                string relative = Path.GetRelativePath(pack.Output, file);
                Assert.Equal(decoded[relative], ReplacementResourceIO.ReadBytes(file));
                Assert.NotEqual(original[relative], Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))));
            }
            Assert.Equal(original, Snapshot(pack.Input));
            var catalog = pack.Discover(root: pack.Output);
            Assert.Empty(catalog.Errors);
            Assert.Equal(3, catalog.Packages.Count);
            Assert.Equal(2, pack.Discover(false, pack.Output).Packages.Count);
            Assert.Empty(Directory.GetDirectories(pack.DirectoryPath, ".be-encrypt-*"));
        }

        [Fact]
        public void Encrypt_PreservesSharedDependenciesAcrossSiblingDirectories()
        {
            pack.Write("shared/page.png", EncryptedPackFixture.Png, true);
            foreach (string folder in new[] { "first", "second" })
            {
                string manifest = $$$"""
                    {"formatVersion":2,"id":"{{{folder}}}","targets":[
                    {"type":"texture","loader":"mti","assetKey":"UI/Sheet","image":"../shared/page.png"}]}
                    """;
                pack.WriteText(folder + "/pack.replacement.json", manifest, folder == "first");
            }
            Assert.Equal(3, PackEncryptor.Encrypt(pack.Input, pack.Output));
            Assert.Equal(EncryptedPackFixture.Png, ReplacementResourceIO.ReadBytes(Path.Combine(pack.Output, "shared", "page.png")));
            var catalog = pack.Discover(root: pack.Output);
            Assert.Empty(catalog.Errors);
            Assert.Equal(2, catalog.Packages.Count);
            Assert.Single(catalog.Packages.SelectMany(package => package.Targets).Select(target => target.ImagePath).Distinct());
        }

        [Fact]
        public void Encrypt_CanReencryptItsOwnOutputWithoutDoubleWrapping()
        {
            pack.CreatePack();
            PackEncryptor.Encrypt(pack.Input, pack.Output);
            string second = Path.Combine(pack.DirectoryPath, "second");
            Assert.Equal(4, PackEncryptor.Encrypt(pack.Output, second));
            foreach (string source in Directory.GetFiles(pack.Input))
            {
                string file = Path.GetFileName(source);
                Assert.Equal(File.ReadAllBytes(source), ReplacementResourceIO.ReadBytes(Path.Combine(second, file)));
                Assert.False(File.ReadAllBytes(Path.Combine(pack.Output, file)).SequenceEqual(File.ReadAllBytes(Path.Combine(second, file))));
            }
        }

        [Fact]
        public void Encrypt_RejectsExistingOutputAndOverlappingDirectories()
        {
            pack.CreatePack();
            Directory.CreateDirectory(pack.Output);
            string marker = Path.Combine(pack.Output, "keep.txt");
            File.WriteAllText(marker, "keep");
            Assert.Throws<IOException>(() => PackEncryptor.Encrypt(pack.Input, pack.Output));
            Assert.Equal("keep", File.ReadAllText(marker));
            Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, pack.Input));
            Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, Path.Combine(pack.Input, "child")));
            Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, pack.DirectoryPath));
            Assert.Throws<DirectoryNotFoundException>(() => PackEncryptor.Encrypt(pack.Input, Path.Combine(pack.DirectoryPath, "missing", "output")));
            Assert.Throws<IOException>(() => PackEncryptor.Encrypt(pack.Input, marker));
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("corrupt-manifest")]
        [InlineData("corrupt-png")]
        [InlineData("version")]
        [InlineData("duplicate")]
        [InlineData("json")]
        [InlineData("atlas")]
        [InlineData("no-manifests")]
        public void Encrypt_PrevalidatesDependenciesWithoutPublishingPartialOutput(string failure)
        {
            pack.CreatePack(15);
            if (failure == "missing") File.Delete(Path.Combine(pack.Input, "page.png"));
            if (failure == "corrupt-manifest") pack.Corrupt("pack.replacement.json");
            if (failure == "corrupt-png") pack.Corrupt("page.png");
            if (failure == "version") pack.WriteText("pack.replacement.json", EncryptedPackFixture.Manifest("立绘包").Replace("\"formatVersion\":2", "\"formatVersion\":1"), true);
            if (failure == "duplicate") pack.WriteText("duplicate.replacement.json", EncryptedPackFixture.Manifest("立绘包"), true);
            if (failure == "json") pack.WriteText("page.json", "not JSON", true);
            if (failure == "atlas") pack.WriteText("page.atlas", " ", true);
            if (failure == "no-manifests") File.Delete(Path.Combine(pack.Input, "pack.replacement.json"));
            var original = Snapshot(pack.Input);
            using var output = new StringWriter();
            using var error = new StringWriter();
            Assert.Equal(1, EncryptCommand.Run(new[] { "encrypt", "--input", pack.Input, "--output", pack.Output }, output, error));
            Assert.Contains("Encryption failed:", error.ToString());
            Assert.Equal(original, Snapshot(pack.Input));
            pack.AssertNoOutput();
        }

        [Theory]
        [InlineData("Sensitive/page.png")]
        [InlineData("../outside.png")]
        [InlineData("C:/outside.png")]
        public void Encrypt_RejectsUnauthorizedDependencyPaths(string image)
        {
            pack.CreatePack(15);
            pack.Write("Sensitive/page.png", EncryptedPackFixture.Png, true);
            pack.WriteText("pack.replacement.json", EncryptedPackFixture.Manifest("立绘包").Replace("\"image\":\"page.png\"", "\"image\":\"" + image + "\""), true);
            Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, pack.Output));
            pack.AssertNoOutput();
        }

        [Fact]
        public void Encrypt_CleansItsStagingDirectoryAfterLateFailure()
        {
            pack.CreatePack(15);
            var original = Snapshot(pack.Input);
            int completed = 0;
            Assert.Throws<IOException>(() => PackEncryptor.Encrypt(pack.Input, pack.Output, _ =>
            {
                completed++;
                throw new IOException("Injected output failure");
            }));
            Assert.Equal(1, completed);
            Assert.Equal(original, Snapshot(pack.Input));
            pack.AssertNoOutput();
        }

        [Fact]
        public void Encrypt_RejectsSourceChangedAfterValidation()
        {
            pack.CreatePack(15);
            int completed = 0;
            var error = Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, pack.Output, _ =>
            {
                if (++completed == 1) pack.WriteText("page.json", "{\"changed\":true}");
            }));
            Assert.Contains("did not round-trip", error.Message);
            pack.AssertNoOutput();
        }

        [Fact]
        public void Encrypt_RejectsDirectoryLinksInInputAndOutputAncestors()
        {
            pack.CreatePack(15);
            string link = Path.Combine(pack.Input, "linked");
            string outside = Path.Combine(pack.DirectoryPath, "outside");
            Directory.CreateDirectory(outside);
            if (OperatingSystem.IsWindows())
            {
                // Junction 不需要管理员权限；删除时只移除链接本身。
                var start = new ProcessStartInfo("cmd.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (string argument in new[] { "/c", "mklink", "/J", link, outside }) start.ArgumentList.Add(argument);
                using var process = Process.Start(start);
                process.WaitForExit();
                Assert.True(process.ExitCode == 0, process.StandardError.ReadToEnd());
            }
            else Directory.CreateSymbolicLink(link, outside);
            try
            {
                Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(pack.Input, pack.Output));
                Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(link, pack.Output));
                Assert.Throws<InvalidDataException>(() => PackEncryptor.Encrypt(outside, Path.Combine(link, "output")));
                Assert.Throws<InvalidDataException>(() => ReplacementResourcePaths.Resolve(pack.Input, pack.Input, "linked/page.png"));
                pack.AssertNoOutput();
            }
            finally { Directory.Delete(link); }
            Assert.True(Directory.Exists(outside));
        }

        [Fact]
        public void Command_ReportsUsageAndArgumentErrors()
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            Assert.Equal(0, EncryptCommand.Run(new[] { "--help" }, output, error));
            Assert.Contains("encrypt --input", output.ToString());
            Assert.Equal(2, EncryptCommand.Run(Array.Empty<string>(), output, error));
            Assert.Equal(2, EncryptCommand.Run(new[] { "decrypt", "--input", pack.Input, "--output", pack.Output }, output, error));
            Assert.Equal(2, EncryptCommand.Run(new[] { "encrypt", "--input", pack.Input, "--input", pack.Output }, output, error));
            Assert.Equal(2, EncryptCommand.Run(new[] { "encrypt", "--input", " ", "--output", pack.Output }, output, error));
            pack.AssertNoOutput();
        }

        [Fact]
        public void ToolAssembly_ReferencesOnlyFrameworkAssemblies()
        {
            Assert.All(typeof(EncryptCommand).Assembly.GetReferencedAssemblies(), reference => Assert.StartsWith("System", reference.Name));
            Assert.DoesNotContain(typeof(ReplacementCatalog).Assembly.GetReferencedAssemblies(), reference => reference.Name.Contains("ResourceEncryptor"));
        }

        private static SortedDictionary<string, string> Snapshot(string root) => new SortedDictionary<string, string>(
            Directory.GetFiles(root, "*", SearchOption.AllDirectories).ToDictionary(path => Path.GetRelativePath(root, path),
                path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));

        public void Dispose() => pack.Dispose();
    }
}
