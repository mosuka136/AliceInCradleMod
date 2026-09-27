using System;
using System.IO;

namespace BetterExperience.Patches.ReplaceTexture
{
    // 客户端自动解密用的内置密钥，不构成授权或防逆向边界。新版本增加编号，不修改旧编号的密钥。
    internal static class ReplacementResourceKeys
    {
        internal const uint CurrentId = 1;

        internal static void Get(uint id, out byte[] encryption, out byte[] authentication)
        {
            if (id != 1) throw new InvalidDataException("Unknown encrypted resource key id: " + id);
            encryption = Convert.FromBase64String("Hk477dhmHZbpOEOEfyl2mNvXxz0V0OeXOp4teqrU0zQ=");
            authentication = Convert.FromBase64String("uYRrTYvTF5Nh+vBJSHn7JuHBz9H+YezT9IjaVVGfvZA=");
        }
    }
}
