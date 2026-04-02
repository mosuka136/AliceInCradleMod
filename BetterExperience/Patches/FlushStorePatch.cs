using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        public class FlushStorePatch
        {
            private static HotkeyInputSystem _flushStoreHotkey;

            [HarmonyPatch]
            public class FrameUpdateBoosterPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
                public static void Postfix()
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
