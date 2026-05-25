using BepInEx;
using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using BetterExperience.HProvider;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterExperience
{
    /// <summary>
    /// 定义插件在 BepInEx、Harmony 和文件系统中的固定标识与目录约定。
    /// 该类型只保存启动阶段和资源加载阶段共享的常量/路径，不负责创建目录或验证文件存在性。
    /// </summary>
    public class PatchInfo
    {
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

    /// <summary>
    /// BepInEx 插件入口，负责按启动顺序初始化配置、日志、贴图替换和 Harmony 补丁。
    /// 该类只协调插件级生命周期，不承载具体补丁逻辑；具体行为由 <c>Patches</c> 下的 Harmony Patch 类实现。
    /// Unity 生命周期方法由主线程调用，当前实现未设计为从后台线程重复初始化。
    /// </summary>
    [BepInPlugin(PatchInfo.BepInPluginId, nameof(BetterExperience), PatchInfo.BepInPluginVersion)]
    public class BetterExperience : BaseUnityPlugin
    {
        private void Awake()
        {
            try
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;

                // 配置必须先于日志和补丁读取，因为后续初始化会依赖开关、日志等级和资源路径设置。
                ConfigManager.Initialize(PatchInfo.ConfigFilePath);

                if (ConfigManager.EnableBetterExperience.Value)
                    Logger.LogWarning($"{nameof(BetterExperience)} Enabled!");
                else
                {
                    Logger.LogWarning($"{nameof(BetterExperience)} Disabled!");
                    return;
                }

                InitializeLog();

                HLog.Info($"{nameof(BetterExperience)} startup initialized. Version={PatchInfo.BepInPluginVersion}");

                TextureManager.Initialize(PatchInfo.ReplaceImagePath, PatchInfo.ReplaceSensitiveImagePath, PatchInfo.ReplaceImageSupportedExtensions);

                // Harmony 注册失败时跳过单个补丁类，避免某个补丁因目标方法变更导致整个插件不可用。
                var harmony = new Harmony(PatchInfo.HarmonyPluginId);
                HLog.Debug($"Starting Harmony patch registration: {PatchInfo.HarmonyPluginId}");
                PatchAll(harmony, typeof(BetterExperience).Assembly);
                LogPatchesInfo(harmony);
                HLog.Info("Harmony patch registration completed.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to patch!\n{ex}");
                HLog.Error("Failed to patch!", ex);
            }
        }

        private void Update()
        {
        }

        public void InitializeLog()
        {
            var unityProvider = new UnityProvider();

            HLog.EnableLog = ConfigManager.EnableHLog.Value;
            ConfigManager.EnableHLog.OnValueChanged += (s, e) => HLog.EnableLog = ConfigManager.EnableHLog.Value;
            ConfigManager.HLogLevel.OnValueChanged += (s, e) => HLog.HLogLevel = ConfigManager.HLogLevel.Value;
            HLog.Initialize(PatchInfo.LoggerPath, PatchInfo.LoggerName, ConfigManager.HLogLevel.Value, unityProvider);

            ConfigManager.BepInExLogLevel.OnValueChanged += (s, e) => BepInExHLog.LogLevel = ConfigManager.BepInExLogLevel.Value;
            BepInExHLog.Initialize(ConfigManager.BepInExLogLevel.Value, unityProvider, new BepInExLoggerProvider(Logger));

            HLog.OnLogAdd += BepInExHLog.Log;
        }

        /// <summary>
        /// 扫描程序集中的 Harmony Patch 类型并逐个注册。
        /// 如果某个类型因为游戏版本差异或反射异常注册失败，只记录错误并继续处理其他补丁。
        /// </summary>
        /// <param name="harmony">用于注册补丁的 Harmony 实例，调用方负责保证其标识与插件一致。</param>
        /// <param name="asm">需要扫描的程序集，通常为当前插件程序集。</param>
        public void PatchAll(Harmony harmony, Assembly asm)
        {
            int patchClassCount = 0;

            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                HLog.Warn($"ReflectionTypeLoadException occurred while getting assembly types: {asm.FullName}");
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var t in types)
            {
                if (t == null)
                    continue;

                try
                {
                    if (!t.GetCustomAttributes(true).Any(a => a.GetType().Name.StartsWith(nameof(HarmonyPatch))))
                        continue;
                    harmony.CreateClassProcessor(t).Patch();
                    patchClassCount++;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Skip patch class: {t.FullName}", ex);
                }
            }

            HLog.Debug($"Scanned and processed Harmony patch classes: {patchClassCount}");
        }

        /// <summary>
        /// 输出当前 Harmony 实例实际注册的补丁统计信息。
        /// 统计按 owner 过滤，只计入本插件标识下的 prefix、postfix、transpiler 和 finalizer。
        /// </summary>
        /// <param name="harmony">要检查的 Harmony 实例。</param>
        public void LogPatchesInfo(Harmony harmony)
        {
            string id = harmony.Id;

            int patchedMethodCount = 0;
            int prefixCount = 0;
            int postfixCount = 0;
            int transpilerCount = 0;
            int finalizerCount = 0;

            foreach (var original in harmony.GetPatchedMethods())
            {
                var info = Harmony.GetPatchInfo(original);
                if (info == null)
                    continue;

                patchedMethodCount++;
                HLog.Debug($"Original: {original.DeclaringType.FullName}.{original.Name}");

                CountList(info.Prefixes, ref prefixCount);
                CountList(info.Postfixes, ref postfixCount);
                CountList(info.Transpilers, ref transpilerCount);
                CountList(info.Finalizers, ref finalizerCount);

                void CountList(IList<Patch> patches, ref int count)
                {
                    if (patches == null)
                        return;

                    foreach (var p in patches)
                    {
                        if (p.owner != id)
                            continue;

                        count++;
                        HLog.Debug($"    {p.PatchMethod.DeclaringType.FullName}.{p.PatchMethod.Name} ({p.PatchMethod.MetadataToken:X8})");
                    }
                }
            }

            HLog.Info($"Total patched methods: {patchedMethodCount}");
            HLog.Debug($"Total prefixes: {prefixCount}");
            HLog.Debug($"Total postfixes: {postfixCount}");
            HLog.Debug($"Total transpilers: {transpilerCount}");
            HLog.Debug($"Total finalizers: {finalizerCount}");
            HLog.Info($"Total patch methods: {prefixCount + postfixCount + transpilerCount + finalizerCount}");
        }
    }
}
