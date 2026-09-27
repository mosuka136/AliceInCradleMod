using BetterExperience.Patches.ReplaceTexture;
using System.Security.Cryptography;
using System.Text;

namespace BetterExperience.Test.Patches.ReplaceTexture
{
    public sealed class EncryptedResourceTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "be-crypto-" + Guid.NewGuid().ToString("N"));
        public EncryptedResourceTests() => Directory.CreateDirectory(directory);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(33)]
        [InlineData(81921)]
        public void Codec_RoundTripsBoundaryLengthsAndPartialReads(int length)
        {
            byte[] original = Enumerable.Range(0, length).Select(i => (byte)(i * 17)).ToArray();
            string path = Write("resource.png", Encrypt(original));

            Assert.Equal(original, ReplacementResourceIO.ReadBytes(path));
            Assert.Equal(original.Take(33), ReplacementResourceIO.ReadPrefix(path, 33));
            Assert.Empty(ReplacementResourceIO.ReadPrefix(path, 0));
            Assert.Equal(original, ReplacementResourceIO.ReadPrefix(path, length + 10));
            using (var decoded = ReplacementResourceIO.OpenRead(path))
            {
                Assert.Equal(length, decoded.Length);
                Assert.Equal(length == 0 ? -1 : original[0], decoded.ReadByte());
                if (OperatingSystem.IsWindows())
                    Assert.Throws<IOException>(() => File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
            }
            // 包含部分读取和零长度读取的路径都必须释放文件句柄。
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Reader_PreservesPlainBytesAndBomAwareText(bool encrypted)
        {
            foreach (var encoding in new Encoding[] { new UTF8Encoding(false), new UTF8Encoding(true), Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 })
            {
                const string text = "{\"名称\":\"立绘\"}\r\n";
                byte[] original = encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
                string path = Write("text.json", encrypted ? Encrypt(original) : original);
                Assert.Equal(text, ReplacementResourceIO.ReadText(path));
                Assert.Equal(original, ReplacementResourceIO.ReadBytes(path));
                Assert.Equal(original.Take(7), ReplacementResourceIO.ReadPrefix(path, 7));
            }
            string empty = Write("empty", Array.Empty<byte>());
            Assert.Empty(ReplacementResourceIO.ReadBytes(empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReplacementResourceIO.ReadPrefix(empty, -1));
            Assert.Throws<FileNotFoundException>(() => ReplacementResourceIO.ReadBytes(Path.Combine(directory, "missing")));
        }

        [Fact]
        public void Codec_UsesRandomIvAndMatchesIndependentAesAndHmac()
        {
            var original = Encoding.UTF8.GetBytes("立绘资源\r\n独立校验");
            var encoded = Encrypt(original);
            Assert.False(encoded.SequenceEqual(Encrypt(original)));
            using var reader = new BinaryReader(new MemoryStream(encoded));
            Assert.Equal("BEREENC\0", Encoding.ASCII.GetString(reader.ReadBytes(8)));
            Assert.Equal(1, reader.ReadByte());
            Assert.Equal(1u, reader.ReadUInt32());
            byte[] iv = reader.ReadBytes(16);
            ulong length = reader.ReadUInt64();
            Assert.Equal((ulong)encoded.Length - 69, length);
            ReplacementResourceKeys.Get(1, out var key, out var macKey);
            using var hmac = new HMACSHA256(macKey);
            Assert.Equal(encoded.TakeLast(32), hmac.ComputeHash(encoded, 0, encoded.Length - 32));
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var decryptor = aes.CreateDecryptor();
            Assert.Equal(original, decryptor.TransformFinalBlock(encoded, 37, (int)length));
        }

        [Fact]
        public void Reader_AcceptsIndependentlyProducedEnvelope()
        {
            byte[] original = Encoding.UTF8.GetBytes("测试另一端生成的密文");
            ReplacementResourceKeys.Get(1, out var key, out var macKey);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            byte[] cipher = encryptor.TransformFinalBlock(original, 0, original.Length);
            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output, Encoding.UTF8, true);
            writer.Write(Encoding.ASCII.GetBytes("BEREENC\0"));
            writer.Write((byte)1);
            writer.Write(1u);
            writer.Write(aes.IV);
            writer.Write((ulong)cipher.Length);
            writer.Write(cipher);
            using var hmac = new HMACSHA256(macKey);
            writer.Write(hmac.ComputeHash(output.ToArray()));
            Assert.Equal(original, ReplacementResourceIO.ReadBytes(Write("independent", output.ToArray())));
        }

        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(13)]
        [InlineData(29)]
        [InlineData(37)]
        [InlineData(-1)]
        public void Reader_RejectsAlteredVersionKeyIvLengthCiphertextAndTag(int offset)
        {
            var encoded = Encrypt(new byte[100]);
            encoded[offset < 0 ? encoded.Length - 1 : offset] ^= 0x40;
            string path = Write("damaged", encoded);
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadBytes(path));
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadPrefix(path, 1));
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadText(path));
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        }

        [Theory]
        [InlineData(8)]
        [InlineData(36)]
        [InlineData(84)]
        public void Reader_RejectsTruncatedEnvelope(int length)
        {
            string path = Write("truncated", Encrypt(new byte[100]).Take(length).ToArray());
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadBytes(path));
        }

        [Fact]
        public void Reader_RejectsOverflowTrailingDataAndInvalidAuthenticatedPadding()
        {
            byte[] encoded = Encrypt(new byte[100]);
            var overflow = (byte[])encoded.Clone();
            Array.Fill(overflow, (byte)255, 29, 8);
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadPrefix(Write("overflow", overflow), 1));
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadBytes(Write("trailing", encoded.Concat(new byte[1]).ToArray())));
            // 无明文的文件只有一个全填充块；修改 IV 使末字节从 16 变成 17，再重算 MAC。
            var badPadding = Encrypt(Array.Empty<byte>());
            badPadding[28] ^= 1;
            ReplacementResourceKeys.Get(1, out _, out var macKey);
            using var hmac = new HMACSHA256(macKey);
            hmac.ComputeHash(badPadding, 0, badPadding.Length - 32).CopyTo(badPadding, badPadding.Length - 32);
            Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadPrefix(Write("padding", badPadding), 0));
        }

        [Fact]
        public void Reader_AuthenticatesAllCiphertextBeforeInspectingPaddingOrReturningPrefix()
        {
            var encoded = Encrypt(new byte[200000]);
            encoded[encoded.Length - 33] ^= 1;
            string path = Write("corrupt-tail.png", encoded);
            var error = Assert.Throws<InvalidDataException>(() => ReplacementResourceIO.ReadPrefix(path, 33));
            Assert.Contains("authentication failed", error.Message);
            Assert.Contains(path, error.Message);
        }

        [Fact]
        public void Encrypt_RejectsInvalidStreamsWithoutOverwritingDestination()
        {
            using var source = new MemoryStream(new byte[1]);
            using var destination = new MemoryStream(new byte[] { 42 });
            Assert.Throws<ArgumentException>(() => ReplacementResourceIO.Encrypt(source, destination));
            Assert.Equal(new byte[] { 42 }, destination.ToArray());
            Assert.Throws<ArgumentException>(() => ReplacementResourceIO.Encrypt(null, destination));
        }

        private static byte[] Encrypt(byte[] source)
        {
            using var input = new MemoryStream(source);
            using var output = new MemoryStream();
            ReplacementResourceIO.Encrypt(input, output);
            return output.ToArray();
        }

        private string Write(string name, byte[] content)
        {
            string path = Path.Combine(directory, name);
            File.WriteAllBytes(path, content);
            return path;
        }

        public void Dispose()
        {
            Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), Path.GetFullPath(directory));
            Assert.StartsWith("be-crypto-", Path.GetFileName(directory));
            Directory.Delete(directory, true);
        }
    }
}
