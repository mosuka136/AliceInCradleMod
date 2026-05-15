using BepInEx;
using BetterExperience.BConfigManager;
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
    public class PatchInfo
    {
        public const string BepInPluginId = "com.buele.betterexperience";
        public const string BepInPluginVersion = "2.1.0";

        public const string HarmonyPluginId = "com.buele.betterexperience";
        public const string HarmonyPluginVersion = "2.1.0";

        public static readonly string PluginPath = Path.Combine(Paths.PluginPath, nameof(BetterExperience));

        public static readonly string ConfigFilePath = Path.Combine(PluginPath, $"{nameof(BetterExperience)}.cfg");

        public static readonly string LoggerPath = Path.Combine(PluginPath, "logs");
        public const string LoggerName = "BetterExperience.log";

        public static readonly string ReplaceImagePath = Path.Combine(PluginPath, "ReplaceTexture");
        public static readonly string ReplaceSensitiveImagePath = Path.Combine(ReplaceImagePath, "Sensitive");
        public static readonly string[] ReplaceImageSupportedExtensions = { ".png", ".btep" };
    }

    [BepInPlugin(PatchInfo.BepInPluginId, nameof(BetterExperience), PatchInfo.BepInPluginVersion)]
    public class BetterExperience : BaseUnityPlugin
    {
        public static BetterExperience Instance { get; private set; }

        private void Awake()
        {
            try
            {
                Instance = this;

                gameObject.hideFlags = HideFlags.HideAndDontSave;

                ConfigManager.Initialize(PatchInfo.ConfigFilePath);

                if (ConfigManager.EnableBetterExperience.Value)
                    Logger.LogWarning($"{nameof(BetterExperience)} Enabled!");
                else
                {
                    Logger.LogWarning($"{nameof(BetterExperience)} Disabled!");
                    return;
                }

                HLog.Initialize(
                    PatchInfo.LoggerPath,
                    PatchInfo.LoggerName,
                    new UnityProvider(),
                    new BepInExLoggerProvider(Logger),
                    ConfigManager.HLogLevel.Value,
                    ConfigManager.BepInExLogLevel.Value);

                HLog.Info($"{nameof(BetterExperience)} startup initialized. Version={PatchInfo.BepInPluginVersion}");

                TextureManager.Initialize(PatchInfo.ReplaceImagePath, PatchInfo.ReplaceSensitiveImagePath, PatchInfo.ReplaceImageSupportedExtensions);

                var harmony = new Harmony(PatchInfo.HarmonyPluginId);
                HLog.Debug($"Starting Harmony patch registration: {PatchInfo.HarmonyPluginId}");
                SafePatchAll(harmony, typeof(BetterExperience).Assembly);
                LogPatchesInfo(harmony);
                HLog.Info("Harmony patch registration completed.");
            }
            catch (Exception ex)
            {
                HLog.Error("Failed to patch", ex);
            }
        }

        void Update()
        {
            if (!ConfigManager.EnableBetterExperience.Value)
                return;

            DealInputReloadConfig();
        }

        private void DealInputReloadConfig()
        {
            if (ConfigManager.ReloadConfigHotkey.Value.WasPressedThisFrame())
            {
                try
                {
                    ConfigManager.ReloadConfig();
                    Logger.LogInfo("Reloaded config!");
                    HLog.Info("Reloaded config.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {name}.", ex);
                }
            }
        }

        public void SafePatchAll(Harmony harmony, Assembly asm)
        {
            int patchClassCount = 0;

            foreach (var t in GetTypesSafe(asm))
            {
                if (t == null)
                    continue;

                try
                {
                    if (!t.GetCustomAttributes(true).Any(a => a.GetType().Name.StartsWith("HarmonyPatch")))
                        continue;
                    harmony.CreateClassProcessor(t).Patch();
                    patchClassCount++;
                }
                catch (Exception ex)
                {
                    HLog.Error("Skip patch class: " + t.FullName, ex);
                }
            }

            HLog.Debug($"Scanned and processed Harmony patch classes: {patchClassCount}");
        }

        public IEnumerable<Type> GetTypesSafe(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                HLog.Warn($"ReflectionTypeLoadException occurred while enumerating assembly types: {asm.FullName}");
                return e.Types.Where(t => t != null);
            }
        }

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
                if (info == null) continue;

                patchedMethodCount++;
                HLog.Debug($"Original: {original.DeclaringType.FullName}.{original.Name}");

                CountList(info.Prefixes, ref prefixCount);
                CountList(info.Postfixes, ref postfixCount);
                CountList(info.Transpilers, ref transpilerCount);
                CountList(info.Finalizers, ref finalizerCount);

                void CountList(IList<Patch> patches, ref int count)
                {
                    if (patches == null) return;
                    for (int i = 0; i < patches.Count; i++)
                    {
                        var p = patches[i];
                        if (p.owner != id) continue;
                        count++;
                        HLog.Debug($"  {p.PatchMethod.DeclaringType.FullName}.{p.PatchMethod.Name} ({p.PatchMethod.MetadataToken:X8})");
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
