using BetterExperience.Patches.ReplaceTexture;
using System.Text;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    internal sealed class EncryptedPackFixture : IDisposable
    {
        internal readonly string DirectoryPath = Path.Combine(Path.GetTempPath(), "be-encrypted-pack-" + Guid.NewGuid().ToString("N"));
        internal string Input => Path.Combine(DirectoryPath, "input");
        internal string Output => Path.Combine(DirectoryPath, "output");
        internal const string SpineIdentity = "spine\nstand_normal\nstand_normal";
        internal const string TextureIdentity = "texture\nresources\nUI/Icon\nTexture2D";
        internal const string Atlas = "page.png\nsize: 1,1\nfilter: Linear,Linear\npart\nbounds: 0,0,1,1\n";
        internal const string Skeleton = """
            {"skeleton":{"spine":"4.1.24","hash":"test"},"bones":[{"name":"root"}],
            "slots":[{"name":"body","bone":"root","attachment":"part"}],
            "skins":[{"name":"default","attachments":{"body":{"part":{"width":1,"height":1}}}}],
            "animations":{"stand":{}}}
            """;
        internal static byte[] Png => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        internal EncryptedPackFixture() => Directory.CreateDirectory(Input);

        internal void CreatePack(int encrypted = 0, string folder = "", string id = "立绘包")
        {
            string prefix = string.IsNullOrEmpty(folder) ? "" : folder + "/";
            Write(prefix + "page.png", Png, (encrypted & 2) != 0);
            WriteText(prefix + "page.atlas", Atlas, (encrypted & 4) != 0);
            WriteText(prefix + "page.json", Skeleton, (encrypted & 8) != 0);
            WriteText(prefix + "pack.replacement.json", Manifest(id), (encrypted & 1) != 0);
        }

        internal static string Manifest(string id) => $$$"""
            {"formatVersion":2,"id":"{{{id}}}","targets":[
            {"type":"texture","loader":"resources","path":"UI/Icon","objectType":"Texture2D","image":"page.png"},
            {"type":"texture","loader":"mti","assetKey":"UI/Sheet","image":"page.png"},
            {"type":"spine","key":"stand_normal","jsonKey":"stand_normal","image":"page.png","atlas":"page.atlas",
             "spine":{"json":"page.json","replace":["all"]}}]}
            """;

        internal string WriteText(string relative, string text, bool encrypted = false)
            => Write(relative, new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(text)).ToArray(), encrypted);

        internal string Write(string relative, byte[] bytes, bool encrypted = false)
        {
            string path = Path.Combine(Input, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (encrypted)
            {
                using var source = new MemoryStream(bytes);
                using var output = new MemoryStream();
                ReplacementResourceIO.Encrypt(source, output);
                bytes = output.ToArray();
            }
            File.WriteAllBytes(path, bytes);
            return path;
        }

        internal ReplacementCatalog Discover(bool allowSensitive = true, string root = null)
            => ReplacementCatalog.Discover(root ?? Input, Path.Combine(root ?? Input, "Sensitive"), allowSensitive);

        internal void Corrupt(string relative)
        {
            string path = Path.Combine(Input, relative);
            byte[] bytes = File.ReadAllBytes(path);
            bytes[bytes.Length - 1] ^= 1;
            File.WriteAllBytes(path, bytes);
        }

        internal void AssertNoOutput()
        {
            Assert.False(Directory.Exists(Output));
            Assert.Empty(Directory.GetDirectories(DirectoryPath, ".be-encrypt-*"));
        }

        public void Dispose()
        {
            Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())), Path.GetDirectoryName(DirectoryPath));
            Assert.StartsWith("be-encrypted-pack-", Path.GetFileName(DirectoryPath));
            Directory.Delete(DirectoryPath, true);
        }
    }
}
