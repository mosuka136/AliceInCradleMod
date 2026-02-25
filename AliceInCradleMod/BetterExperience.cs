using BepInEx;
using BetterExperience.BepConfigManager;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterExperience
{
    internal sealed class PatchInfo
    {
        public const string BepInPluginId = "com.buele.betterexperience";
        public const string BepInPluginVersion = "1.0.0";

        public const string HarmonyPluginId = "com.buele.betterexperience";
        public const string HarmonyPluginVersion = "1.0.0";

        public const string HarmonyLoggerName = "BetterExperience_Harmony.log";
    }

    [BepInPlugin(PatchInfo.BepInPluginId, nameof(BetterExperience), PatchInfo.BepInPluginVersion)]
    internal sealed class BetterExperience : BaseUnityPlugin
    {
        private HotkeyInputSystem _reloadConfigHotkey;

        public static BetterExperience Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            gameObject.hideFlags = HideFlags.HideAndDontSave;

            ConfigManager.Initialize();

            if (ConfigManager.EnableBetterExperience.Value)
                Logger.LogWarning($"{nameof(BetterExperience)} Enabled!");
            else
            {
                Logger.LogWarning($"{nameof(BetterExperience)} Disabled!");
                return;
            }

            HLog.Initialize(
                Path.Combine(Paths.PluginPath, nameof(BetterExperience), "logs"),
                PatchInfo.HarmonyLoggerName,
                Logger,
                ConfigManager.HarmonyLogLevel.Value,
                ConfigManager.BepInExLogLevel.Value);

            var harmony = new Harmony(PatchInfo.HarmonyPluginId);
            try
            {
                SafePatchAll(harmony, typeof(BetterExperience).Assembly);
                LogPatchesInfo(harmony);
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
            if (_reloadConfigHotkey == null)
            {
                var h = ConfigManager.ReloadConfigHotkey.Value;
                if (!HotkeyInputSystem.TryParse(h, out _reloadConfigHotkey))
                {
                    Logger.LogWarning("Invalid Hotkey: " + h);
                    h = "Ctrl+R";
                    HotkeyInputSystem.TryParse(h, out _reloadConfigHotkey);
                }
                Logger.LogInfo("Reload config hotkey set: " + h);
            }

            if (_reloadConfigHotkey != null && _reloadConfigHotkey.IsValid && _reloadConfigHotkey.WasPressedThisFrame())
            {
                ConfigManager.ReloadConfig();
                Logger.LogInfo("Reloaded config!");

                _reloadConfigHotkey = null;
            }
        }

        public void SafePatchAll(Harmony harmony, Assembly asm)
        {
            foreach (var t in GetTypesSafe(asm))
            {
                if (t == null)
                    continue;

                bool isPatchClass = false;
                try
                {
                    isPatchClass = t.GetCustomAttributes(true)
                        .Any(a => a.GetType().Name.StartsWith("HarmonyPatch"));
                }
                catch (Exception ex)
                {
                    HLog.Error("Skip type (attribute load failed): " + t.FullName, ex);
                    continue;
                }

                if (!isPatchClass) continue;

                try
                {
                    harmony.CreateClassProcessor(t).Patch();
                }
                catch (Exception ex)
                {
                    HLog.Error("Skip patch class: " + t.FullName, ex);
                }
            }
        }

        private IEnumerable<Type> GetTypesSafe(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }


        private void LogPatchesInfo(Harmony harmony)
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
                HLog.Info($"Original: {original.DeclaringType.FullName}.{original.Name}");

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
                        HLog.Info($"  {p.PatchMethod.DeclaringType.FullName}.{p.PatchMethod.Name} ({p.PatchMethod.MetadataToken:X8})");
                    }
                }
            }

            HLog.Info($"Total patched methods: {patchedMethodCount}");
            HLog.Info($"Total prefixes: {prefixCount}");
            HLog.Info($"Total postfixes: {postfixCount}");
            HLog.Info($"Total transpilers: {transpilerCount}");
            HLog.Info($"Total finalizers: {finalizerCount}");
            HLog.Info($"Total patch methods: {prefixCount + postfixCount + transpilerCount + finalizerCount}");
            HLog.WriteLine();
        }
    }
}
