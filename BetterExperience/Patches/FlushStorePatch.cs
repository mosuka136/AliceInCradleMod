using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        private class FlushStorePatch
        {
            private static HotkeyInputSystem _flushStoreHotkey;

            [HarmonyPatch]
            private class FrameUpdateBoosterPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
                private static void Postfix()
                {
                    FrameUpdateBooster.Instance.OnFrameUpdate += Update;

                    ConfigManager.FlushAllStoreHotkey.OnValueChanged += (s, e) =>
                    {
                        _flushStoreHotkey = null;
                    };
                }
            }

            public static void Update()
            {
                if (!ConfigManager.EnableFlushAllStore.Value)
                    return;

                if (_flushStoreHotkey == null)
                {
                    var h = ConfigManager.FlushAllStoreHotkey.Value;
                    if (!HotkeyInputSystem.TryParse(h, out _flushStoreHotkey))
                    {
                        HLog.Warn("Invalid Hotkey: " + h);

                        h = "F";
                        HotkeyInputSystem.TryParse(h, out _flushStoreHotkey);
                        HLog.Info("Flush store hotkey set: " + h);
                    }
                }

                if (_flushStoreHotkey != null && _flushStoreHotkey.IsValid && _flushStoreHotkey.WasPressedThisFrame())
                {
                    try
                    {
                        StoreManager.FlushAll();
                    }
                    catch (Exception ex)
                    {
                        HLog.Error("FlushAllStore failed!", ex);
                    }

                    HLog.Warn("Flushed all store!");
                }
            }
        }
    }
}
