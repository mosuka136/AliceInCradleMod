using BepInEx;
using HarmonyLib;
using nel;
using System.Collections.Generic;
using UnityEngine;
using XX;

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
    internal class BetterExperience : BaseUnityPlugin
    {
        private HotkeyInputSystem _hotkey;

        public static BetterExperience Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            gameObject.hideFlags = HideFlags.HideAndDontSave;

            ConfigManager.Initialization();

            if (ConfigManager.EnableBetterExperience.Value)
                Logger.LogWarning($"{nameof(BetterExperience)} Enabled!");
            else
            {
                Logger.LogWarning($"{nameof(BetterExperience)} Disabled!");
                return;
            }

            HLog.Initialize(
                System.IO.Path.Combine(Paths.PluginPath, nameof(BetterExperience), "logs"),
                PatchInfo.HarmonyLoggerName,
                Logger);

            var harmony = new Harmony(PatchInfo.HarmonyPluginId);
            harmony.PatchAll(typeof(BetterExperience).Assembly);
            LogPatchesInfo(harmony);
        }

        void Update()
        {
            if (!ConfigManager.EnableBetterExperience.Value)
                return;

            if(!ConfigManager.EnableFlushAllStore.Value)
                return;

            if (_hotkey == null)
            {
                var h = ConfigManager.FlushAllStoreHotkey.Value;
                if (!HotkeyInputSystem.TryParse(h, out _hotkey))
                {
                    Logger.LogWarning("Invalid Hotkey: " + h);

                    h = "F";
                    HotkeyInputSystem.TryParse(h, out _hotkey);
                }
                Logger.LogInfo("Flush store hotkey set: " + h);
            }
            if (_hotkey != null && _hotkey.IsValid && _hotkey.WasPressedThisFrame())
            {
                Patchs.FlushStorePatch.Flush();
                Logger.LogInfo("Flushed all store!");
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
