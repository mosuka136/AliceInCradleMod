using System;
using System.Collections.Generic;
using System.IO;

namespace BetterExperience.Patches.ReplaceTexture
{
    // 与作者工具共享路径约束，不依赖游戏、Spine 或 Unity 程序集。
    internal static class ReplacementResourcePaths
    {
        internal static string Resolve(string root, string directory, string relative, HashSet<string> verified = null)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0)
                throw new InvalidDataException("Expected relative resource path: " + relative);
            string full = Path.GetFullPath(Path.Combine(directory, relative));
            if (!Within(root, full)) throw new InvalidDataException("Resource escapes ReplaceTexture: " + relative);
            for (string path = full; path != null && Within(root, path); path = Path.GetDirectoryName(path))
            {
                if (verified != null && verified.Contains(path)) continue;
                RejectLink(path);
                verified?.Add(path);
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

        internal static void RejectLink(string path)
        {
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Resource uses a reparse point: " + path);
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
        }

        internal static void CheckAncestors(string path)
        {
            for (string current = Path.GetFullPath(path); current != null; current = Path.GetDirectoryName(current))
                RejectLink(current);
        }
    }
}
