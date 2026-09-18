using BepInEx;
using System.IO;
using UnityModBase.HTranslatorSpace;

namespace $safeprojectname$
{
    /// <summary>
    /// 定义插件在 BepInEx、Harmony 和文件系统中的固定标识与目录约定。
    /// 该类型只保存启动阶段和资源加载阶段共享的常量/路径，不负责创建目录或验证文件存在性。
    /// </summary>
    public class PatchInfo
    {
        // 显示名：第一个参数是中文，第二个是英文；按需改成你的 Mod 名称。
        public static readonly Translator UserName = new Translator("$safeprojectname$", "$safeprojectname$");

        // 插件 GUID：默认 com.example.$safeprojectname$，请在发布前改成你自己的前缀（如 com.你的名字.xxx）。
        public const string BepInPluginId = "$modguidprefix$.$safeprojectname$";
        public const string BepInPluginVersion = "1.0.0";

        public const string HarmonyPluginId = "$modguidprefix$.$safeprojectname$";
        public const string HarmonyPluginVersion = "1.0.0";

        public static readonly string PluginPath = Path.Combine(Paths.PluginPath, nameof($safeprojectname$));

        public static readonly string ConfigFilePath = Path.Combine(PluginPath, $"{nameof($safeprojectname$)}.cfg");

        public static readonly string LoggerPath = Path.Combine(PluginPath, "logs");
        public const string LoggerName = "$safeprojectname$.log";
    }
}
