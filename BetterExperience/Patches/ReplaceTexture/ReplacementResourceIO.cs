using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BetterExperience.Patches.ReplaceTexture
{
    /// <summary>明文与 BEREENC v1 资源的统一读取入口。只使用框架 API，不依赖 Unity。</summary>
    internal static class ReplacementResourceIO
    {
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("BEREENC\0");
        private const byte Version = 1;
        private const int HeaderSize = 37;
        private const int TagSize = 32;
        private const int BlockSize = 16;

        internal static byte[] ReadBytes(string path)
        {
            using (var stream = OpenRead(path))
            {
                if (stream.Length > int.MaxValue) throw new IOException("Resource is too large: " + path);
                var bytes = new byte[(int)stream.Length];
                ReadExactly(stream, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        internal static string ReadText(string path)
        {
            using (var stream = OpenRead(path))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                return reader.ReadToEnd();
        }

        internal static byte[] ReadPrefix(string path, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            using (var stream = OpenRead(path))
            {
                var bytes = new byte[(int)Math.Min(stream.Length, count)];
                ReadExactly(stream, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        internal static Stream OpenRead(string path)
        {
            // 校验、解密共用句柄；读取期间不允许写入或替换文件。
            var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Aes aes = null;
            ICryptoTransform transform = null;
            byte[] encryption = null, authentication = null;
            try
            {
                var prefix = new byte[Magic.Length];
                int count = ReadUpTo(file, prefix, 0, prefix.Length);
                file.Position = 0;
                if (count != Magic.Length || !Equal(prefix, Magic)) return file;
                if (file.Length < HeaderSize + BlockSize + TagSize)
                    throw new InvalidDataException("Truncated encrypted resource.");

                byte[] iv;
                ulong length;
                using (var reader = new BinaryReader(file, Encoding.UTF8, true))
                {
                    reader.ReadBytes(Magic.Length);
                    if (reader.ReadByte() != Version) throw new InvalidDataException("Unsupported encrypted resource version.");
                    ReplacementResourceKeys.Get(reader.ReadUInt32(), out encryption, out authentication);
                    iv = reader.ReadBytes(BlockSize);
                    length = reader.ReadUInt64();
                }
                // 与真实长度比较后才转换类型或分配内存，拒绝溢出、截断和附加数据。
                if (length < BlockSize || length % BlockSize != 0
                    || length != (ulong)(file.Length - HeaderSize - TagSize))
                    throw new InvalidDataException("Invalid encrypted resource length.");
                long cipherLength = (long)length;
                var actual = Authenticate(file, authentication, HeaderSize + cipherLength);
                var expected = new byte[TagSize];
                ReadExactly(file, expected, 0, expected.Length);
                if (!Equal(actual, expected)) throw new InvalidDataException("Encrypted resource authentication failed.");

                aes = Aes.Create();
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Key = encryption;
                // 认证后单独校验末块填充并取得明文长度。前缀读取无需解密整张图片，
                // 也避免旧版 Mono/.NET Framework 的 CryptoStream 在提前 Dispose 时校验不完整的填充。
                byte[] lastIv = iv;
                file.Position = HeaderSize + cipherLength - BlockSize;
                if (cipherLength > BlockSize)
                {
                    file.Position -= BlockSize;
                    lastIv = new byte[BlockSize];
                    ReadExactly(file, lastIv, 0, lastIv.Length);
                }
                var lastBlock = new byte[BlockSize];
                ReadExactly(file, lastBlock, 0, lastBlock.Length);
                aes.IV = lastIv;
                aes.Padding = PaddingMode.PKCS7;
                long plainLength;
                using (var tail = aes.CreateDecryptor())
                {
                    var decoded = tail.TransformFinalBlock(lastBlock, 0, lastBlock.Length);
                    plainLength = cipherLength - BlockSize + decoded.Length;
                    Array.Clear(decoded, 0, decoded.Length);
                }
                aes.IV = iv;
                aes.Padding = PaddingMode.None;
                transform = aes.CreateDecryptor();
                file.Position = HeaderSize;
                var ciphertext = new LimitedReadStream(file, cipherLength);
                var decodedStream = new CryptoStream(ciphertext, transform, CryptoStreamMode.Read);
                return new LimitedReadStream(decodedStream, plainLength, transform, aes);
            }
            catch (Exception ex)
            {
                transform?.Dispose();
                aes?.Dispose();
                file.Dispose();
                if (ex is CryptographicException || ex is InvalidDataException)
                    throw new InvalidDataException("Invalid encrypted resource: " + path + ": " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (encryption != null) Array.Clear(encryption, 0, encryption.Length);
                if (authentication != null) Array.Clear(authentication, 0, authentication.Length);
            }
        }

        /// <summary>输出流必须可读写、可寻址且为空；调用者负责原子发布文件。</summary>
        internal static void Encrypt(Stream plaintext, Stream output)
        {
            if (plaintext == null || !plaintext.CanRead) throw new ArgumentException("Readable source required.", nameof(plaintext));
            if (output == null || !output.CanRead || !output.CanWrite || !output.CanSeek || output.Length != 0)
                throw new ArgumentException("Empty, readable, writable and seekable destination required.", nameof(output));
            ReplacementResourceKeys.Get(ReplacementResourceKeys.CurrentId, out var encryption, out var authentication);
            try
            {
                output.Position = 0;
                var iv = new byte[BlockSize];
                using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(iv);
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
                {
                    writer.Write(Magic);
                    writer.Write(Version);
                    writer.Write(ReplacementResourceKeys.CurrentId);
                    writer.Write(iv);
                    writer.Write(0UL);
                }
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = encryption;
                    aes.IV = iv;
                    using (var encryptor = aes.CreateEncryptor())
                    using (var crypto = new CryptoStream(output, encryptor, CryptoStreamMode.Write, true))
                    {
                        plaintext.CopyTo(crypto);
                        crypto.FlushFinalBlock();
                    }
                }
                long cipherLength = output.Position - HeaderSize;
                output.Position = HeaderSize - sizeof(ulong);
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true)) writer.Write((ulong)cipherLength);
                byte[] tag = Authenticate(output, authentication, HeaderSize + cipherLength);
                output.Write(tag, 0, tag.Length);
                output.Flush();
            }
            finally
            {
                Array.Clear(encryption, 0, encryption.Length);
                Array.Clear(authentication, 0, authentication.Length);
            }
        }

        private static byte[] Authenticate(Stream stream, byte[] key, long count)
        {
            stream.Position = 0;
            using (var hmac = new HMACSHA256(key))
            {
                var buffer = new byte[81920];
                while (count > 0)
                {
                    int read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                    if (read == 0) throw new InvalidDataException("Truncated encrypted resource.");
                    hmac.TransformBlock(buffer, 0, read, buffer, 0);
                    count -= read;
                }
                hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return hmac.Hash;
            }
        }

        private static bool Equal(byte[] left, byte[] right)
        {
            if (left.Length != right.Length) return false;
            int difference = 0;
            for (int i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private static int ReadUpTo(Stream stream, byte[] buffer, int offset, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = stream.Read(buffer, offset + total, count - total);
                if (read == 0) break;
                total += read;
            }
            return total;
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            if (ReadUpTo(stream, buffer, offset, count) != count) throw new InvalidDataException("Truncated resource.");
        }

        private sealed class LimitedReadStream : Stream
        {
            private readonly Stream inner;
            private readonly long length;
            private readonly IDisposable[] owners;
            private long remaining;
            private bool disposed;

            internal LimitedReadStream(Stream inner, long length, params IDisposable[] owners)
            {
                this.inner = inner;
                this.length = remaining = length;
                this.owners = owners;
            }

            public override bool CanRead => !disposed && inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => length;
            public override long Position { get => length - remaining; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (disposed) throw new ObjectDisposedException(nameof(LimitedReadStream));
                if (buffer == null) throw new ArgumentNullException(nameof(buffer));
                if (offset < 0 || count < 0 || offset > buffer.Length - count) throw new ArgumentOutOfRangeException();
                if (remaining == 0 || count == 0) return 0;
                int read = inner.Read(buffer, offset, (int)Math.Min(count, remaining));
                if (read == 0) throw new InvalidDataException("Truncated resource.");
                remaining -= read;
                return read;
            }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            protected override void Dispose(bool disposing)
            {
                if (disposing && !disposed)
                {
                    disposed = true;
                    try { inner.Dispose(); }
                    finally { foreach (var owner in owners) owner.Dispose(); }
                }
                base.Dispose(disposing);
            }
        }
    }
}
