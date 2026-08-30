using BepInEx;
using System.IO;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience
{
    /// <summary>
    /// 定义插件在 BepInEx、Harmony 和文件系统中的固定标识与目录约定。
    /// 该类型只保存启动阶段和资源加载阶段共享的常量/路径，不负责创建目录或验证文件存在性。
    /// </summary>
    public class PatchInfo
    {
        public static readonly Translator UserName = new Translator("更好的体验", "BetterExperience");

        public const string BepInPluginId = "com.buele.betterexperience";
        public const string BepInPluginVersion = "2.1.1";

        public const string HarmonyPluginId = "com.buele.betterexperience";
        public const string HarmonyPluginVersion = "2.1.1";

        public static readonly string PluginPath = Path.Combine(Paths.PluginPath, nameof(BetterExperience));

        public static readonly string ConfigFilePath = Path.Combine(PluginPath, $"{nameof(BetterExperience)}.cfg");

        public static readonly string LoggerPath = Path.Combine(PluginPath, "logs");
        public const string LoggerName = "BetterExperience.log";

        public static readonly string ReplaceImagePath = Path.Combine(PluginPath, "ReplaceTexture");
        public static readonly string ReplaceSensitiveImagePath = Path.Combine(ReplaceImagePath, "Sensitive");
        public static readonly string[] ReplaceImageSupportedExtensions = { ".png", ".btep" };
    }
}
